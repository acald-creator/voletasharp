open System
open FSharp.Text.Lexing
open VoletaSharp.Compiler

let readExpression (input: string) : string =
    if String.IsNullOrWhiteSpace(input) then
        "Please provide an expression to parse."
    else
        try
            let lexbuf = LexBuffer<char>.FromString input
            let ast = Parser.start Lexer.tokenize lexbuf
            let ir = CodeGen.compileToIR ast
            sprintf "Parsed AST: %A\n\nGenerated LLVM IR:\n%s" ast ir
        with
        | ex -> sprintf "Error: %s" ex.Message

[<EntryPoint>]
let main argv =
    let input = if argv.Length = 0 then "" else argv.[0]
    let result = readExpression input
    printfn "%s\n" result
    0