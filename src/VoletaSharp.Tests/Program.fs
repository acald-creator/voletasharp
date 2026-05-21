open NUnit.Framework
open FsUnit
open FSharp.Text.Lexing
open VoletaSharp.Compiler
open VoletaSharp.Compiler.Syntax

[<Test>]
let ``test parsing basic arithmetic expression`` () =
    let input = "2 + 3 * (4 - 1)"
    let lexbuf = LexBuffer<char>.FromString input
    let ast = Parser.start Lexer.tokenize lexbuf
    
    // Check that it parsed successfully into the expected structure
    match ast with
    | AddOp (Term (Factor (Digit 2)), Plus, _) ->
        Assert.Pass()
    | _ ->
        Assert.Fail(sprintf "Unexpected AST: %A" ast)

[<Test>]
let ``test code gen output`` () =
    let input = "10 / 2"
    let lexbuf = LexBuffer<char>.FromString input
    let ast = Parser.start Lexer.tokenize lexbuf
    let ir = CodeGen.compileToIR ast
    
    ir.Contains("voleta_module") |> should be True
    ir.Contains("define i32 @main") |> should be True