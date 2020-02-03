module ``MessageParser Tests``

open Xunit
open ChangeLogCreator;
open ChangeLogCreator.MessageParser;

let isEqualTo (expected:'a) (actual:'a) = Assert.Equal<'a>(expected, actual)
    
let readLinesTestCases = 
    let testCase input tokens = [| input :> obj; Array.ofList tokens :> obj|]
    seq {
        
        yield testCase "" [ Eof]
        yield testCase "\r\n" [ Blank ; Eof ]
        yield testCase "\n" [ Blank ; Eof ]

        // One line
        yield testCase "Line1\r\n" [ Line "Line1"; Eof ]
        yield testCase "Line1\n" [ Line "Line1"; Eof ]

        // 2 lines
        yield testCase "Line1\r\nLine2" [ Line "Line1"; Line "Line2"; Eof ]
        yield testCase "Line1\nLine2" [ Line "Line1"; Line "Line2"; Eof ]
        yield testCase "Line1\r\nLine2\r\n" [ Line "Line1"; Line "Line2"; Eof ]
        yield testCase "Line1\nLine2\r\n" [ Line "Line1"; Line "Line2"; Eof ]
        // 3 Lines
        yield testCase "Line1\r\n\r\nLine2" [ Line "Line1"; Blank; Line "Line2"; Eof ]
        yield testCase "Line1\n\nLine2" [ Line "Line1"; Blank; Line "Line2"; Eof ]
        yield testCase "Line1\r\n\r\nLine2\r\n" [ Line "Line1"; Blank; Line "Line2"; Eof ]
        yield testCase "Line1\n\nLine2\n" [ Line "Line1"; Blank; Line "Line2"; Eof ]
    }

[<Theory>]
[<MemberData("readLinesTestCases")>]
let ``readLines returns expected lines`` input (expectedTokens : LineToken[]) =
    let actualTokens = MessageParser.readLines input 
    List.ofSeq actualTokens |> isEqualTo (List.ofArray expectedTokens)



let parserTestCases = 
    let testCase (input: string) (expectedResult: ParserResult) = [| input :> obj; expectedResult :> obj|]
    seq {
        //TODO: Description must not be empty

        // Invalid inputs 
        yield testCase "" (Failed "Unexpected token Eof")
        //// Missing ': '
        yield testCase "feat" (Failed "Expected token Colon")
        //// Incomplete scope / missing ')'
        yield testCase "feat(scope: Description" (Failed "Expected token CloseParenthesis")

        // Valid inputs
        let descriptions = [
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

        // single line body
        yield testCase 
            "type(scope): Description\r\n\r\nSingle Line Body" 
            (Parsed {
                Type = "type"; 
                Scope = Some "scope"; 
                Description = "Description"; 
                IsBreakingChange = false; 
                Body = [Paragraph "Single Line Body"]
                Footers = []
            })

        // multi-line body
        yield testCase 
            "type(scope): Description\r\n\r\nBody line 1\r\nBody line 2" 
            (Parsed {
                Type = "type"; 
                Scope = Some "scope"; 
                Description = "Description"; 
                IsBreakingChange = false; 
                Body = [Paragraph "Body line 1\r\nBody line 2"]
                Footers = []
            })

        // multi-paragraph body (1)
        yield testCase 
            (
            "type(scope): Description\r\n" + 
            "\r\n" + 
            "Body line 1.1\r\n" + 
            "Body line 1.2\r\n" + 
            "\r\n" +
            "Body line 2.1\r\n" + 
            "Body line 2.2\r\n"
            )
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

        // multi-paragraph body (2)
        yield testCase 
            (
            "type(scope): Description\r\n" + 
            "\r\n" + 
            "Body line 1.1\r\n" + 
            "Body line 1.2\r\n" + 
            "\r\n" +
            "Body line 2.1"
            )
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

        //TODO: tests for all footer separators
        //TODO: tests for all footer types (no space and BREAKING CHANGE)
        // message with body AND footer
        yield testCase 
            (
            "type(scope): Description\r\n" + 
            "\r\n" + 
            "Body line 1.1\r\n" + 
            "Body line 1.2\r\n" + 
            "\r\n" +
            "Body line 2.1\r\n" +
            "\r\n" +
            "Reviewed-by: Z"
            )
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

        yield testCase 
            (
            "type(scope): Description\r\n" + 
            "\r\n" + 
            "Body line 1.1\r\n" + 
            "Body line 1.2\r\n" + 
            "\r\n" +
            "Body line 2.1\r\n" +
            "\r\n" +
            "Reviewed-by: Z\r\n" +
            "Footer2: description"
            )
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

        yield testCase 
           (
           "type(scope): Description\r\n" + 
           "\r\n" +
           "Reviewed-by: Z\r\n" +
           "Footer2: description"
           )
           (Parsed {
               Type = "type"; 
               Scope = Some "scope"; 
               Description = "Description"; 
               IsBreakingChange = false; 
               Body = [ ]
               Footers = [
                   { Type = "Reviewed-by"; Description =  "Z" }
                   { Type = "Footer2"; Description =  "description" }
               ]
           })

        yield testCase 
           (
           "type(scope): Description\r\n" + 
           "\r\n" +           
           "BREAKING CHANGE: description"
           )
           (Parsed {
               Type = "type"; 
               Scope = Some "scope"; 
               Description = "Description"; 
               IsBreakingChange = false; 
               Body = [ ]
               Footers = [
                   { Type = "BREAKING CHANGE"; Description =  "description" }
               ]
           })

        //TODO: footer description must not be empty
    }

[<Theory>]
[<MemberData("parserTestCases")>]
let ``parse returns expected ParseResult`` input expectedResult =
    let actualResult = MessageParser.parse input 
    actualResult |> isEqualTo expectedResult
