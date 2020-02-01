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
        yield testCase "feat:" [ (StringToken "feat"); ColonToken; EofToken ] 
        yield testCase "fix:" [ (StringToken "fix"); ColonToken; EofToken ] 
        yield testCase "chore:" [ (StringToken "chore"); ColonToken ; EofToken ] 
        yield testCase "someOtherType:" [ (StringToken "someOtherType"); ColonToken; EofToken ] 
        yield testCase "feat(scope):" [ (StringToken "feat"); OpenParenthesisToken; (StringToken "scope"); CloseParenthesisToken; ColonToken ; EofToken ] 
        yield testCase "feat(scope):\r\n" [ (StringToken "feat"); OpenParenthesisToken; (StringToken "scope"); CloseParenthesisToken; ColonToken; LineBreakToken ; EofToken ] 
        yield testCase "feat(scope):\n" [ (StringToken "feat"); OpenParenthesisToken; (StringToken "scope"); CloseParenthesisToken; ColonToken; LineBreakToken ; EofToken ] 
        yield testCase "feat:\r\n" [ (StringToken "feat"); ColonToken; LineBreakToken ; EofToken ] 
        yield testCase "feat:\n" [ (StringToken "feat"); ColonToken; LineBreakToken ; EofToken ] 
        yield testCase "feat: " [ (StringToken "feat"); ColonToken; SpaceToken ; EofToken ] 
        yield testCase "feat!: " [ (StringToken "feat"); ExclamationMarkToken; ColonToken; SpaceToken ; EofToken ] 
        yield testCase "feat(scope)!: " [ 
            (StringToken "feat"); 
            OpenParenthesisToken; 
            (StringToken "scope"); 
            CloseParenthesisToken; 
            ExclamationMarkToken; 
            ColonToken; 
            SpaceToken; 
            EofToken 
        ] 
    }

[<Theory>]
[<MemberData("tokenizeTestData")>]
let ``tokenize retruns expected tokens`` input (expectedTokens : Token[]) =
    let actualTokens = MessageParser.tokenize input 
    List.ofSeq actualTokens |> listIsEqualTo (List.ofArray expectedTokens)