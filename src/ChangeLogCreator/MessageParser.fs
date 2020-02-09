namespace ChangeLogCreator

open System
open System.Text

module MessageParser =

    // Token types
    //
    type internal LineToken =
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
        | Eol                  // end of input / last token

    type internal FooterToken =
        | String of string     // any string value
        | Colon                // ':'
        | Space                // ' '
        | Hash                 // '#'
        | Eol                  // end of input / last token

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
            | HeaderToken.Eol -> ""

    let internal footerTokenToString token =
        match token with
            | FooterToken.String str -> str
            | FooterToken.Colon -> ":"
            | FooterToken.Space -> " "
            | FooterToken.Hash -> "#"
            | FooterToken.Eol -> ""

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
            | HeaderToken.Eol,              HeaderToken.Eol              -> true
            |_                                                           -> false

    let internal isFooterTokenType (a:FooterToken) (b:FooterToken) : bool =
        match a, b with
            | FooterToken.String _, FooterToken.String _  -> true
            | FooterToken.Colon   , FooterToken.Colon     -> true
            | FooterToken.Space   , FooterToken.Space     -> true
            | FooterToken.Hash    , FooterToken.Hash      -> true
            | FooterToken.Eol     , FooterToken.Eol       -> true
            | _                                           -> false

    // Tokenizer
    //

    /// <summary>
    /// Splits the input string into a sequence of <see cref="LineToken" />
    /// </summary>
    let internal readLines (input:string) : LineToken seq =
        seq {

            /// <summary>
            /// Resets the specified StringBuilder and returns its current value as <see cref="LineToken" />
            /// </summary>
            let getStringTokenAndReset (stringBuilder: StringBuilder) =
                let value = stringBuilder.ToString()
                stringBuilder.Clear() |> ignore
                (Line value)

            // iterate over the input string and split at line breaks.
            // Matches both \r\n and \n line breaks
            let currentValueBuilder = StringBuilder()
            let mutable i = 0
            while i < input.Length  do
                let nextChar =  if i + 1 < input.Length then Some input.[i+1] else None

                match input.[i],nextChar with
                    | '\r', Some '\n' ->
                        if currentValueBuilder.Length = 0 then
                            yield Blank
                        else
                            yield getStringTokenAndReset currentValueBuilder
                        i <- i + 1  // next already matched => skip next iteration
                    | '\n', _ ->
                        if currentValueBuilder.Length = 0 then
                            yield Blank
                        else
                            yield getStringTokenAndReset currentValueBuilder
                    | _ ->  currentValueBuilder.Append(input.[i]) |> ignore
                i <- i + 1

            // if any input is left in currentValueBuilder, return it as line
            if currentValueBuilder.Length > 0 then
                yield getStringTokenAndReset currentValueBuilder

            // yield Eof token to indicate end of input
            yield LineToken.Eof
        }


    // Parser
    //

    /// <summary>
    /// Provides helper method for changing data in a <see cref="ConventionalCommit" />
    /// </summary>
    module internal Commit =
        let setType (commit:ConventionalCommit) newType = { commit with ConventionalCommit.Type = newType }
        let setScope (commit:ConventionalCommit) newScope = { commit with Scope = Some newScope }
        let setIsBreakingChange (commit:ConventionalCommit) value = { commit with IsBreakingChange = value }
        let setDescription (commit:ConventionalCommit) value = { commit with ConventionalCommit.Description = value }
        let addParagraph (commit:ConventionalCommit) paragraph = { commit with Body = commit.Body @ [ paragraph ] }
        let addFooter (commit:ConventionalCommit) footer = { commit with Footers = commit.Footers @ [ footer ] }

    /// Provides helper methods for parsing
    module internal Parser =

        /// Creates a new ParserState from the specified result and tokens
        let fromResult currentResult tokens = { CurrentResult = currentResult; UnparsedTokens = tokens }

        /// Create a new ParserState with an "empty" ConventionalCommit and the specified tokens
        let start tokens = {
                CurrentResult = Parsed { Type = ""; Scope = None; Description = ""; IsBreakingChange = false; Body = []; Footers = [] };
                UnparsedTokens = tokens }

        /// Creates a "failed" ParserState from the specified state
        let internal failure error state = { state with CurrentResult = Failed error }

        /// Checks if the specified ParserState is in an error state and calls the specified parser function only if no error was found
        let withErrorCheck parseFunction state =
              match state.CurrentResult with
                  | Failed _ -> state
                  | _ -> parseFunction state

        // Checks if the specified parser state has a next token and the token matches the specified predicate
        let testToken (predicate: 'tokenType -> bool) (state:ParserState<'tokenType>) : bool =
               match state.UnparsedTokens with
                   | head::_ -> predicate head
                   | _ -> false

        // Checks if the specified parser state has a next token and the token is equal to the specified token
        let testTokenEq (expected:'tokenType) (state:ParserState<'tokenType>) : bool = testToken (fun t -> t = expected) state

        /// Removes the next token in the specified state if it is equal to the specified token.
        /// Otherwise returns an error ParserState
        let matchToken (expected:'a) (state:ParserState<'a>) : ParserState<'a> =
            if testTokenEq expected state then
                { state with UnparsedTokens = state.UnparsedTokens |> List.skip 1 }
            else
                failure (sprintf "Expected token %A" expected ) state

        /// Updates the parsed data in the specified parser result using the specified update function
        let updateResult updater state param =
            let currentCommit = match state with
                                | Parsed d -> d
                                | Failed _ -> raise (InvalidOperationException "Data from invalid ParseResult requested. 'updateParsed' must not be called without error checking")
            let newCommit = updater currentCommit param
            Parsed newCommit

        /// Parses the content of a LineToken.Line value using the specified parser function
        let parseLine parser state =
            match state.UnparsedTokens with
                | Line str::tail ->
                    let innerParserResult = parser state str
                    match innerParserResult.CurrentResult with
                        | Parsed p -> { CurrentResult = Parsed p ; UnparsedTokens = tail }
                        | Failed f -> { state with CurrentResult = Failed f }
                | _ -> failure (sprintf "Unexpected token %A" state.UnparsedTokens.Head)  state

        /// Calls the specified parser function if the predicate evaluates to true
        let parseIf (predicate:ParserState<'a> -> bool) parser state = if predicate state then parser state else state


    /// <summary>
    /// Attempts to parse the specified string as <see cref="ConventionalCommit" />
    /// </summary>
    let parse (message: string) : ParserResult =

        /// <summary>
        /// Splits a string into sequence of <see cref="FooterToken" />
        /// </summary>
        let tokenizeFooter (input:string) : FooterToken seq =
            seq {

                /// <summary>
                /// Resets the specified StringBuilder and returns its current value as <see cref="FooterToken" />
                /// </summary>
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

                yield FooterToken.Eol
            }

        /// Checks if the current line is the start of a conventional commit footer
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

        /// Recursively parses the conventional commit's footers
        let rec parseFooters (state:ParserState<LineToken>) : ParserState<LineToken> =

            let parseFooterTokens (state:ParserState<FooterToken>) : ParserState<FooterToken> =
                let footerType,descriptionTokens =
                    match state.UnparsedTokens with
                        | String "BREAKING"::Space::String "CHANGE"::Colon::Space::tail
                        | String "BREAKING"::Space::String "CHANGE"::Space::Hash::tail -> "BREAKING CHANGE",tail
                        | String footerType::Colon::Space::tail
                        | String footerType::Space::Hash::tail -> footerType,tail
                        | _ -> "",[]

                let tokens =  descriptionTokens |> List.takeWhile (fun t -> t <> FooterToken.Eol)
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
                    |> Parser.withErrorCheck (Parser.matchToken Eol)

            // parse the current line as footer
            let newState = Parser.parseLine parseFooterLine state

            // if there is a line left to parse, recurse
            if Parser.testToken (isLineTokenType (LineToken.Line "")) newState then
                Parser.withErrorCheck parseFooters newState
            else
                newState

        /// Recursively parses the conventional commit's body
        let rec parseBody (state:ParserState<LineToken>) : ParserState<LineToken> =

            let parseParagraph (state:ParserState<LineToken>) =
                if Parser.testToken (isLineTokenType (Line "")) state then
                    let lines = state.UnparsedTokens |> List.takeWhile (fun line -> line <> LineToken.Eof && line <> LineToken.Blank)
                    if lines |> Seq.length > 0 then
                        let text = lines
                                    |> Seq.map lineTokenToString
                                    |> joinString Environment.NewLine
                        let unparsedTokens =  state.UnparsedTokens |> List.skip lines.Length
                        { CurrentResult = Parser.updateResult Commit.addParagraph state.CurrentResult (Paragraph text);
                          UnparsedTokens = unparsedTokens }
                    else
                        state
                else
                    Parser.failure "Failed to parse body: Expected 'Line' token" state

            if not (Parser.testTokenEq Blank state) then // there must be a blank line before the paragraph
                state
            else
                let newState = state |> (Parser.matchToken Blank)
                // if current line is a footer, stop parsing the body, return unchanged state
                if(testFooterStart newState) then
                    state
                else
                    newState
                        |> Parser.withErrorCheck parseParagraph
                        |> Parser.withErrorCheck parseBody

        let parseHeaderLine (initialState:ParserState<LineToken>) (input:string) : ParserState<HeaderToken> =

            /// <summary>
            /// Splits a string into sequence of <see cref="HeaderToken" />
            /// </summary>
            let tokenizeHeader (input:string) : HeaderToken seq =
                seq {

                    /// <summary>
                    /// Resets the specified StringBuilder and returns its current value as <see cref="HeaderToken" />
                    /// </summary>
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

                    yield HeaderToken.Eol
                }

            let matchString (onMatch: ParserResult -> string -> ParserResult) (state: ParserState<HeaderToken>) =
                match state.UnparsedTokens with
                    | (HeaderToken.String str)::tail ->  { CurrentResult = (onMatch state.CurrentResult str); UnparsedTokens = tail }
                    | _ -> Parser.failure "Expected string token" state

            let parseType state = matchString (Parser.updateResult Commit.setType) state

            let parseScope state =
                if Parser.testTokenEq OpenParenthesis state then
                    state
                        |> Parser.withErrorCheck (Parser.matchToken OpenParenthesis)
                        |> Parser.withErrorCheck (matchString (Parser.updateResult Commit.setScope))
                        |> Parser.withErrorCheck (Parser.matchToken CloseParenthesis)
                else
                    state

            let parseBreakingChange state =
                if (Parser.testTokenEq ExclamationMark state) then
                    { CurrentResult = (Parser.updateResult Commit.setIsBreakingChange state.CurrentResult true);
                      UnparsedTokens = state.UnparsedTokens |> List.skip 1 }
                else
                    state

            let parseDescription (state:ParserState<HeaderToken>) : ParserState<HeaderToken> =
                let tokens =  state.UnparsedTokens |> List.takeWhile (fun t -> t <> HeaderToken.Eol)
                let description = tokens
                                    |> Seq.map headerTokenToString
                                    |> joinString ""

                if String.IsNullOrWhiteSpace description then
                    Parser.failure "Failed to parse header: Description must not be empty" state
                else
                    let remainingTokens = state.UnparsedTokens |> List.skip tokens.Length
                    let newResult = Parser.updateResult Commit.setDescription state.CurrentResult description
                    Parser.matchToken HeaderToken.Eol { CurrentResult = newResult; UnparsedTokens = remainingTokens }

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


        message
            |> readLines
            |> List.ofSeq
            |> Parser.start
            |> Parser.withErrorCheck (Parser.parseLine parseHeaderLine)
            |> Parser.withErrorCheck parseBody
            |> Parser.withErrorCheck (Parser.parseIf (Parser.testTokenEq Blank) ((Parser.matchToken Blank) >> parseFooters))
            |> Parser.withErrorCheck (Parser.matchToken LineToken.Eof)
            |> (fun s -> s.CurrentResult)