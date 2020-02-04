namespace ChangeLogCreator

open System
open System.Text

module MessageParser =

    type LineToken =
        | Line of string
        | Blank
        | Eof

    let readLines (input:string) =
        seq {

            let getStringTokenAndReset (stringBuilder: StringBuilder) =
                let value = stringBuilder.ToString()
                stringBuilder.Clear() |> ignore
                (Line value)

            let mutable i = 0
            let currentValueBuilder = StringBuilder()

            while i < input.Length  do
                let nextChar =  if i + 1 < input.Length then Some input.[i+1] else None

                match input.[i],nextChar with
                    | '\r', Some '\n' ->
                        if currentValueBuilder.Length = 0 then
                            yield Blank
                        else
                            yield getStringTokenAndReset currentValueBuilder
                        // next already matched => skip next iteration
                        i <- i + 1
                    | '\n', _ ->
                        if currentValueBuilder.Length = 0 then
                            yield Blank
                        else
                            yield getStringTokenAndReset currentValueBuilder
                    | _ ->  currentValueBuilder.Append(input.[i]) |> ignore

                i <- i + 1

            // if any input is left in currentValueBuilder, return it as StringToken
            if currentValueBuilder.Length > 0 then
                yield getStringTokenAndReset currentValueBuilder

            yield Eof
        }

    type ParserResult =
        | Parsed of ConventionalCommit
        | Failed of string               //TODO: Add union type for parser errors

    type internal ParserState<'a> = {
        CurrentResult : ParserResult;
        UnparsedTokens : 'a list
    }

    type internal HeaderToken =
        | String of string     // any string value
        | OpenParenthesis     // '('
        | CloseParenthesis     // ')'
        | Colon                // ':'
        | Space                // ' '
        | ExclamationMark      // '!'
        | Eof                  // end of input / last token

    type internal FooterToken =
        | String of string     // any string value
        | Colon                // ':'
        | Space                // ' '
        | Hash                 // '#'
        | Eof                  // end of input / last token


    let parse (input: string) : ParserResult =

        let joinString (separator:string) (values : string seq) = String.Join(separator, values |> Array.ofSeq)
        
        let isLineTokenType (left:LineToken) (right:LineToken) : bool =
            match left,right with
                | Line _,Line _ -> true
                | Blank,Blank -> true
                | LineToken.Eof,LineToken.Eof -> true
                | _ -> false

        let testToken2 (predicate:'tokenType -> bool) (state:ParserState<'tokenType>) : bool =
            match state.UnparsedTokens with
                | head::_ -> predicate head
                | _ -> false

        let testToken (expected:'tokenType) (state:ParserState<'tokenType>) : bool =
            match state.UnparsedTokens with
                | head::_ -> head = expected
                | _ -> false

        let matchToken (expected:'a) (state:ParserState<'a>) : ParserState<'a> =
            if testToken expected state then
                { state with UnparsedTokens = state.UnparsedTokens |> List.skip 1 }
            else
                { state with CurrentResult = Failed (sprintf "Expected token %A" expected ) }

        let withErrorCheck parseFunction state =
              match state.CurrentResult with
                  | Failed _ -> state
                  | _ -> parseFunction state

        let updateResult (mapper: ConventionalCommit -> 'a -> ConventionalCommit) (state : ParserResult) (param: 'a): ParserResult =
            let currentCommit = match state with
                                | Parsed d -> d
                                | Failed _ -> raise (InvalidOperationException "Data from invalid ParseResult requested. 'updateParsed' must not be called without error checking")
            let newCommit = mapper currentCommit param
            Parsed newCommit

        let lineTokenToString token =
              match token with
                  | LineToken.Line str -> str
                  | LineToken.Blank -> Environment.NewLine
                  | LineToken.Eof -> ""

        let tokenizeFooter (input:string) : FooterToken seq =
            seq {

                let getStringTokenAndReset (stringBuilder: StringBuilder) =
                    let value = stringBuilder.ToString()
                    stringBuilder.Clear() |> ignore
                    (FooterToken.String value)

                let currentValueBuilder = StringBuilder()

                let tryGetHeaderToken char =
                    match char with
                        | ':' -> Some FooterToken.Colon
                        | ' ' -> Some FooterToken.Space
                        | '#' -> Some FooterToken.Hash
                        | _ -> None

                if input.Length > 0 then
                    for i = 0 to input.Length - 1 do
                        match tryGetHeaderToken input.[i] with
                            | Some matchedCharToken ->
                                if currentValueBuilder.Length > 0 then yield getStringTokenAndReset currentValueBuilder
                                yield matchedCharToken
                            | None ->  currentValueBuilder.Append(input.[i]) |> ignore

                    // if any input is left in currentValueBuilder, return it as StringToken
                    if currentValueBuilder.Length > 0 then
                        yield getStringTokenAndReset currentValueBuilder

                yield FooterToken.Eof
            }

        let parseHeader (state:ParserState<LineToken>) : ParserState<LineToken> =

            let tokenizeHeader (input:string) : HeaderToken seq =
                seq {

                    let getStringTokenAndReset (stringBuilder: StringBuilder) =
                        let value = stringBuilder.ToString()
                        stringBuilder.Clear() |> ignore
                        (HeaderToken.String value)

                    let currentValueBuilder = StringBuilder()

                    let tryGetHeaderToken char =
                        match char with
                            | '(' -> Some HeaderToken.OpenParenthesis
                            | ')' -> Some HeaderToken.CloseParenthesis
                            | ':' -> Some HeaderToken.Colon
                            | ' ' -> Some HeaderToken.Space
                            | '!' -> Some HeaderToken.ExclamationMark
                            | _ -> None

                    if input.Length > 0 then
                        for i = 0 to input.Length - 1 do
                            match tryGetHeaderToken input.[i] with
                                | Some matchedCharToken ->
                                    if currentValueBuilder.Length > 0 then yield getStringTokenAndReset currentValueBuilder
                                    yield matchedCharToken
                                | None ->  currentValueBuilder.Append(input.[i]) |> ignore

                        // if any input is left in currentValueBuilder, return it as StringToken
                        if currentValueBuilder.Length > 0 then
                            yield getStringTokenAndReset currentValueBuilder

                    yield HeaderToken.Eof
                }

            let tokenToString token =
                match token with
                    | HeaderToken.Colon -> ":"
                    | HeaderToken.ExclamationMark -> "!"
                    | HeaderToken.OpenParenthesis -> "("
                    | HeaderToken.CloseParenthesis -> ")"
                    | HeaderToken.Space -> " "
                    | HeaderToken.String str ->  str
                    | HeaderToken.Eof -> ""

            let matchString (onMatch: ParserResult -> string -> ParserResult) (state: ParserState<HeaderToken>) =
                match state.UnparsedTokens with
                    | (HeaderToken.String str)::tail ->  { CurrentResult = (onMatch state.CurrentResult str); UnparsedTokens = tail }
                    | _ -> { state with CurrentResult = (Failed "Expected string token") }

            let parseType state = matchString (updateResult (fun commit newType -> { commit with Type = newType }) ) state

            let parseScope state =
                if testToken OpenParenthesis state then
                    state
                        |> withErrorCheck (matchToken OpenParenthesis)
                        |> withErrorCheck (matchString (updateResult (fun commit newScope -> { commit with Scope = Some newScope } ) ) )
                        |> withErrorCheck (matchToken CloseParenthesis)
                else
                    state

            let parseBreakingChange state =
                if (testToken ExclamationMark state) then
                    { CurrentResult = (updateResult (fun commit isBreakingChange -> { commit with IsBreakingChange = isBreakingChange }) state.CurrentResult true);
                      UnparsedTokens = state.UnparsedTokens |> List.skip 1 }
                else
                    state

            //TODO
            let parseDescription (state:ParserState<HeaderToken>) : ParserState<HeaderToken> =
                let tokens =  state.UnparsedTokens |> List.takeWhile (fun t -> t <> HeaderToken.Eof)
                let description = tokens
                                    |> Seq.map tokenToString
                                    |> joinString ""

                if String.IsNullOrWhiteSpace description then
                    { state with CurrentResult = Failed "Failed to parse header: Description must not be empty" }
                else
                    let remainingTokens = state.UnparsedTokens |> List.skip tokens.Length
                    let newResult = updateResult (fun c x -> { c with Description = x }) state.CurrentResult description
                    matchToken HeaderToken.Eof { CurrentResult = newResult; UnparsedTokens = remainingTokens }

            match state.UnparsedTokens with
                | (Line str)::tail ->
                    let headerParserState =
                        {  CurrentResult = state.CurrentResult; UnparsedTokens = tokenizeHeader str  |> List.ofSeq }
                            |> withErrorCheck parseType
                            |> withErrorCheck parseScope
                            |> withErrorCheck parseBreakingChange
                            |> withErrorCheck (matchToken HeaderToken.Colon)
                            |> withErrorCheck (matchToken HeaderToken.Space)
                            |> withErrorCheck parseDescription

                    match headerParserState.CurrentResult with
                        | Parsed p -> { CurrentResult = Parsed p ; UnparsedTokens = tail }
                        | Failed f -> { state with CurrentResult = Failed f }
                | _ -> { state with CurrentResult = (Failed (sprintf "Unexpected token %A" state.UnparsedTokens.Head ) ) }


        let testFooterStart (state:ParserState<LineToken>) : bool =
            match state.UnparsedTokens with
                | (LineToken.Line str)::_ -> 
                    let tokens = tokenizeFooter str |> List.ofSeq
                    match tokens with 
                        | FooterToken.String "BREAKING"::FooterToken.Space::FooterToken.String "CHANGE"::FooterToken.Colon::FooterToken.Space::_ -> true
                        | FooterToken.String "BREAKING"::FooterToken.Space::FooterToken.String "CHANGE"::FooterToken.Space::FooterToken.Hash::_ -> true
                        | FooterToken.String _::FooterToken.Colon::FooterToken.Space::_ -> true
                        | FooterToken.String _::FooterToken.Space::FooterToken.Hash::_ -> true
                        | _ -> false
                | _ -> false

        let rec parseBody (state:ParserState<LineToken>) : ParserState<LineToken> =

            let parseParagraph (state:ParserState<LineToken>) =
                
                if testToken2 (isLineTokenType (Line"")) state then

                    let lines = state.UnparsedTokens |> List.takeWhile (fun line -> line <> LineToken.Eof && line <> LineToken.Blank)
                    if lines |> Seq.length > 0 then
                        let text = lines
                                    |> Seq.map lineTokenToString
                                    |> joinString Environment.NewLine
                        let unparsedTokens =  state.UnparsedTokens |> List.skip lines.Length
                        { CurrentResult = ( (updateResult (fun commit paragraph -> { commit with Body = commit.Body @[paragraph] } )) state.CurrentResult (Paragraph text) );
                          UnparsedTokens = unparsedTokens
                        }
                    else
                        state
                else
                    { state with CurrentResult = ( Failed "Failed to parse body: Expected 'Line' token") }

            if testToken Blank state then
                let newState = state |> (matchToken Blank)
                if(testFooterStart newState) then
                    state
                else
                    newState
                        |> withErrorCheck parseParagraph
                        |> withErrorCheck parseBody
            else
                state
        
        let parseFooters (state:ParserState<LineToken>) : ParserState<LineToken> = 

            let tokenToString token = 
                match token with
                    | FooterToken.String str -> str
                    | FooterToken.Colon -> ":"
                    | FooterToken.Space -> " "
                    | FooterToken.Hash -> "#"
                    | FooterToken.Eof -> ""

            let parseFooterTokens (state:ParserState<FooterToken>) : ParserState<FooterToken> = 
                match state.UnparsedTokens with
                    | FooterToken.String "BREAKING"::FooterToken.Space::FooterToken.String "CHANGE"::FooterToken.Colon::FooterToken.Space::tail 
                    | FooterToken.String "BREAKING"::FooterToken.Space::FooterToken.String "CHANGE"::FooterToken.Space::FooterToken.Hash::tail ->
                        let tokens =  tail |> List.takeWhile (fun t -> t <> FooterToken.Eof)
                        let description = tokens
                                            |> List.map tokenToString
                                            |> joinString ""

                        let remainingTokens = tail |> List.skip tokens.Length
                        if tokens.Length = 0 || String.IsNullOrWhiteSpace(description) then
                            { state with CurrentResult = Failed "Failed to parse footer: Description must not be empty"}
                        else
                            let footer = { Type = "BREAKING CHANGE"; Description = description } 
                            { state with UnparsedTokens = remainingTokens ; CurrentResult = (updateResult (fun c footer -> { c with Footers = c.Footers @ [ footer ]}) state.CurrentResult footer )}    
                                |> matchToken Eof
                    | FooterToken.String footerType::FooterToken.Colon::FooterToken.Space::tail 
                    | FooterToken.String footerType::FooterToken.Space::FooterToken.Hash::tail ->                    
                        
                        let tokens =  tail |> List.takeWhile (fun t -> t <> FooterToken.Eof)
                        let description = tokens
                                            |> List.map tokenToString
                                            |> joinString ""

                        let remainingTokens = tail |> List.skip tokens.Length
                        if tokens.Length = 0 || String.IsNullOrWhiteSpace(description) then
                            { state with CurrentResult = Failed "Failed to parse footer: Description must not be empty"}
                        else
                            let footer = { Type = footerType; Description = description } 
                            { state with UnparsedTokens = remainingTokens ; CurrentResult = (updateResult (fun c footer -> { c with Footers = c.Footers @ [ footer ]}) state.CurrentResult footer )}    
                                |> matchToken Eof                            
                    | _ -> { state with CurrentResult = Failed "Expected footer"}

            let rec parseSingleFooter (state:ParserState<LineToken>) : ParserState<LineToken> =
                let newState = match state.UnparsedTokens with
                                | (LineToken.Line value)::tail -> 
                                    let footerTokens = tokenizeFooter value |> List.ofSeq
                                    let innerParserState = 
                                        { CurrentResult = state.CurrentResult; UnparsedTokens = footerTokens }
                                            |> parseFooterTokens
                                    match innerParserState.CurrentResult with 
                                        | (Parsed commit) -> { state with CurrentResult = Parsed commit; UnparsedTokens = tail }
                                        | (Failed err) -> { state with CurrentResult = (Failed err) }
                                | _ ->  { state with CurrentResult = Failed (sprintf "Unexpected token %A" state.UnparsedTokens.Head )  }

                if (testToken LineToken.Blank newState || testToken LineToken.Eof newState) then 
                    newState
                else
                    withErrorCheck parseSingleFooter newState

            if testToken Blank state then
                let newState = state |> (matchToken Blank)
                newState 
                    |> parseSingleFooter
            else
                state


        let lines = input |> readLines |> List.ofSeq
        let initalParserState = {
            CurrentResult = Parsed { Type = ""; Scope = None; Description = ""; IsBreakingChange = false; Body = []; Footers = [] };
            UnparsedTokens = lines
        }
        let result =
            initalParserState
                |> withErrorCheck parseHeader
                |> withErrorCheck parseBody
                |> withErrorCheck parseFooters
                |> withErrorCheck (matchToken LineToken.Eof)
        result.CurrentResult