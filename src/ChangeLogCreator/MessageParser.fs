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

    // 
    // Parser
    //
    type ParseError =
        | EmptyInput
        | UnspecifiedError
        | EmptyText
        | UnexpectedToken of Token * Token

    type ConventionalCommit = {
        Type : string
        Scope : string option
        Description : string
        IsBreakingChange : bool
    }
    
    type ParseResult =
        | Parsed of ConventionalCommit
        | Failed of ParseError


    type private ParserState = {
        CurrentResult : ParseResult
        UnparsedTokens : Token list
    }

    let parse (input: string) : ParseResult =                 

        //
        // parsing helper functions
        //
        let checkForError parseLogic (state:ParserState) : ParserState  = 
            match state.CurrentResult with 
                | Failed parseError -> state
                | _ -> parseLogic state
           
        // matching a single token
        let matchToken expectedToken (state:ParserState) : ParserState =
            match state.UnparsedTokens with
                | head::tail ->
                    if head = expectedToken then
                        { state with UnparsedTokens = tail }
                    else
                        { state with CurrentResult = Failed (UnexpectedToken (head,expectedToken)) }
                | [] -> state
        
        let updateParsed dataUpdater (result:ParseResult) value : ParseResult =
            let currentData = 
                match result with 
                    | Parsed d -> d
                    | _ -> raise (InvalidOperationException "Data from invalid ParseResult requested. 'matchString' should not be called without error checking")
            Parsed (dataUpdater currentData value)

        // matches a string token and updates the parsed data using the specified function
        let matchString dataUpdater (state:ParserState) = 
            match state.UnparsedTokens with
                | head::tail -> 
                    match head with 
                        | StringToken strValue -> { CurrentResult = (updateParsed dataUpdater state.CurrentResult strValue); 
                                                    UnparsedTokens = tail}
                        | t -> { state with CurrentResult = Failed (UnexpectedToken (t, StringToken "")) }
                | [] -> { state with CurrentResult = Failed EmptyInput }

        let testToken (tokens: Token list) (expectedToken:Token) =
            match tokens with
                | head::_ -> head = expectedToken                    
                | [] -> false

        let matchTextLine dataUpdater (state:ParserState) =
            let processableTokens = 
                state.UnparsedTokens
                    |> List.takeWhile (fun token ->
                        match token with
                            | StringToken _  -> true
                            | OpenParenthesisToken -> true
                            | CloseParenthesisToken -> true
                            | ColonAndSpaceToken -> true
                            | ExclamationMarkToken -> true
                            | SpaceAndHashToken -> true
                            | EofToken -> false
                            | LineBreakToken _ -> false)

            if (List.isEmpty processableTokens) then
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

        let parseDescription = checkForError (matchTextLine (fun data value -> { data with Description = value }))

        let parseToken token = checkForError (matchToken token)

        let parseScope (state:ParserState) : ParserState =
            let doParseScope (state:ParserState) : ParserState =
                if (testToken state.UnparsedTokens OpenParenthesisToken) then                                    
                    state
                        |> matchToken OpenParenthesisToken
                        |> matchString (fun data value -> { data with Scope = Some value })
                        |> matchToken CloseParenthesisToken                
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
            if testToken state.UnparsedTokens ExclamationMarkToken then              
              let newState = matchToken ExclamationMarkToken state
              { newState with CurrentResult =  (updateParsed (fun d value -> { d with IsBreakingChange = value }) newState.CurrentResult true) }
            else
                state

        let state  = 
             { CurrentResult = Parsed { Type = ""; Scope = None; Description = ""; IsBreakingChange = false };
               UnparsedTokens = tokenize input |> List.ofSeq } 
                |> ensureNotEmpty
                |> parseType
                |> parseScope
                |> parseBreakingChange
                |> parseToken ColonAndSpaceToken
                |> parseDescription
                |> ignoreTrailingLineBreaks
                |> parseToken EofToken
        state.CurrentResult

        