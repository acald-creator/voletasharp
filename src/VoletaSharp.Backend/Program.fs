open System



[<EntryPoint>]
let main argv =
    let input = if argv.Length = 0 then "" else argv.[0]
    let result = readExpression input
    printfn "%s\n" result
    0