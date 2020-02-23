namespace ChangeLogCreator

type Paragraph = Paragraph of string

type ConventionalCommitFooter = {
    Type : string
    Description : string
}

type ConventionalCommit = {

    /// The type of change, e.g. 'feat' or 'fix'
    Type : string

    /// The optional scope of the change
    Scope : string option

    /// Indicates whether a breaking changes hint was included in the header 
    /// (breaking changes are indicated by a '!' after the scope)
    /// Note: Breaking changes might also be indicated using a "BREAKING CHANGE" footer.

    IsBreakingChange : bool

    /// The description of the change, i.e. the summary (value does not include the feature/scope prefix)
    Description : string

    /// The paragraphs of the message's body. Empty list if no body was provided
    Body : Paragraph list

    /// The message's footers.
    Footers : ConventionalCommitFooter list
}

type GitAuthor = {
    Email : string
    Name : string
}

type GitCommit = {
    Id : string
    Message : string
    Date : System.DateTime
    Author: GitAuthor
}

type GitTag = {
    Name : string
    CommitId : string
}

type ChangeLogEntryType =
    | Feature
    | BugFix
    | WorkInProgress
    | Other of string

type ChangeLogEntry = {
    Date : System.DateTime
    Type : ChangeLogEntryType
    Scope: string option
    Summary : string
    Body: Paragraph list
    CommitId : string
    IsBreakingChange: bool
}

type VersionInfo = {
    Version : NuGet.Versioning.SemanticVersion
    Tag : GitTag
}

type VersionChangeLog = {    
    VersionInfo: VersionInfo
    BugFixes : ChangeLogEntry list
    Features : ChangeLogEntry list
    OtherChanges : (string * ChangeLogEntry list) list
}

type ChangeLog = {
    Versions : VersionChangeLog list
}