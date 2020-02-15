namespace ChangeLogCreator

module SemVer =
    
    open NuGet.Versioning

    let tryParse (value:string) : SemanticVersion option =
        let (sucess, parsedVersion) =  SemanticVersion.TryParse value
        if sucess then
            Some parsedVersion
        else
            None    