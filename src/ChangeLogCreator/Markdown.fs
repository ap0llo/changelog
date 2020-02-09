namespace ChangeLogCreator

open Grynwald.MarkdownGenerator;

// Helper functions for generating Markdown (with helper function wrapping the MarkdownGenerator library)
module Markdown =

    let toTextSpan (content:string) : MdSpan = upcast MdTextSpan content
    
    let toCodeSpan (content:string) : MdSpan = upcast MdCodeSpan content

    let toStrongEmphasisSpan (content:string) : MdSpan = upcast MdStrongEmphasisSpan (toTextSpan content)

    let toCompositeSpan (content: MdSpan seq) : MdSpan = upcast MdCompositeSpan (Array.ofSeq content)

    let toRawMarkdownSpan  (content:string) : MdSpan = upcast MdRawMarkdownSpan content

    let heading (level:int) (content:string) : MdBlock = 
        let contentSpan = toTextSpan content
        upcast MdHeading(level, contentSpan)

    let h1 = heading 1
    let h2 = heading 2
    let h3 = heading 3
    let h4 = heading 4

    let htmlBlock (html:string) : MdBlock =
        let span = toRawMarkdownSpan html
        upcast MdParagraph [| span |]

    let containerBlock (content: MdBlock seq) : MdBlock =
        let container = content
                        |> Array.ofSeq
                        |> MdContainerBlock
        upcast container : MdBlock

    let collapsibleSection (summary:string) (content:MdBlock seq) : MdBlock = 
        let blocks = seq {
            yield htmlBlock "<details>"
            yield htmlBlock (sprintf "  <summary>%s</summary>" summary)

            for innerBlock in content do
                yield innerBlock

            yield htmlBlock "</details>"
        } 
        blocks |> containerBlock
