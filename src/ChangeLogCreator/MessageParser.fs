namespace ChangeLogCreator

open System
open System.Text

module MessageParser =
               
    //
    // Tokenizer 
    //

    type Token =
        | StringToken of string     // any string value
        | OpenParenthesisToken      // '('
        | CloseParenthesisToken     // ')'
        | ColonAndSpaceToken        // ': '
        | LineBreakToken of string  // '\r\n' or '\n'
        | ExclamationMarkToken      // '!'
        | SpaceAndHashToken         // ' #'
        | EofToken                  // end of input / last token
    
    let tokenize (input: string) =
        seq {

            let getStringTokenAndReset (stringBuilder: StringBuilder) =
                let value = stringBuilder.ToString()
                stringBuilder.Clear() |> ignore
                (StringToken value)

            let currentValueBuilder = StringBuilder()

            let getSingleCharToken char =
                match char with 
                    | '(' -> Some OpenParenthesisToken
                    | ')' -> Some CloseParenthesisToken
                    | '!' -> Some ExclamationMarkToken
                    | _ -> None
           
            let mutable i = 0
            while i < input.Length  do
                let nextChar =  if i + 1 < input.Length then Some input.[i+1] else None
                let currentCharToken = getSingleCharToken input.[i]                

                match input.[i], nextChar, currentCharToken with                    
                    | _, _, Some matchedCharToken ->
                        if currentValueBuilder.Length > 0 then
                            yield getStringTokenAndReset currentValueBuilder
                        yield matchedCharToken
                    | '\r', Some '\n', _ ->
                        if currentValueBuilder.Length > 0 then
                            yield getStringTokenAndReset currentValueBuilder
                        yield LineBreakToken "\r\n"
                        // next already matched => skip next iteration
                        i <- i + 1
                    | '\n', _, _ -> 
                        if currentValueBuilder.Length > 0 then
                            yield getStringTokenAndReset currentValueBuilder
                        yield LineBreakToken "\n"
                    | ':', (Some ' '), _ ->
                        if currentValueBuilder.Length > 0 then
                            yield getStringTokenAndReset currentValueBuilder
                        yield ColonAndSpaceToken   
                        // next already matched => skip next iteration
                        i <- i + 1
                    | ' ', (Some '#'), _ ->
                        if currentValueBuilder.Length > 0 then
                            yield getStringTokenAndReset currentValueBuilder
                        yield SpaceAndHashToken   
                        // next already matched => skip next iteration
                        i <- i + 1
                    | _ ->  currentValueBuilder.Append(input.[i]) |> ignore

                i <- i + 1

            // if any input is left in currentValueBuilder, return it as StringToken
            if currentValueBuilder.Length > 0 then
                yield getStringTokenAndReset currentValueBuilder

            yield EofToken
        }

    let tokenToString (token : Token) : string =
        match token with 
            | StringToken str -> str
            | OpenParenthesisToken -> "("
            | CloseParenthesisToken -> ")"
            | ColonAndSpaceToken -> ": "
            | LineBreakToken lb -> lb
            | ExclamationMarkToken -> "!"
            | SpaceAndHashToken -> " #"
            | EofToken -> ""

    /// <summary>
    /// Tests if the two specified tokens are of the same token type. 
    /// For <see cref="StringToken" /> and <see cref="LineBreakToken" /> the content is ignored for the comparison
    /// </summary>
    let internal isTokenType expected actual =
        match expected with
            | StringToken _ -> 
                match actual with
                    | StringToken _ -> true
                    | _ -> false
            | LineBreakToken _ ->
                match actual with
                    |LineBreakToken _ -> true
                    | _ -> false
            | OpenParenthesisToken -> expected = actual
            | CloseParenthesisToken -> expected = actual
            | ColonAndSpaceToken -> expected = actual
            | ExclamationMarkToken -> expected = actual
            | SpaceAndHashToken -> expected = actual
            | EofToken -> expected = actual

    // 
    // Parser
    //
    type ParseError =
        | EmptyInput
        | UnspecifiedError
        | EmptyText
        | UnexpectedToken of Token

    type Paragraph = Paragraph of string
        
    type ConventionalCommit = {
        Type : string
        Scope : string option
        Description : string
        IsBreakingChange : bool
        Body : Paragraph list
    }
    
    type ParseResult =
        | Parsed of ConventionalCommit
        | Failed of ParseError


    type private ParserState = {
        CurrentResult : ParseResult
        UnparsedTokens : Token list
    }
    
    /// Parses the input string into a conventional commit message
    let parse (input: string) : ParseResult =                 

        //
        // parsing helper functions
        //
        let checkForError parseLogic (state:ParserState) : ParserState  = 
            match state.CurrentResult with 
                | Failed _ -> state
                | _ -> parseLogic state
           
        // matching a single token
        let matchToken predicate (state:ParserState) : ParserState =
            match state.UnparsedTokens with
                | head::tail ->
                    if predicate head then
                        { state with UnparsedTokens = tail }
                    else
                        { state with CurrentResult = Failed (UnexpectedToken head) }
                | [] -> state
        
        let updateParsed updateAction (result:ParseResult) value : ParseResult =
            let currentData = 
                match result with 
                    | Parsed d -> d
                    | _ -> raise (InvalidOperationException "Data from invalid ParseResult requested. 'updateParsed' must not be called without error checking")
            Parsed (updateAction currentData value)

        // matches a string token and updates the parsed data using the specified function
        let matchString dataUpdater (state:ParserState) = 
            match state.UnparsedTokens with
                | head::tail -> 
                    match head with 
                        | StringToken strValue -> { CurrentResult = (updateParsed dataUpdater state.CurrentResult strValue); 
                                                    UnparsedTokens = tail}
                        | t -> { state with CurrentResult = Failed (UnexpectedToken t) }
                | [] -> { state with CurrentResult = Failed EmptyInput }

        /// Tests if the first token of the specified token list matches the specified predicate
        let testToken (tokens: Token list) (predicate: Token -> bool) : bool =
            match tokens with
                | head::_ -> predicate head
                | [] -> false

        /// Tests if the first and second token of the specified token list match the specified predicate
        let testToken2 (tokens: Token list) (predicate: Token -> bool) : bool =
            match tokens with
                | first::second::_ -> predicate first && predicate second
                | _ -> false


        let matchTextLineWhile allowEmpty predicate dataUpdater (state:ParserState) =
            let processableTokens =  state.UnparsedTokens |> ListEx.takeWhile predicate
            if ((not allowEmpty) && List.isEmpty processableTokens) then
                { state with CurrentResult = Failed EmptyText }
            else
                let strValue = 
                    processableTokens  
                    |> Seq.map tokenToString
                    |> Seq.reduce( fun a b -> a + b)                        
                { CurrentResult = (updateParsed dataUpdater state.CurrentResult strValue); 
                  UnparsedTokens = state.UnparsedTokens |> List.skip(Seq.length processableTokens) }
        
        //
        // high-level parsing functions
        //
        let ensureNotEmpty (state:ParserState) =
                match state.UnparsedTokens with
                    | head::_ ->
                        match head with
                            | EofToken -> { state with CurrentResult = Failed EmptyInput }
                            | _ -> state
                    | [] -> { state with CurrentResult = Failed EmptyInput }

        let parseType  = checkForError (matchString (fun data value -> { data with Type = value }))

        let parseDescription = 
            // match description until EOF or a line break
            let setDescription data value = { data with Description = value }
            let isDescriptionToken token _ = 
                match token with
                    | StringToken _  -> true
                    | OpenParenthesisToken -> true
                    | CloseParenthesisToken -> true
                    | ColonAndSpaceToken -> true
                    | ExclamationMarkToken -> true
                    | SpaceAndHashToken -> true
                    | EofToken -> false
                    | LineBreakToken _ -> false

            checkForError (matchTextLineWhile false isDescriptionToken setDescription)

        let parseToken token = checkForError (matchToken (isTokenType token))

        let parseScope (state:ParserState) : ParserState =
            let doParseScope (state:ParserState) : ParserState =
                if (testToken state.UnparsedTokens (isTokenType OpenParenthesisToken)) then                                    
                    state
                        |> matchToken (isTokenType OpenParenthesisToken)
                        |> matchString (fun data value -> { data with Scope = Some value })
                        |> matchToken (isTokenType CloseParenthesisToken)
                else
                    state
            checkForError doParseScope state
           
        let ignoreTrailingLineBreaks (state:ParserState) =
            let rec doIgnoreTrailingLineBreaks (state:ParserState) =
                match state.UnparsedTokens with
                    | head::tail ->
                        match head with 
                            | LineBreakToken _ -> doIgnoreTrailingLineBreaks { state with UnparsedTokens = tail }
                            | _ -> state
                    | [] -> state
            checkForError doIgnoreTrailingLineBreaks state

        let parseBreakingChange (state:ParserState) =
            if testToken state.UnparsedTokens (isTokenType ExclamationMarkToken) then              
              let newState = matchToken (isTokenType ExclamationMarkToken) state
              { newState with CurrentResult =  (updateParsed (fun d value -> { d with IsBreakingChange = value }) newState.CurrentResult true) }
            else
                state
        
        let parseParagraph (state:ParserState) =
            let continueParagraph head tail = 
                match head with 
                    | StringToken _ -> true
                    | OpenParenthesisToken -> true
                    | CloseParenthesisToken -> true
                    | ColonAndSpaceToken -> true
                    | ExclamationMarkToken -> true
                    | SpaceAndHashToken -> true
                    | EofToken -> false
                    // abort token when there is a blank line ( = double line break)
                    | LineBreakToken _ -> 
                        // LineBreak + EOF
                        if testToken tail (isTokenType EofToken) then
                            false
                        // LineBreak + LineBreak
                        elif (testToken tail (isTokenType (LineBreakToken ""))) then
                            false
                        else
                            true
                    
            let addParagraph commit newParagraph = { commit with Body = commit.Body @ [Paragraph(newParagraph)] }

            matchTextLineWhile true continueParagraph addParagraph state
            

        let parseBody (state:ParserState) =
            let isLineBreakToken = isTokenType (LineBreakToken "")
            // paragraphs always with a blank link (i.e. 2 line breaks)
            let mutable currentState = state
            while (testToken2 currentState.UnparsedTokens isLineBreakToken) do
                // match both tokens
                currentState <- matchToken isLineBreakToken currentState
                currentState <- matchToken isLineBreakToken currentState
                currentState <- parseParagraph currentState
            currentState

        let state  = 
             { CurrentResult = Parsed { Type = ""; Scope = None; Description = ""; IsBreakingChange = false; Body = [] };
               UnparsedTokens = tokenize input |> List.ofSeq } 
                |> ensureNotEmpty
                |> parseType
                |> parseScope
                |> parseBreakingChange
                |> parseToken ColonAndSpaceToken
                |> parseDescription
                |> parseBody
                |> ignoreTrailingLineBreaks
                |> parseToken EofToken
        state.CurrentResult

        