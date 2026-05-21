namespace VoletaSharp.Compiler

module Syntax =
    type AddOperator =
        | Plus
        | Minus

    type MultiplyOperator =
        | Times
        | DividedBy

    type Factor =
        | Digit of int
        | ParenthesisExpression of Expr

    and Term =
        | MulOp of Term * MultiplyOperator * Factor
        | Factor of Factor

    and Expr =
        | AddOp of Expr * AddOperator * Term
        | Term of Term
