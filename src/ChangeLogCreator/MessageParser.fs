namespace ChangeLogCreator

open System
open System.Text

module MessageParser =

    // Token types
    //
    type LineToken =
        | Line of string
        | Blank
        | Eof

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

    // Parser state and result types
    //
    type ParserResult =
        | Parsed of ConventionalCommit
        | Failed of string               //TODO: Add union type for parser errors

    type internal ParserState<'a> = {
        CurrentResult : ParserResult;
        UnparsedTokens : 'a list
    }

    // Helper methods
    //
    let internal joinString (separator:string) (values : string seq) = String.Join(separator, values |> Array.ofSeq)

    let internal lineTokenToString token =
          match token with
              | LineToken.Line str -> str
              | LineToken.Blank -> Environment.NewLine
              | LineToken.Eof -> ""

    let internal headerTokenToString token =
        match token with
            | HeaderToken.Colon -> ":"
            | HeaderToken.ExclamationMark -> "!"
            | HeaderToken.OpenParenthesis -> "("
            | HeaderToken.CloseParenthesis -> ")"
            | HeaderToken.Space -> " "
            | HeaderToken.String str ->  str
            | HeaderToken.Eof -> ""

    let internal footerTokenToString token = 
        match token with
            | FooterToken.String str -> str
            | FooterToken.Colon -> ":"
            | FooterToken.Space -> " "
            | FooterToken.Hash -> "#"
            | FooterToken.Eof -> ""

    let internal isLineTokenType (a:LineToken) (b:LineToken) : bool =
        match a, b with
            | LineToken.Line _, LineToken.Line _ -> true
            | LineToken.Blank,  LineToken.Blank  -> true
            | LineToken.Eof,    LineToken.Eof    -> true
            | _                                  -> false

    let internal isHeaderTokenType (a:HeaderToken) (b:HeaderToken) : bool =
        match a, b with
            | HeaderToken.String _,         HeaderToken.String _         -> true
            | HeaderToken.OpenParenthesis,  HeaderToken.OpenParenthesis  -> true
            | HeaderToken.CloseParenthesis, HeaderToken.CloseParenthesis -> true
            | HeaderToken.Colon,            HeaderToken.Colon            -> true
            | HeaderToken.Space,            HeaderToken.Space            -> true
            | HeaderToken.ExclamationMark,  HeaderToken.ExclamationMark  -> true
            | HeaderToken.Eof,              HeaderToken.Eof              -> true
            |_                                                           -> false

    let internal isFooterTokenType (a:FooterToken) (b:FooterToken) : bool = 
        match a, b with
            | FooterToken.String _, FooterToken.String _  -> true
            | FooterToken.Colon   , FooterToken.Colon     -> true       
            | FooterToken.Space   , FooterToken.Space     -> true
            | FooterToken.Hash    , FooterToken.Hash      -> true
            | FooterToken.Eof     , FooterToken.Eof       -> true
            | _                                           -> false

    // Tokenizer
    //
    let readLines (input:string) : LineToken seq =
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

            yield LineToken.Eof
        }


    // Parser
    //
    module internal Commit =
        let setType commit newType = { commit with ConventionalCommit.Type = newType }
        let setScope commit newScope = { commit with Scope = Some newScope }
        let setIsBreakingChange commit value = { commit with IsBreakingChange = value }
        let setDescription commit value = { commit with ConventionalCommit.Description = value }
        let addParagraph commit paragraph = { commit with Body = commit.Body @[ paragraph ] }
        let addFooter commit footer = { commit with Footers = commit.Footers @ [ footer ] }

    module internal Parser = 
        let fromResult currentResult tokens = { CurrentResult = currentResult; UnparsedTokens = tokens }

        let start tokens = {
                CurrentResult = Parsed { Type = ""; Scope = None; Description = ""; IsBreakingChange = false; Body = []; Footers = [] };
                UnparsedTokens = tokens }
        
        let internal failure error state = { state with CurrentResult = Failed error }

        let withErrorCheck parseFunction state =
              match state.CurrentResult with
                  | Failed _ -> state
                  | _ -> parseFunction state

        let testToken (expected:'tokenType) (state:ParserState<'tokenType>) : bool =
            match state.UnparsedTokens with
                | head::_ -> 
                    let foo = head = expected
                    foo
                | _ -> false

        let testToken2 predicate (state:ParserState<'tokenType>) : bool =
               match state.UnparsedTokens with
                   | head::_ -> predicate head
                   | _ -> false

        let matchToken (expected:'a) (state:ParserState<'a>) : ParserState<'a> =
            if testToken expected state then
                { state with UnparsedTokens = state.UnparsedTokens |> List.skip 1 }
            else
                failure (sprintf "Expected token %A" expected ) state

        let updateResult updater state param =
            let currentCommit = match state with
                                | Parsed d -> d
                                | Failed _ -> raise (InvalidOperationException "Data from invalid ParseResult requested. 'updateParsed' must not be called without error checking")
            let newCommit = updater currentCommit param
            Parsed newCommit

        let currentResult state = state.CurrentResult

        let parseLine parser state =
            match state.UnparsedTokens with 
                | Line str::tail -> 
                    let innerParserResult = parser state str 
                    match innerParserResult.CurrentResult with
                        | Parsed p -> { CurrentResult = Parsed p ; UnparsedTokens = tail }
                        | Failed f -> { state with CurrentResult = Failed f }
                | _ -> failure (sprintf "Unexpected token %A" state.UnparsedTokens.Head)  state

        let parseIf (predicate:ParserState<'a> -> bool) parser state = if predicate state then parser state else state

        let ref parseRec (predicate:ParserState<'a> -> bool) parser state = 
            let newState = parser state
            if predicate newState then
                parseRec predicate parser newState
            else
                newState

    let parse (message: string) : ParserResult =

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

        let parseHeaderLine (initialState:ParserState<LineToken>) (input:string) : ParserState<HeaderToken> =

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
           
            let matchString (onMatch: ParserResult -> string -> ParserResult) (state: ParserState<HeaderToken>) =
                match state.UnparsedTokens with
                    | (HeaderToken.String str)::tail ->  { CurrentResult = (onMatch state.CurrentResult str); UnparsedTokens = tail }
                    | _ -> Parser.failure "Expected string token" state

            let parseType state = matchString (Parser.updateResult Commit.setType) state

            let parseScope state =
                if Parser.testToken OpenParenthesis state then
                    state
                        |> Parser.withErrorCheck (Parser.matchToken OpenParenthesis)
                        |> Parser.withErrorCheck (matchString (Parser.updateResult Commit.setScope ))
                        |> Parser.withErrorCheck (Parser.matchToken CloseParenthesis)
                else
                    state

            let parseBreakingChange state =
                if (Parser.testToken ExclamationMark state) then
                    { CurrentResult = (Parser.updateResult Commit.setIsBreakingChange state.CurrentResult true);
                      UnparsedTokens = state.UnparsedTokens |> List.skip 1 }
                else
                    state
            
            let parseDescription (state:ParserState<HeaderToken>) : ParserState<HeaderToken> =
                let tokens =  state.UnparsedTokens |> List.takeWhile (fun t -> t <> HeaderToken.Eof)
                let description = tokens
                                    |> Seq.map headerTokenToString
                                    |> joinString ""

                if String.IsNullOrWhiteSpace description then
                    Parser.failure "Failed to parse header: Description must not be empty" state
                else
                    let remainingTokens = state.UnparsedTokens |> List.skip tokens.Length
                    let newResult = Parser.updateResult Commit.setDescription state.CurrentResult description
                    Parser.matchToken HeaderToken.Eof { CurrentResult = newResult; UnparsedTokens = remainingTokens }

            input 
                |> tokenizeHeader
                |> List.ofSeq
                |> Parser.fromResult initialState.CurrentResult
                |> Parser.withErrorCheck parseType
                |> Parser.withErrorCheck parseScope
                |> Parser.withErrorCheck parseBreakingChange
                |> Parser.withErrorCheck (Parser.matchToken HeaderToken.Colon)
                |> Parser.withErrorCheck (Parser.matchToken HeaderToken.Space)
                |> Parser.withErrorCheck parseDescription
                
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
                
                if Parser.testToken2 (isLineTokenType (Line"")) state then

                    let lines = state.UnparsedTokens |> List.takeWhile (fun line -> line <> LineToken.Eof && line <> LineToken.Blank)
                    if lines |> Seq.length > 0 then
                        let text = lines
                                    |> Seq.map lineTokenToString
                                    |> joinString Environment.NewLine
                        let unparsedTokens =  state.UnparsedTokens |> List.skip lines.Length
                        { CurrentResult = Parser.updateResult Commit.addParagraph state.CurrentResult (Paragraph text);
                          UnparsedTokens = unparsedTokens
                        }
                    else
                        state
                else
                    Parser.failure "Failed to parse body: Expected 'Line' token" state

            if Parser.testToken Blank state then
                let newState = state |> (Parser.matchToken Blank)
                if(testFooterStart newState) then
                    state
                else
                    newState
                        |> Parser.withErrorCheck parseParagraph
                        |> Parser.withErrorCheck parseBody
            else
                state
        
        
        let rec parseFooters (state:ParserState<LineToken>) : ParserState<LineToken> = 

            let parseFooterTokens (state:ParserState<FooterToken>) : ParserState<FooterToken> = 
                let footerType,descriptionTokens = 
                    match state.UnparsedTokens with
                        | String "BREAKING"::Space::String "CHANGE"::Colon::Space::tail 
                        | String "BREAKING"::Space::String "CHANGE"::Space::Hash::tail -> "BREAKING CHANGE",tail
                        | String footerType::Colon::Space::tail 
                        | String footerType::Space::Hash::tail -> footerType,tail
                        | _ -> "",[]

                let tokens =  descriptionTokens |> List.takeWhile (fun t -> t <> FooterToken.Eof)
                let description = tokens
                                    |> List.map footerTokenToString
                                    |> joinString ""

                let remainingTokens = descriptionTokens |> List.skip tokens.Length

                if tokens.Length = 0 || String.IsNullOrWhiteSpace(description) then
                    Parser.failure "Failed to parse footer: Description must not be empty" state
                else
                    let footer = { Type = footerType; Description = description } 
                    { UnparsedTokens = remainingTokens ;
                      CurrentResult = (Parser.updateResult Commit.addFooter state.CurrentResult footer ) }    
                        
            let parseFooterLine (initialState: ParserState<LineToken>) (input:string) : ParserState<FooterToken> = 
                input
                    |> tokenizeFooter
                    |> List.ofSeq
                    |> Parser.fromResult initialState.CurrentResult
                    |> Parser.withErrorCheck parseFooterTokens       
                    |> Parser.withErrorCheck (Parser.matchToken Eof)
                    

            let newState = Parser.parseLine parseFooterLine state

            if Parser.testToken2 (isLineTokenType (LineToken.Line "")) newState then
                Parser.withErrorCheck parseFooters newState
            else
                newState
            
        message
            |> readLines
            |> List.ofSeq
            |> Parser.start
            |> Parser.withErrorCheck (Parser.parseLine parseHeaderLine)
            |> Parser.withErrorCheck parseBody
            |> Parser.withErrorCheck (Parser.parseIf (Parser.testToken Blank) ((Parser.matchToken Blank) >> parseFooters))
            |> Parser.withErrorCheck (Parser.matchToken LineToken.Eof)
            |> Parser.currentResult