namespace ChangeLogCreator

open System.Text

module MessageParser =
               
    // Tokenizer types
    type Token =
        | StringToken of string
        | OpenParenthesisToken
        | CloseParenthesisToken
        | ColonToken
        | LineBreakToken
        | SpaceToken
        | ExclamationMarkToken
        | EofToken
    
    let tokenize (input: string) =
        seq {

            let getStringTokenAndReset (stringBuilder: StringBuilder) =
                let value = stringBuilder.ToString()
                stringBuilder.Clear() |> ignore
                (StringToken value)

            let currentValueBuilder = StringBuilder()

            let getSingleCharToken char =
                match char with 
                    | ':' -> Some ColonToken
                    | '(' -> Some OpenParenthesisToken
                    | ')' -> Some CloseParenthesisToken
                    | ' ' -> Some SpaceToken
                    | '!' -> Some ExclamationMarkToken
                    | _ -> None
           
            let mutable i = 0
            while i < input.Length  do
                let nextChar =  if i + 1 < input.Length then Some input.[i+1] else None
                let currentCharToken = getSingleCharToken input.[i]                

                match input.[i], nextChar, currentCharToken with                    
                    | _, _, Some matchedCharToken ->
                        if currentValueBuilder.Length > 0 then
                            yield getStringTokenAndReset currentValueBuilder
                        yield matchedCharToken
                    | '\r', Some '\n', _ ->
                        if currentValueBuilder.Length > 0 then
                            yield getStringTokenAndReset currentValueBuilder
                        yield LineBreakToken
                        // next already matched => skip next iteration
                        i <- i + 1
                    | '\n', _, _ -> 
                        if currentValueBuilder.Length > 0 then
                            yield getStringTokenAndReset currentValueBuilder
                        yield LineBreakToken
                    | _ ->  currentValueBuilder.Append(input.[i]) |> ignore

                i <- i + 1

            yield EofToken
        }