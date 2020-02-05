module ``MessageParser Tests``

open System
open Xunit
open ChangeLogCreator;
open ChangeLogCreator.MessageParser;

let isEqualTo (expected:'a) (actual:'a) = Assert.Equal<'a>(expected, actual)

/// Test cases for the "line tokenizer"
let readLinesTestCases =
    let testCase (input : string) (tokens : LineToken list) : obj array = [| input :> obj; Array.ofList tokens :> obj|]
    seq {

        yield testCase "" [ Eof ]

        for lineBreak in ["\n"; "\r\n"] do
            // just a line break
            yield testCase lineBreak [ Blank ; Eof ]
            yield testCase lineBreak [ Blank ; Eof ]

            // 1 line without line break
            yield testCase "Line1" [ Line "Line1"; Eof ]

            // 1 line with line break
            yield testCase ("Line1" + lineBreak)  [ Line "Line1"; Eof ]

            // 2 lines (with and without trailing line break)
            yield testCase ("Line1" + lineBreak + "Line2" + lineBreak) [ Line "Line1"; Line "Line2"; Eof ]
            yield testCase ("Line1" + lineBreak + "Line2") [ Line "Line1"; Line "Line2"; Eof ]

            // 2 lines with blank line in between (with and without trailing line break)
            yield testCase ("Line1" + lineBreak + lineBreak + "Line2") [ Line "Line1"; Blank; Line "Line2"; Eof ]
            yield testCase ("Line1" + lineBreak + lineBreak + "Line2" + lineBreak) [ Line "Line1"; Blank; Line "Line2"; Eof ]
    }

[<Theory>]
[<MemberData("readLinesTestCases")>]
let ``readLines returns expected lines`` input (expectedTokens : obj[]) =
    let objectToLineToken (obj:obj) : LineToken = downcast obj
    let actualTokens = MessageParser.readLines input
    List.ofSeq actualTokens |> isEqualTo (expectedTokens |> Seq.map objectToLineToken |> List.ofSeq)


/// parser test cases
let parserTestCases =
    let lineBreak = "\r\n"
    let joinString (separator:string) (values : string list) = String.Join(separator, values |> Array.ofList)
    let testCase (input: string) (expectedResult: ParserResult) : obj array = [| input :> obj; expectedResult :> obj|]
    let multiLineTestCase (input:string list) = testCase (joinString lineBreak input)

    seq {

        // ---------------------------
        // Invalid Inputs
        // ---------------------------

        // empty
        yield testCase "" (Failed "Unexpected token Eof")

        // Missing ': ' in header
        yield testCase "feat" (Failed "Expected token Colon")

        // Incomplete scope / missing ')' in header
        yield testCase "feat(scope: Description" (Failed "Expected token CloseParenthesis")

        // missing description in header
        for invalidDescription in [ ""; "\t" ] do
            yield testCase ("feat:" + invalidDescription) (Failed "Expected token Space")
            yield testCase ("feat(scope):" + invalidDescription) (Failed "Expected token Space")

        for invalidDescription in [ " "; "  " ] do
            yield testCase ("feat:" + invalidDescription) (Failed "Failed to parse header: Description must not be empty")
            yield testCase ("feat(scope):" + invalidDescription) (Failed "Failed to parse header: Description must not be empty")

        // missing description in footer
        for invalidFooter in [ "Footer: "; "BREAKING CHANGE: "; "Footer: \t"; "Footer:  "; "Footer # "; "BREAKING CHANGE #\t" ] do
            yield testCase ("type(scope): Description" + lineBreak + lineBreak + invalidFooter) (Failed "Failed to parse footer: Description must not be empty")

        // multiple blank lines between header and body
        yield multiLineTestCase
            [
                "type: Description"
                ""
                ""
                "Body"
            ]
            (Failed "Failed to parse body: Expected 'Line' token")

        // multiple blank lines between body and footer
        yield multiLineTestCase
            [
                "type: Description"
                ""
                "Body 1"
                "Body 2"
                ""
                ""
                "Footer: Value"
            ]
            (Failed "Failed to parse body: Expected 'Line' token")

        // footer with empty description
        yield multiLineTestCase
            [
                "type: Description"
                ""
                "Footer: "
            ]
            (Failed "Failed to parse footer: Description must not be empty")
        yield multiLineTestCase
            [
                "type: Description"
                ""
                "Footer #"
            ]
            (Failed "Failed to parse footer: Description must not be empty")


        // ---------------------------
        // Valid inputs
        // ---------------------------
        let descriptions =
            [
                "Some description"
                "Description: "
                "Description ()"
                "Description!"
                "Description #"
                "Some description\r\n"  // trailing line breaks are valid but ignored by the parser
                "Some description\n"  // trailing line breaks are valid but ignored by the parser
            ]
        for descr in descriptions do
            let template = { Type = "feat"; Scope = None; Description = descr.TrimEnd('\r', '\n'); IsBreakingChange = false; Body = []; Footers = [] }
            yield testCase ("feat: " + descr) (Parsed template)
            yield testCase ("feat(scope): " + descr) (Parsed { template with Scope = Some "scope"; })
            yield testCase ("feat(scope)!: " + descr) (Parsed { template with Scope = Some "scope"; IsBreakingChange = true })

        //// TODO: Ignore trailing blank lines

        // single line body
        yield multiLineTestCase
            [
                "type(scope): Description"
                ""
                "Single Line Body"
            ]
            (Parsed {
                Type = "type";
                Scope = Some "scope";
                Description = "Description";
                IsBreakingChange = false;
                Body = [Paragraph "Single Line Body"]
                Footers = []
            })

        // multi-line body
        yield multiLineTestCase
            [
                "type(scope): Description"
                ""
                "Body line 1"
                "Body line 2"
            ]
            (Parsed {
                Type = "type";
                Scope = Some "scope";
                Description = "Description";
                IsBreakingChange = false;
                Body = [Paragraph "Body line 1\r\nBody line 2"]
                Footers = []
            })

        //multi-paragraph body (1)
        yield multiLineTestCase
            [
                "type(scope): Description"
                ""
                "Body line 1.1"
                "Body line 1.2"
                ""
                "Body line 2.1"
                "Body line 2.2"
            ]
            (Parsed {
                Type = "type";
                Scope = Some "scope";
                Description = "Description";
                IsBreakingChange = false;
                Body = [
                    Paragraph "Body line 1.1\r\nBody line 1.2"
                    Paragraph "Body line 2.1\r\nBody line 2.2"
                ]
                Footers = []
            })

        // multi-paragraph body with trailing line break
        yield multiLineTestCase
            [
                "type(scope): Description"
                ""
                "Body line 1.1"
                "Body line 1.2"
                ""
                "Body line 2.1\r\n"
            ]
            (Parsed {
                Type = "type";
                Scope = Some "scope";
                Description = "Description";
                IsBreakingChange = false;
                Body = [
                    Paragraph "Body line 1.1\r\nBody line 1.2"
                    Paragraph "Body line 2.1"
                ]
                Footers = []
            })


        // messages with footer
        for footerType in ["footer-type"; "BREAKING CHANGE"; "BREAKING-CHANGE"] do
            for separator in [": "; " #"] do
                yield multiLineTestCase
                    [
                        "type: Description"
                        ""
                        footerType + separator + "Footer Description"
                    ]
                    (Parsed {
                        Type = "type";
                        Scope = None
                        Description = "Description";
                        IsBreakingChange = false;
                        Body = []
                        Footers = [
                            { Type = footerType; Description = "Footer Description" }
                        ]
                    })

        yield multiLineTestCase
            [
                "type: Description"
                ""
                "Footer1: Footer Description1"
                "Footer2 #Footer Description2"
            ]
            (Parsed {
                Type = "type";
                Scope = None
                Description = "Description";
                IsBreakingChange = false;
                Body = []
                Footers = [
                    { Type = "Footer1"; Description = "Footer Description1" }
                    { Type = "Footer2"; Description = "Footer Description2" }
                ]
            })
        // message with body AND footer
        yield multiLineTestCase
            [
                "type(scope): Description"
                ""
                "Body line 1.1"
                "Body line 1.2"
                ""
                "Body line 2.1"
                ""
                "Reviewed-by: Z"
            ]
            (Parsed {
                Type = "type";
                Scope = Some "scope";
                Description = "Description";
                IsBreakingChange = false;
                Body = [
                    Paragraph "Body line 1.1\r\nBody line 1.2"
                    Paragraph "Body line 2.1"
                ]
                Footers = [
                    { Type = "Reviewed-by"; Description =  "Z" }
                ]
            })

        yield multiLineTestCase
            [
                "type(scope): Description"
                ""
                "Body line 1.1"
                "Body line 1.2"
                ""
                "Body line 2.1"
                ""
                "Reviewed-by: Z"
                "Footer2: description"
            ]
            (Parsed {
                Type = "type";
                Scope = Some "scope";
                Description = "Description";
                IsBreakingChange = false;
                Body = [
                    Paragraph "Body line 1.1\r\nBody line 1.2"
                    Paragraph "Body line 2.1"
                ]
                Footers = [
                    { Type = "Reviewed-by"; Description =  "Z" }
                    { Type = "Footer2"; Description =  "description" }
                ]
            })
    }

[<Theory>]
[<MemberData("parserTestCases")>]
let ``parse returns expected ParseResult`` input expectedResult =
    let actualResult = MessageParser.parse input
    actualResult |> isEqualTo expectedResult
