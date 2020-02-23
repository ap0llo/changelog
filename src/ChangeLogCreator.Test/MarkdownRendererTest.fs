module ``MarkdownRenderer Test``

open System
open ChangeLogCreator
open Xunit
open NuGet.Versioning
open ApprovalTests
open ApprovalTests.Namers
open System.IO
open ApprovalTests.Reporters
open ChangeLogCreator.Test

type ApprovalNamer(testId: string) = 
    inherit UnitTestFrameworkNamer()
    member this.TestId = testId
    override this.Subdirectory = Path.Combine(base.Subdirectory, "_referenceResults");    
    override this.Name = base.Name + "_" + this.TestId.Replace(":", "")

let joinString (separator:string) (values : string list) = String.Join(separator, values |> Array.ofSeq)

let getVersionChangeLogTestCases = 

    let stringToVersionInfo (semver: string) : VersionInfo =
        { Version = SemanticVersion.Parse(semver); Tag = { Name ="Irrelevant"; CommitId = "Irrelevant" } }

    let testCase (name: string) (input:VersionChangeLog) : obj array =                
        [| name :> obj; input |> XunitSerializableVersionChangeLog :> obj|]

    seq {

        let emptyChangeLogEntry = { Date = DateTime.Now; Type = Feature; Scope = None; Summary = ""; Body = []; CommitId= ""; IsBreakingChange = false }        

        yield testCase 
            "T01_EmptyChangeLog"
            { VersionInfo = stringToVersionInfo "1.2.3"; BugFixes =[]; Features = []; OtherChanges = []  }            
        yield testCase 
            "T02_SingleBugFixChange"
            { VersionInfo = stringToVersionInfo "1.2.3-alpha"
              BugFixes = [
                { emptyChangeLogEntry with Type = BugFix; Summary = "Fixed a bug"; CommitId= "Some ID" }
              ]
              Features = []
              OtherChanges = []  }
            
        yield testCase 
            "T03_BugFixAndFeatureChange"
            { VersionInfo = stringToVersionInfo "1.2.3-alpha"
              BugFixes = [
                { emptyChangeLogEntry with Type = BugFix; Summary = "Fixed a bug"; CommitId= "Some ID" }
              ]
              Features = [
                { emptyChangeLogEntry with Type = Feature; Summary = "A new feature"; CommitId = "Some other ID" }
              ]
              OtherChanges = []  }
            
        yield testCase 
            "T04_TwoFeatureChanges"
            { VersionInfo = stringToVersionInfo "1.2.3"
              BugFixes = [ ]
              Features = [
                { emptyChangeLogEntry with Type = Feature; Scope = Some "scope1"; Summary = "A new feature"; CommitId= "Some ID" }
                { emptyChangeLogEntry with Type = Feature; Scope = Some "scope2"; Summary = "Another feature"; CommitId= "Some other ID" }
              ]
              OtherChanges = []  }
        
        // TODO: Messages with body
        // TODO: Scope
        // TODO: Breaking changes
        // TODO: Full changelog (multiple versions)

        yield testCase
            "T05_MultipleOtherChanges"
            { VersionInfo = stringToVersionInfo "1.2.3"
              BugFixes = [ ]
              Features = [ ]
              OtherChanges = [
                "docs",[ { emptyChangeLogEntry with Type = Other "docs"; Scope = Some "scope1"; Summary = "An docs update"; CommitId= "Some ID" } ]
                "build",[ { emptyChangeLogEntry with Type = Other "build"; Scope = Some "scope1"; Summary = "A change to the build system"; CommitId= "Some ID" } ]
              ]  
            }

        yield testCase
            "T06_Features_are_sorted_by_date"
            { VersionInfo = stringToVersionInfo "1.2.3"
              BugFixes = [ ]
              Features = [
                { emptyChangeLogEntry with Date = new DateTime(2020,01,01); Type = Feature; Summary = "Newer change"; CommitId= "id2" }
                { emptyChangeLogEntry with Date = new DateTime(2019,12,01); Type = Feature; Summary = "Older change"; CommitId= "id1" }
              ]
              OtherChanges = []
            }

        yield testCase
            "T07_Bugfixes_are_sorted_by_date"
            { VersionInfo = stringToVersionInfo "1.2.3"
              Features = [ ]
              BugFixes = [
                { emptyChangeLogEntry with Date = new DateTime(2020,01,01); Type = Feature; Summary = "Newer change"; CommitId= "id2" }
                { emptyChangeLogEntry with Date = new DateTime(2019,12,01); Type = Feature; Summary = "Older change"; CommitId= "id1" }
              ]
              OtherChanges = []
            }

        yield testCase 
            "T08: Breaking changes in features are highlighted"
            { VersionInfo = stringToVersionInfo "1.2.3"
              BugFixes = [ ]
              Features = [
                { emptyChangeLogEntry with IsBreakingChange = false; Type = Feature; Summary = "Newer change"; CommitId= "id2"; }
                { emptyChangeLogEntry with IsBreakingChange = true; Type = Feature; Summary = "Older change";  CommitId= "id1"; }
              ]
              OtherChanges = []
            }
            
        //TODO: Breaking changes in bug fixes are highlighted
        //TODO: Other changes are included if they are a breaking change
    }



[<Theory>]
[<MemberData("getVersionChangeLogTestCases")>]
[<UseReporter(typedefof<DiffReporter>)>]
let ``getVersionChangeLog returns expected Markdown`` (testCaseName:string) (input: XunitSerializableVersionChangeLog) =    
    let actualMarkdown = (MarkdownRenderer.renderVersionChangeLog input.Value).ToString(MarkdownRenderer.serializationOptions)
    let writer = new ApprovalTextWriter(actualMarkdown)
    Approvals.Verify(writer, ApprovalNamer(testCaseName),Approvals.GetReporter())
    ()



