module ``MessageParser Tests``

open Xunit
open Xunit.Sdk
open ChangeLogCreator;
open ChangeLogCreator.MessageParser;

let isEqualTo (expected:'a) (actual:'a) = Assert.Equal<'a>(expected, actual)
    
let tokenizeTestCases = 
    let testCase input tokens = [| input :> obj; Array.ofList tokens :> obj|]
    seq {
        // Base test cases: single matchable token (& and EOF to indicate the end of the string)
        yield testCase "" [ EofToken ]
        yield testCase "SomeString" [ StringToken "SomeString"; EofToken]
        yield testCase "(" [ OpenParenthesisToken ; EofToken]
        yield testCase ")" [ CloseParenthesisToken; EofToken ]
        yield testCase ": " [ ColonAndSpaceToken; EofToken ]
        yield testCase "\n" [ LineBreakToken "\n"; EofToken ]
        yield testCase "\r\n" [ LineBreakToken "\r\n"; EofToken ]
        yield testCase "!" [ ExclamationMarkToken; EofToken ]
        yield testCase " #" [ SpaceAndHashToken; EofToken ]

        // More complex test cases combining multiple tokens
        yield testCase "feat: " [ StringToken "feat"; ColonAndSpaceToken; EofToken ] 
        yield testCase "fix: " [ StringToken "fix"; ColonAndSpaceToken; EofToken ] 
        yield testCase "chore: " [ StringToken "chore"; ColonAndSpaceToken ; EofToken ] 
        yield testCase "someOtherType: " [ StringToken "someOtherType"; ColonAndSpaceToken; EofToken ] 
        yield testCase "feat(scope): " [ StringToken "feat"; OpenParenthesisToken; StringToken "scope"; CloseParenthesisToken; ColonAndSpaceToken ; EofToken ] 
        yield testCase "feat(scope): \r\n" [ StringToken "feat"; OpenParenthesisToken; StringToken "scope"; CloseParenthesisToken; ColonAndSpaceToken; LineBreakToken "\r\n"; EofToken ] 
        yield testCase "feat(scope): \n" [ StringToken "feat"; OpenParenthesisToken; StringToken "scope"; CloseParenthesisToken; ColonAndSpaceToken; LineBreakToken "\n"; EofToken ] 
        yield testCase "feat: \r\n" [ StringToken "feat"; ColonAndSpaceToken; LineBreakToken "\r\n" ; EofToken ] 
        yield testCase "feat: \n" [ StringToken "feat"; ColonAndSpaceToken; LineBreakToken "\n" ; EofToken ]         
        yield testCase "feat!: " [ StringToken "feat"; ExclamationMarkToken; ColonAndSpaceToken ; EofToken ] 
        yield testCase "feat(scope)!: " [ 
            StringToken "feat"; 
            OpenParenthesisToken; 
            StringToken "scope"; 
            CloseParenthesisToken; 
            ExclamationMarkToken; 
            ColonAndSpaceToken; 
            EofToken ]

        // Full, valid commit messages
        //// Single line messages without body or footer
        yield testCase "type: New feature added" [ 
            StringToken "type"
            ColonAndSpaceToken
            StringToken "New feature added"
            EofToken ]
        yield testCase "type(scope): New feature added" [ 
            StringToken "type"
            OpenParenthesisToken
            StringToken "scope"
            CloseParenthesisToken
            ColonAndSpaceToken
            StringToken "New feature added"
            EofToken ]
        yield testCase "type(scope)!: New feature added" [ 
            StringToken "type"
            OpenParenthesisToken
            StringToken "scope"
            CloseParenthesisToken
            ExclamationMarkToken
            ColonAndSpaceToken
            StringToken "New feature added"
            EofToken ]
        //// Messages with commit body
        yield testCase 
            ("type(scope)!: New feature added\r\n" + 
             "\r\n" +
             "Message body")
            [  StringToken "type"
               OpenParenthesisToken
               StringToken "scope"
               CloseParenthesisToken
               ExclamationMarkToken
               ColonAndSpaceToken 
               StringToken "New feature added"
               LineBreakToken "\r\n"
               LineBreakToken "\r\n"
               StringToken "Message body"
               EofToken ]
        yield testCase 
            ("type(scope)!: New feature added\r\n" + 
             "\r\n" +
             "Message body\r\n" +
             "\r\n" +
             "Second paragraph")
            [  StringToken "type"
               OpenParenthesisToken
               StringToken "scope"
               CloseParenthesisToken
               ExclamationMarkToken
               ColonAndSpaceToken
               StringToken "New feature added" 
               LineBreakToken "\r\n" 
               LineBreakToken "\r\n" 
               StringToken "Message body"
               LineBreakToken "\r\n" 
               LineBreakToken "\r\n"
               StringToken "Second paragraph"
               EofToken ]
    }

[<Theory>]
[<MemberData("tokenizeTestCases")>]
let ``tokenize returns expected tokens`` input (expectedTokens : Token[]) =
    let actualTokens = MessageParser.tokenize input 
    List.ofSeq actualTokens |> isEqualTo (List.ofArray expectedTokens)



let parserTestCases = 
    let testCase (input: string) (expectedResult: ParseResult) = [| input :> obj; expectedResult :> obj|]
    seq {
        // Invalid inputs 
        yield testCase "" (Failed EmptyInput)
        //// Missing ': '
        yield testCase "feat" (Failed (UnexpectedToken EofToken))   
        //// Incomplete scope / missing ')'
        yield testCase "feat(scope: Description" (Failed (UnexpectedToken ColonAndSpaceToken))   

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
            let template = { Type = "feat"; Scope = None; Description = descr.TrimEnd('\r', '\n'); IsBreakingChange = false; Body = []}
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
            })
    }

[<Theory>]
[<MemberData("parserTestCases")>]
let ``parse returns expected ParseResult`` input expectedResult =
    let actualResult = MessageParser.parse input 
    actualResult |> isEqualTo expectedResult
