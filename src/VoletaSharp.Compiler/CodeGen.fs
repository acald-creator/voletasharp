namespace VoletaSharp.Compiler

module CodeGen =
    open LLVMSharp
    open VoletaSharp.Compiler.Syntax

    let rec codegenFactor (context: LLVMContextRef) (builder: LLVMBuilderRef) (factor: Factor) : LLVMValueRef =
        match factor with
        | Digit n ->
            LLVM.ConstInt(LLVM.Int32TypeInContext(context), uint64 n, false)
        | ParenthesisExpression expr ->
            codegenExpr context builder expr

    and codegenTerm (context: LLVMContextRef) (builder: LLVMBuilderRef) (term: Term) : LLVMValueRef =
        match term with
        | Factor f ->
            codegenFactor context builder f
        | MulOp (left, op, right) ->
            let lhs = codegenTerm context builder left
            let rhs = codegenFactor context builder right
            match op with
            | Times -> LLVM.BuildMul(builder, lhs, rhs, "multmp")
            | DividedBy -> LLVM.BuildSDiv(builder, lhs, rhs, "sdivtmp")

    and codegenExpr (context: LLVMContextRef) (builder: LLVMBuilderRef) (expr: Expr) : LLVMValueRef =
        match expr with
        | Term t ->
            codegenTerm context builder t
        | AddOp (left, op, right) ->
            let lhs = codegenExpr context builder left
            let rhs = codegenTerm context builder right
            match op with
            | Plus -> LLVM.BuildAdd(builder, lhs, rhs, "addtmp")
            | Minus -> LLVM.BuildSub(builder, lhs, rhs, "subtmp")

    let compileToIR (expr: Expr) : string =
        let context = LLVM.GetGlobalContext()
        let themodule = LLVM.ModuleCreateWithNameInContext("voleta_module", context)
        let builder = LLVM.CreateBuilderInContext(context)

        // Define: int main()
        let fnType = LLVM.FunctionType(LLVM.Int32TypeInContext(context), [||], false)
        let mainFn = LLVM.AddFunction(themodule, "main", fnType)
        let entryBlock = LLVM.AppendBasicBlock(mainFn, "entry")
        LLVM.PositionBuilderAtEnd(builder, entryBlock)

        // Generate AST instructions
        let value = codegenExpr context builder expr
        LLVM.BuildRet(builder, value) |> ignore

        // Output IR to string
        let irPtr = LLVM.PrintModuleToString(themodule)
        let irString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(irPtr)
        LLVM.DisposeMessage(irPtr)

        // Clean up
        LLVM.DisposeBuilder(builder)
        LLVM.DisposeModule(themodule)
        irString
