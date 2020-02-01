module ``MessageParser Tests``

open Xunit
open Xunit.Sdk
open ChangeLogCreator;
open ChangeLogCreator.MessageParser;

let rec listIsEqualTo (expected:'a list) (actual:'a list) =        
    match expected,actual with
        | expectedHead :: expectedTail, actualHead :: actualTail ->
            Assert.Equal<'a>(expectedHead, actualHead)
            actualTail |> listIsEqualTo expectedTail
        | [],[] -> ()
        | _ -> raise (XunitException (sprintf "List comparison failed:\r\nExpected %A\r\nActual:%A" expected actual))
    
let tokenizeTestData = 
    let testCase input tokens = [| input :> obj; Array.ofList tokens :> obj|]
    seq {
        // Base test cases: single matchable token (& and EOF to indicate the end of the string)
        yield testCase "" [ EofToken ]
        yield testCase "SomeString" [ StringToken "SomeString"; EofToken]
        yield testCase "(" [ OpenParenthesisToken ; EofToken]
        yield testCase ")" [ CloseParenthesisToken; EofToken ]
        yield testCase ": " [ ColonAndSpaceToken; EofToken ]
        yield testCase "\n" [ LineBreakToken; EofToken ]
        yield testCase "\r\n" [ LineBreakToken; EofToken ]
        yield testCase "!" [ ExclamationMarkToken; EofToken ]
        yield testCase " #" [ SpaceAndHashToken; EofToken ]

        // More complex test cases combining multiple tokens
        yield testCase "feat: " [ StringToken "feat"; ColonAndSpaceToken; EofToken ] 
        yield testCase "fix: " [ StringToken "fix"; ColonAndSpaceToken; EofToken ] 
        yield testCase "chore: " [ StringToken "chore"; ColonAndSpaceToken ; EofToken ] 
        yield testCase "someOtherType: " [ StringToken "someOtherType"; ColonAndSpaceToken; EofToken ] 
        yield testCase "feat(scope): " [ StringToken "feat"; OpenParenthesisToken; StringToken "scope"; CloseParenthesisToken; ColonAndSpaceToken ; EofToken ] 
        yield testCase "feat(scope): \r\n" [ StringToken "feat"; OpenParenthesisToken; StringToken "scope"; CloseParenthesisToken; ColonAndSpaceToken; LineBreakToken ; EofToken ] 
        yield testCase "feat(scope): \n" [ StringToken "feat"; OpenParenthesisToken; StringToken "scope"; CloseParenthesisToken; ColonAndSpaceToken; LineBreakToken ; EofToken ] 
        yield testCase "feat: \r\n" [ StringToken "feat"; ColonAndSpaceToken; LineBreakToken ; EofToken ] 
        yield testCase "feat: \n" [ StringToken "feat"; ColonAndSpaceToken; LineBreakToken ; EofToken ]         
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
            StringToken "type"; 
            ColonAndSpaceToken; 
            StringToken "New feature added"
            EofToken ]
        yield testCase "type(scope): New feature added" [ 
            StringToken "type"; 
            OpenParenthesisToken;
            StringToken "scope"
            CloseParenthesisToken;
            ColonAndSpaceToken; 
            StringToken "New feature added"
            EofToken ]
        yield testCase "type(scope)!: New feature added" [ 
            StringToken "type"; 
            OpenParenthesisToken;
            StringToken "scope"
            CloseParenthesisToken;
            ExclamationMarkToken
            ColonAndSpaceToken; 
            StringToken "New feature added"
            EofToken ]
        //// Messages with commit body
        yield testCase 
            ("type(scope)!: New feature added\r\n" + 
             "\r\n" +
             "Message body")
            [  StringToken "type"; 
               OpenParenthesisToken;
               StringToken "scope"
               CloseParenthesisToken;
               ExclamationMarkToken
               ColonAndSpaceToken; 
               StringToken "New feature added"
               LineBreakToken
               LineBreakToken
               StringToken "Message body"
               EofToken ]
        yield testCase 
            ("type(scope)!: New feature added\r\n" + 
             "\r\n" +
             "Message body\r\n" +
             "\r\n" +
             "Second paragraph")
            [  StringToken "type"; 
               OpenParenthesisToken;
               StringToken "scope"
               CloseParenthesisToken;
               ExclamationMarkToken
               ColonAndSpaceToken; 
               StringToken "New feature added"
               LineBreakToken
               LineBreakToken
               StringToken "Message body"
               LineBreakToken
               LineBreakToken
               StringToken "Second paragraph"
               EofToken ]

    }

[<Theory>]
[<MemberData("tokenizeTestData")>]
let ``tokenize returns expected tokens`` input (expectedTokens : Token[]) =
    let actualTokens = MessageParser.tokenize input 
    List.ofSeq actualTokens |> listIsEqualTo (List.ofArray expectedTokens)