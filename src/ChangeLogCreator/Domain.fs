namespace ChangeLogCreator

type Paragraph = Paragraph of string

type CommitFooter = {
    Type : string
    Description : string
}

type ConventionalCommit = {
    Type : string
    Scope : string option
    Description : string
    IsBreakingChange : bool
    Body : Paragraph list
    Footers : CommitFooter list
}
    