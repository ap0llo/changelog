namespace ChangeLogCreator

type Paragraph = Paragraph of string

type ConventionalCommit = {
    Type : string
    Scope : string option
    Description : string
    IsBreakingChange : bool
    Body : Paragraph list
}
    