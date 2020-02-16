module ``MarkdownRenderer Test``

open System
open ChangeLogCreator
open Xunit
open NuGet.Versioning
open ApprovalTests
open ApprovalTests.Namers
open System.IO
open ApprovalTests.Reporters

type ApprovalNamer(testId: string) = 
    inherit UnitTestFrameworkNamer()
    member this.TestId = testId
    override this.Subdirectory = Path.Combine(base.Subdirectory, "_referenceResults");    
    override this.Name = base.Name + "_" + this.TestId

type MarkdownRendererTestCase = {
    Name : string
    Input: VersionChangeLog
}

let joinString (separator:string) (values : string list) = String.Join(separator, values |> Array.ofSeq)

let getVersionChangeLogTestCases = 

    let stringToVersionInfo (semver: string) : VersionInfo =
        { Version = SemanticVersion.Parse(semver); Tag = { Name ="Irrelevant"; CommitId = "Irrelevant" } }

    let testCase (name: string) (input:VersionChangeLog) : obj array =        
        let testCaseData = { Name = name; Input = input; }
        [| testCaseData :> obj|]

    seq {
        yield testCase 
            "T01_EmptyChangeLog"
            { VersionInfo = stringToVersionInfo "1.2.3"; BugFixes =[]; Features = []; OtherChanges = []  }            
        yield testCase 
            "T02_SingleBugFixChange"
            { VersionInfo = stringToVersionInfo "1.2.3-alpha"
              BugFixes = [
                { Date = DateTime.Now; Type = BugFix; Scope = None; Summary = "Fixed a bug"; Body = []; CommitId= "Some ID"}
              ]
              Features = []
              OtherChanges = []  }
            
        yield testCase 
            "T03_BugFixAndFeatureChange"
            { VersionInfo = stringToVersionInfo "1.2.3-alpha"
              BugFixes = [
                { Date = DateTime.Now; Type = BugFix; Scope = None; Summary = "Fixed a bug"; Body = []; CommitId= "Some ID"}
              ]
              Features = [
                { Date = DateTime.Now; Type = Feature; Scope = None; Summary = "A new feature"; Body = []; CommitId= "Some other ID"}
              ]
              OtherChanges = []  }
            
        yield testCase 
            "T04_TwoFeatureChanges"
            { VersionInfo = stringToVersionInfo "1.2.3"
              BugFixes = [ ]
              Features = [
                { Date = DateTime.Now; Type = Feature; Scope = Some "scope1"; Summary = "A new feature"; Body = []; CommitId= "Some ID"}
                { Date = DateTime.Now; Type = Feature; Scope = Some "scope2"; Summary = "Another feature"; Body = []; CommitId= "Some other ID"}
              ]
              OtherChanges = []  }
            
    }



[<Theory>]
[<MemberData("getVersionChangeLogTestCases")>]
[<UseReporter(typedefof<DiffReporter>)>]
let ``getVersionChangeLog returns expected Markdown`` (testCase: MarkdownRendererTestCase) =    
    let actualMarkdown = (MarkdownRenderer.renderVersionChangeLog testCase.Input).ToString()
    let writer = new ApprovalTextWriter(actualMarkdown)
    Approvals.Verify(writer, ApprovalNamer(testCase.Name),Approvals.GetReporter())
    ()



