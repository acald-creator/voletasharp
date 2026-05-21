
module VoletaSharp.Compiler.Lexer
open FSharp.Text.Lexing
open VoletaSharp.Compiler.Parser/// Rule tokenize
val tokenize: lexbuf: LexBuffer<char> -> token
