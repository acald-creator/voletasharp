open NUnit.Framework
open FsUnit

[<Test>]
let ``test hello`` () =
    5 + 1 |> should equal 6