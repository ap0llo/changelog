namespace ChangeLogCreator

open Grynwald.MarkdownGenerator;

// Helper functions for generating Markdown (with helper function wrapping the MarkdownGenerator library)
module Markdown =

    let textSpan (content:string) : MdSpan = upcast MdTextSpan content
    
    let codeSpan (content:string) : MdSpan = upcast MdCodeSpan content

    let emphasisSpan (content:string) : MdSpan = upcast MdEmphasisSpan (textSpan content)

    let strongEmphasisSpan (content:string) : MdSpan = upcast MdStrongEmphasisSpan (textSpan content)

    let compositeSpan (content: MdSpan seq) : MdSpan = upcast MdCompositeSpan (Array.ofSeq content)

    let rawMarkdownSpan  (content:string) : MdSpan = upcast MdRawMarkdownSpan content

    let linkSpan  (uri: string) (content:MdSpan) : MdSpan = upcast MdLinkSpan(content, uri)
    
    let heading (level:int) (content:string) (id:string) : MdBlock = 
        let contentSpan = textSpan content
        upcast MdHeading(level, contentSpan, Anchor = id)

    let htmlBlock (html:string) : MdBlock =
        let span = rawMarkdownSpan html
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

    let thematicBreak : MdBlock = upcast MdThematicBreak () : MdBlock
