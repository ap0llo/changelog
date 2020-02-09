namespace ChangeLogCreator

open Grynwald.MarkdownGenerator
open ChangeLogCreator.Markdown;

module MarkdownRenderer =

    let saveChangeLog (outputPath:string) (changeLog: ChangeLog) =

        let toParagraph (entry: ChangeLogEntry) : MdParagraph =
            [|  match entry.Scope with
                    | Some scope ->  toCompositeSpan [| toStrongEmphasisSpan (scope + ":"); toTextSpan " " |]
                    | None -> upcast MdEmptySpan.Instance : MdSpan
                toTextSpan  entry.Summary; 
                toTextSpan " (";
                toCodeSpan entry.CommitId;
                toTextSpan ")"
            |] |> MdParagraph

        let toListItem (entry: ChangeLogEntry) : MdListItem =
            entry
              |> toParagraph
              |> MdListItem

        let toChangeList (entries: ChangeLogEntry seq) : MdBlock = 
            let list = entries 
                        |> Seq.sortBy (fun e -> e.Date) 
                        |> Seq.map toListItem  
                        |> MdBulletList
            upcast list : MdBlock

        
        let document =
            seq {
                yield h1 "Change Log"

                yield h2 "Overview"

                if not (Seq.isEmpty changeLog.Features) then
                    yield h3 "Features"
                    yield toChangeList changeLog.Features

                if not (Seq.isEmpty changeLog.BugFixes) then
                    yield h3 "Bug Fixes"
                    yield toChangeList changeLog.BugFixes

                if not (Seq.isEmpty changeLog.OtherChanges) then
                    yield h3 "Other Changes"
                    yield changeLog.OtherChanges 
                                    |> List.map (fun (changeType,changes:ChangeLogEntry list) -> [h3 changeType; toChangeList changes]) 
                                    |> List.reduce (fun a b -> a @ b)
                                    |> collapsibleSection "Expand for details"
            }
            |> Array.ofSeq
            |> MdContainerBlock
            |> MdDocument    
        //TODO: After overview, add details for changes: Message body + footers

        document.Save outputPath
        ()
