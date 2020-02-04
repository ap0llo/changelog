namespace ChangeLogCreator

type Paragraph = Paragraph of string

type CommitFooter = {
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
    Footers : CommitFooter list
}