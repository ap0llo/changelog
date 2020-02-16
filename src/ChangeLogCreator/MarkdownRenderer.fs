namespace ChangeLogCreator

open Grynwald.MarkdownGenerator
open ChangeLogCreator.Markdown;

module MarkdownRenderer =
   
    let private getChangeLogForVersion (headingLevel : int) (versionChangeLog : VersionChangeLog) : MdBlock =

        let convertParagraph (paragraph:Paragraph) : MdBlock =
               let (Paragraph content) = paragraph
               upcast MdParagraph (textSpan content)

        let getEntryHtmlId  entry =
            let heading = new MdHeading(1, textSpan entry.CommitId)
            "heading-" + heading.Anchor

        let h1 = heading headingLevel
        let h2 = heading (headingLevel + 1)
        let h3 = heading (headingLevel + 2)
        let h4 = heading (headingLevel + 3)
        
        let sortChanges = Seq.sortBy (fun e -> e.Date) 

        let isEmpty versionChangeLog =
            versionChangeLog.Features |> List.isEmpty &&
            versionChangeLog.BugFixes |> List.isEmpty    
 
        let scopeAndDescriptionSpan (entry: ChangeLogEntry) : MdSpan =
            compositeSpan ([  
                match entry.Scope with
                    | Some scope ->  compositeSpan [| strongEmphasisSpan (scope + ":"); textSpan " " |]
                    | None -> upcast MdEmptySpan.Instance : MdSpan
                textSpan entry.Summary; 
            ]) 

        let changeList (entries: ChangeLogEntry seq) : MdBlock = 
            let toListItem (entry: ChangeLogEntry) : MdListItem =       
                 let toParagraph (entry: ChangeLogEntry) : MdParagraph = 
                    let text = scopeAndDescriptionSpan entry                    
                    let anchor = getEntryHtmlId entry
                    MdParagraph [| linkSpan ("#" + anchor) text |]
                 entry
                   |> toParagraph
                   |> MdListItem

            let list = entries 
                        |> sortChanges
                        |> Seq.map toListItem  
                        |> MdBulletList
            upcast list : MdBlock
             
        let changeDetailSections (entries: ChangeLogEntry seq) : MdBlock =
            //TODO: Footers
            let changeDetailSection (entry: ChangeLogEntry) : MdBlock = 
                containerBlock(seq {
                    yield new MdHeading(4, scopeAndDescriptionSpan entry, Anchor = getEntryHtmlId entry)
                    yield MdParagraph (compositeSpan [textSpan "Commit: "; codeSpan entry.CommitId])

                    yield entry.Body 
                            |> Seq.map convertParagraph 
                            |> containerBlock
                    yield thematicBreak 
                })
            entries 
                |> sortChanges
                |> Seq.map changeDetailSection 
                |> containerBlock

        //TODO: Use simpler layout if there is only a single change in the changelog
        containerBlock (seq {
                let versionString = versionChangeLog.VersionInfo.Version.ToNormalizedString()
                let headingIdPrefix = "heading-v" + (new MdHeading(1, textSpan versionString)).Anchor

                yield h1 versionString headingIdPrefix

                if isEmpty versionChangeLog then
                    yield MdParagraph (emphasisSpan "No changes found")
                else
                    yield h2 "Overview" (headingIdPrefix + "-overview")

                    if not (List.isEmpty versionChangeLog.Features) then
                        yield h3 "Features" (headingIdPrefix + "-overview-features")
                        yield changeList versionChangeLog.Features

                    if not (List.isEmpty versionChangeLog.BugFixes) then
                        yield h3 "Bug Fixes" (headingIdPrefix + "-overview-bugfixes")
                        yield changeList versionChangeLog.BugFixes
                    
                    yield thematicBreak 
                    yield h2 "Details" (headingIdPrefix + "-details")

                    if not (List.isEmpty versionChangeLog.Features) then
                        yield h3 "Features"  (headingIdPrefix + "-details-features")
                        yield changeDetailSections versionChangeLog.Features 

                    if not (List.isEmpty versionChangeLog.BugFixes) then
                        yield h3 "Bug Fixes"  (headingIdPrefix + "-details-bugfixes")
                        yield changeDetailSections versionChangeLog.BugFixes

                //TODO: Ensure changes are ordered the same way in both overview and details
            })
   
    let serializationOptions : MdSerializationOptions =
        new MdSerializationOptions(HeadingAnchorStyle = MdHeadingAnchorStyle.Tag)

    /// Renders a change log for the specified version
    let renderVersionChangeLog (changeLog : VersionChangeLog) : MdDocument = getChangeLogForVersion 1 changeLog |> MdDocument
    
    /// Renders a full change log for the application
    let renderChangeLog (changeLog : ChangeLog) : MdDocument =
            seq {        
                yield heading 1 "Change Log" "heading-changelog"

                yield changeLog.Versions 
                        |> Seq.sortByDescending (fun x -> x.VersionInfo.Version)
                        |> Seq.map (getChangeLogForVersion 2)
                        |> containerBlock
            }  
            |> containerBlock
            |> MdDocument
