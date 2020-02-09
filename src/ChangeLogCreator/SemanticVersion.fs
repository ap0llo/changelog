namespace ChangeLogCreator

module SemanticVersion =

    type NuGetSemanticVersion = NuGet.Versioning.SemanticVersion

    let parse (value:string) : NuGet.Versioning.SemanticVersion option =
        let (sucess, parsedVersion) =  NuGetSemanticVersion.TryParse value
        if sucess then
            Some parsedVersion
        else
            None
