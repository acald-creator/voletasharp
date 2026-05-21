#nowarn "9"

namespace VoletaSharp.Compiler

module CodeGen =
    open LLVMSharp.Interop
    open Microsoft.FSharp.NativeInterop
    open VoletaSharp.Compiler.Syntax

    let withString (s: string) (f: nativeptr<sbyte> -> 'a) : 'a =
        let bytes = System.Text.Encoding.UTF8.GetBytes(s + "\u0000")
        use p = fixed bytes
        let sptr = p |> NativePtr.toVoidPtr |> NativePtr.ofVoidPtr
        f sptr

    let rec codegenFactor (context: nativeptr<LLVMOpaqueContext>) (builder: nativeptr<LLVMOpaqueBuilder>) (factor: Factor) : nativeptr<LLVMOpaqueValue> =
        match factor with
        | Digit n ->
            LLVM.ConstInt(LLVM.Int32TypeInContext(context), uint64 n, 0)
        | ParenthesisExpression expr ->
            codegenExpr context builder expr

    and codegenTerm (context: nativeptr<LLVMOpaqueContext>) (builder: nativeptr<LLVMOpaqueBuilder>) (term: Term) : nativeptr<LLVMOpaqueValue> =
        match term with
        | Factor f ->
            codegenFactor context builder f
        | MulOp (left, op, right) ->
            let lhs = codegenTerm context builder left
            let rhs = codegenFactor context builder right
            match op with
            | Times -> withString "multmp" (fun name -> LLVM.BuildMul(builder, lhs, rhs, name))
            | DividedBy -> withString "sdivtmp" (fun name -> LLVM.BuildSDiv(builder, lhs, rhs, name))

    and codegenExpr (context: nativeptr<LLVMOpaqueContext>) (builder: nativeptr<LLVMOpaqueBuilder>) (expr: Expr) : nativeptr<LLVMOpaqueValue> =
        match expr with
        | Term t ->
            codegenTerm context builder t
        | AddOp (left, op, right) ->
            let lhs = codegenExpr context builder left
            let rhs = codegenTerm context builder right
            match op with
            | Plus -> withString "addtmp" (fun name -> LLVM.BuildAdd(builder, lhs, rhs, name))
            | Minus -> withString "subtmp" (fun name -> LLVM.BuildSub(builder, lhs, rhs, name))

    let compileToIR (expr: Expr) : string =
        let context = LLVM.GetGlobalContext()
        let themodule = withString "voleta_module" (fun name -> LLVM.ModuleCreateWithNameInContext(name, context))
        let builder = LLVM.CreateBuilderInContext(context)

        // Define: int main()
        let paramTypes : nativeptr<LLVMOpaqueType>[] = [||]
        use paramTypesPtr = fixed paramTypes
        let fnType = LLVM.FunctionType(LLVM.Int32TypeInContext(context), paramTypesPtr, 0u, 0)
        let mainFn = withString "main" (fun name -> LLVM.AddFunction(themodule, name, fnType))
        let entryBlock = withString "entry" (fun name -> LLVM.AppendBasicBlock(mainFn, name))
        LLVM.PositionBuilderAtEnd(builder, entryBlock)

        // Generate AST instructions
        let value = codegenExpr context builder expr
        LLVM.BuildRet(builder, value) |> ignore

        // Output IR to string
        let irPtr = LLVM.PrintModuleToString(themodule)
        let irString = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(System.IntPtr(irPtr |> NativePtr.toVoidPtr))
        LLVM.DisposeMessage(irPtr)

        // Clean up
        LLVM.DisposeBuilder(builder)
        LLVM.DisposeModule(themodule)
        irString
