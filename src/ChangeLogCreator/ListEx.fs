namespace ChangeLogCreator

module ListEx =

    /// <summary>
    /// Similar to <see cref="List.takeWhile" but <paramref name="predicate" /> 
    //  gets not only passed the current element being checked but also the following, unchecked items of the list
    /// </summary>
    let rec takeWhile (predicate: 'a -> 'a list -> bool) (list: 'a list) : 'a list  =
        match list with 
            // non-empty list -> check if predicate matches, then return head and check rest of the list
            | head::tail ->
                if predicate head tail then
                    head::(takeWhile predicate tail)
                else 
                    []
            // empty list -> return list unchanged
            | [] -> list