namespace VoletaSharp.Compiler

module Syntax =
    type Factor =
        | Digit of int
        | ParenthesisExpression of Expression

    and AddOperator =
        | Plus
        | Minus

    and MultiplyOperator =
        | Times
        | DividedBy

    and Term =
        | MultiplyOperator of Term * Factor
        | Factor of Factor

    and Expr =
        | AddOperator of Expression * Term
        | Term of Term
