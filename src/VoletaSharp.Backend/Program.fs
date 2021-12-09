open System

[<EntryPoint>]
let main argv =
    let who = if argv.Length = 0 then "F#" else argv.[0]
    printfn "Hello world from %s" who
    0