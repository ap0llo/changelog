namespace ChangeLogCreator.Test

open System
open ChangeLogCreator.MessageParser
open Xunit.Abstractions
open Newtonsoft.Json
open ChangeLogCreator
open NuGet.Versioning

type internal XunitSerializableLineToken(value: LineToken) =    
    let mutable _value = value

    member this.Value with get() = _value

    new() = XunitSerializableLineToken Eof

    interface IXunitSerializable with
        member this.Deserialize (info: IXunitSerializationInfo) =
            let tokenType = info.GetValue("tokenType")
            _value <- match tokenType with
                                | "Blank" -> Blank
                                | "Eof" -> Eof
                                | "Line" ->
                                    let lineValue = info.GetValue("tokenValue")
                                    (Line lineValue)
                                | _ -> raise (InvalidOperationException "")
            ()
        member this.Serialize (info: IXunitSerializationInfo) =
            match _value with
                | Blank -> info.AddValue("tokenType", "Blank")
                | Eof -> info.AddValue("tokenType", "Eof")
                | Line str -> 
                    info.AddValue("tokenType", "Line")
                    info.AddValue("tokenValue", str)                

type internal XunitSerializableParserResult(value : ParserResult) =
    let mutable _value = value

    member this.Value with get() = _value

    new () = XunitSerializableParserResult(Failed "Unparsed")

    interface IXunitSerializable with
        member this.Deserialize (info: IXunitSerializationInfo) =
            let resultType = info.GetValue("ParserResult.Type")
            _value <- match resultType with 
                                | "Failed" -> 
                                    let message = info.GetValue("Failed.Message")
                                    Failed message
                                | "Parsed" ->
                                    let json = info.GetValue("Parsed.Data")
                                    let conventionalCommit = JsonConvert.DeserializeObject<ConventionalCommit>(json)
                                    Parsed conventionalCommit
                                | _ -> raise (InvalidOperationException "")
        member this.Serialize (info: IXunitSerializationInfo) =
            match _value with
                | Parsed parsed -> 
                    let json = JsonConvert.SerializeObject(parsed)
                    info.AddValue("ParserResult.Type", "Parsed")
                    info.AddValue("Parsed.Data", json)
                | Failed message ->
                    info.AddValue("ParserResult.Type", "Failed")
                    info.AddValue("Failed.Message", message)                
                    

type XunitSerializableGitTag(value:GitTag) =
    let mutable _value = value

    member this.Value with get() = _value

    new() = XunitSerializableGitTag { Name = ""; CommitId = ""}

    interface IXunitSerializable with
        member this.Deserialize (info: IXunitSerializationInfo) =
            let name = info.GetValue("Name")
            let commitId = info.GetValue("CommitId")
            _value <- { Name = name; CommitId = commitId }

        member this.Serialize (info: IXunitSerializationInfo) =
            info.AddValue("Name", _value.Name)
            info.AddValue("CommitId", _value.CommitId)

type XunitSerializableVersionInfo(value: VersionInfo) =
    let mutable _value = value

    member this.Value with get() = _value

    new() = XunitSerializableVersionInfo { Version = null; Tag = { Name =""; CommitId = ""} }

    interface IXunitSerializable with
        member this.Deserialize (info: IXunitSerializationInfo) =
            let versionString = info.GetValue("Version")
            let tagOption = info.GetValue<XunitSerializableGitTag>("Tag")
            _value <- { Version = SemanticVersion.Parse(versionString); Tag = tagOption.Value }
            
        member this.Serialize (info: IXunitSerializationInfo) =
            info.AddValue("Version", _value.Version.ToFullString())
            info.AddValue("Tag", XunitSerializableGitTag _value.Tag)
            
type XunitSerializableList<'a>(value: 'a list) = 
    let mutable _value = value

    member this.Value with get() = _value

    new() = XunitSerializableList []

    interface IXunitSerializable with
        member this.Deserialize (info: IXunitSerializationInfo) =
            let array = info.GetValue<'a[]>("value")        
            _value <- List.ofArray array
        
        member this.Serialize (info: IXunitSerializationInfo) =
            let array = Array.ofList _value
            info.AddValue("value", array)

type XunitSerializableOption<'a>(value: 'a option) =
    let mutable _value = value

    member this.Value with get() = _value

    new() = XunitSerializableOption None

    member this.unwrap (mapper: 'a -> 'b) : 'b option = 
        match _value with
            | Some data -> Some (mapper data)
            | None -> None


    interface IXunitSerializable with
        member this.Deserialize (info: IXunitSerializationInfo) =
            let isSome = info.GetValue<bool>("IsSome")
            if isSome then
                let some = info.GetValue<'a>("Value")
                _value <- Some some
            else
                _value <- None
    
        member this.Serialize (info: IXunitSerializationInfo) =
            match value with
                    | Some data -> 
                        info.AddValue("IsSome", true)
                        info.AddValue("Value", data)
                    | None -> info.AddValue("IsSome", false)


type XunitSerializableVersionChangeLog(value: VersionChangeLog) =
    let mutable _value = value

    member this.Value with get() = _value

    new() = XunitSerializableVersionChangeLog { VersionInfo = { Version = null; Tag = { Name = ""; CommitId = ""}}; BugFixes = []; Features = []; OtherChanges = [] }

    interface IXunitSerializable with
        member this.Deserialize (info: IXunitSerializationInfo) =            
            let versionInfo = JsonConvert.DeserializeObject<XunitSerializableVersionInfo>(info.GetValue("VersionInfo"))
            let features = JsonConvert.DeserializeObject<ChangeLogEntry[]>(info.GetValue("Features"))
            let bugfixes = JsonConvert.DeserializeObject<ChangeLogEntry[]>(info.GetValue("BugFixes"))
            let otherChanges = JsonConvert.DeserializeObject<(string*ChangeLogEntry list)[]>(info.GetValue("OtherChanges"))
            _value <- { VersionInfo = versionInfo.Value; Features = features |> List.ofArray ; BugFixes = bugfixes  |> List.ofArray ; OtherChanges = otherChanges |> List.ofArray }

        member this.Serialize (info: IXunitSerializationInfo) =
            info.AddValue("VersionInfo", (XunitSerializableVersionInfo _value.VersionInfo))
            info.AddValue("Features", JsonConvert.SerializeObject(_value.Features |> Array.ofList))
            info.AddValue("BugFixes", JsonConvert.SerializeObject(_value.BugFixes|> Array.ofList))
            info.AddValue("OtherChanges", JsonConvert.SerializeObject(_value.OtherChanges|> Array.ofList))



            //VersionInfo: VersionInfo
            //   BugFixes : ChangeLogEntry list
            //   Features : ChangeLogEntry list
            //   OtherChanges : (string * ChangeLogEntry list) list