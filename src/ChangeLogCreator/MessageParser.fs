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
        | LineBreakToken            // '\r\n' or '\n'
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
                        yield LineBreakToken
                        // next already matched => skip next iteration
                        i <- i + 1
                    | '\n', _, _ -> 
                        if currentValueBuilder.Length > 0 then
                            yield getStringTokenAndReset currentValueBuilder
                        yield LineBreakToken
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


    // 
    // Parser
    //
    type ParseError =
        | EmptyInput
        | UnspecifiedError
        | UnexpectedToken of Token * Token

    type ConventionalCommit = {
        Type : string
        Scope : string option
        Description : string
    }
    
    type ParseResult =
        | Parsed of ConventionalCommit
        | Failed of ParseError

    type private ParseState =
        | Error of ParseError
        | Data of ConventionalCommit

    let parse (input: string) : ParseResult =                 

        //
        // parsing helper functions
        //
        let checkForError parseLogic (state:ParseState) (tokens: Token list) : ParseState * (Token list)  = 
            match state with 
                | Error parseError -> Error parseError,tokens
                | _ -> parseLogic state tokens
           
        // matching a single token
        let matchToken expectedToken state tokens : ParseState * (Token list) =
            match tokens with
                | head::tail ->
                    if head = expectedToken then
                        state,tail
                    else
                        Error (UnexpectedToken (head,expectedToken)),tokens
                | [] -> Error EmptyInput,tokens
        
        // matches a string token and updates the parsed data using the specified function
        let matchString dataUpdater state tokens = 
            match tokens with
                | head::tail -> 
                    match head with 
                        | StringToken strValue -> 
                            let currentData = 
                                match state with 
                                    | Data d -> d
                                    | _ -> raise (InvalidOperationException "Data from invalid ParseState requested. 'matchString' should not be called without error checking")
                            let newData = dataUpdater currentData strValue
                            Data newData,tail
                        | t -> Error (UnexpectedToken (t, StringToken "")),tokens
                | [] -> Error EmptyInput,tokens

        let testToken (tokens: Token list) (expectedToken:Token) =
            match tokens with
                | head::tails -> head = expectedToken                    
                | _ -> false
        //
        // high-level parsing functions
        //
        let ensureNotEmpty state tokens =
                match tokens with
                    | head::tail ->
                        match head with
                            | EofToken -> Error EmptyInput,tokens
                            | _ -> state,tokens
                    | _ -> state, tokens

        let parseType  = checkForError (matchString (fun data value -> { data with Type = value }) )

        let parseDescription = checkForError (matchString (fun data value -> { data with Description = value }))

        let parseToken token = checkForError (matchToken token)

        let parseScope state tokens : ParseState * (Token list) =
            let doParseScope (state:ParseState) (tokens: Token list) : ParseState * (Token list) = 
                if (testToken tokens OpenParenthesisToken) then                                    
                        (state,tokens)
                            ||> matchToken OpenParenthesisToken
                            ||> matchString (fun data value -> { data with Scope = Some value })
                            ||> matchToken CloseParenthesisToken                
                else                
                    state,tokens
            checkForError doParseScope state tokens
           

        let tokens = tokenize input |> List.ofSeq
        let emptyData = Data { Type = ""; Scope = None; Description = "" }

        let result,_ = 
            (emptyData,tokens)
                ||> ensureNotEmpty
                ||> parseType
                ||> parseScope
                ||> parseToken ColonAndSpaceToken
                ||> parseDescription
                ||> parseToken EofToken
                           
        match result with
            | Error error -> Failed error
            | Data data -> Parsed data

        