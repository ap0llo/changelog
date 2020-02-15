module ChangeLogBuilderTests

open Xunit
open Assertions
open ChangeLogCreator
open ChangeLogCreator.ChangeLogBuilder
open NuGet.Versioning

let stringToVersionInfo (semver: string) : VersionInfo =
    { Version = SemanticVersion.Parse(semver); Tag = { Name ="Irrelevant"; CommitId = "Irrelevant" } }

let optionalStringToVersionInfo (semver: string option) : VersionInfo option=
    match semver with
        | Some string -> Some (stringToVersionInfo string)
        | None -> None

let getVersionsTestCases =
    let testCase (tags: GitTag list) (expectedVersions: VersionInfo list) : obj array =  
        [| tags :> obj; expectedVersions :> obj|]

    seq {

        yield testCase [] []

        yield testCase [ { Name = "not a version"; CommitId = "" }] []

        yield testCase
            [ { Name = "1.2.3"; CommitId = "abc" }] 
            [ { Version = SemanticVersion(1, 2, 3) ; Tag = { Name = "1.2.3"; CommitId = "abc" } }]

        yield testCase
            [ { Name = "1.2.3-pre"; CommitId = "abc" }] 
            [ { Version = SemanticVersion(1, 2, 3, "pre"); Tag = { Name = "1.2.3-pre"; CommitId = "abc" } }]

        yield testCase
            [ 
                { Name = "v1.2.3-pre"; CommitId = "abc" }
                { Name = "4.5.6"; CommitId = "def" } 
            ] 
            [ 
                { Version = SemanticVersion(1, 2, 3, "pre"); Tag = { Name = "v1.2.3-pre"; CommitId = "abc" } }
                { Version = SemanticVersion(4, 5, 6); Tag = { Name = "4.5.6"; CommitId = "def" } }
            ]
    }


[<Theory>]
[<MemberData("getVersionsTestCases")>]
let ``getVersions returns expected versions`` tags expectedVersions =
    let actualVersions = getVersions tags
    actualVersions |> mustBeEqualTo expectedVersions

let getPreviousVersionsTestData = 
    let testCase (allVersions: string seq) (currentVersion: string) (expecectedVersion: string option): obj array =        
        [|  allVersions |> Seq.map stringToVersionInfo :> obj 
            currentVersion |> stringToVersionInfo :> obj
            expecectedVersion |> optionalStringToVersionInfo :> obj |]

    seq {
        yield testCase [ "1.0.0"; "1.1.0"] ("1.1.0") (Some "1.0.0")
        yield testCase [ "1.1.0"] "1.1.0" None
        yield testCase [ "1.1.0"; "2.0.0" ] "1.1.0" None
        yield testCase [ "1.0.0"; "1.1.4"; "1.2.5" ] "1.1.6" (Some "1.1.4")
    }

[<Theory>]
[<MemberData("getPreviousVersionsTestData")>]
let ``getPreviousVersion returns expected version`` (allVersions: VersionInfo seq) (currentVersion: VersionInfo) (expectedPreviousVersion: VersionInfo option) =
    let actualPreviousVersion = ChangeLogBuilder.getPreviousVersion allVersions currentVersion
    actualPreviousVersion |> mustBeEqualTo expectedPreviousVersion