# Gemini Context - VoletaSharp Project Guidelines

This document provides context, architecture descriptions, and build instructions for AI assistants (especially Gemini) working on the **VoletaSharp** codebase.

---

## 1. Project Overview
**VoletaSharp** is a lightweight arithmetic expression compiler written in F# that compiles mathematical strings into LLVM IR and standalone Native AOT binaries.

- **Source Grammar**:
  ```
  Expr   ::= Expr '+' Term | Expr '-' Term | Term
  Term   ::= Term '*' Factor | Term '/' Factor | Factor
  Factor ::= Digit | '(' Expr ')'
  ```
- **Backend Target**: LLVM IR (32-bit integer result) using **LLVMSharp 5.0.0** static APIs.
- **Compilation Model**: Native Ahead-of-Time (AOT) compilation enabled for the final runner application.

---

## 2. Directory Structure

- [src/VoletaSharp.Compiler/](file:///c:/Users/pullm/source/repos/voletasharp/src/VoletaSharp.Compiler): Core compiler module.
  - [Syntax.fs](file:///c:/Users/pullm/source/repos/voletasharp/src/VoletaSharp.Compiler/Syntax.fs): AST definition and operator types.
  - [Parser.fsy](file:///c:/Users/pullm/source/repos/voletasharp/src/VoletaSharp.Compiler/Parser.fsy): FsYacc parser specification.
  - [Lexer.fsl](file:///c:/Users/pullm/source/repos/voletasharp/src/VoletaSharp.Compiler/Lexer.fsl): FsLex lexer specification.
  - [CodeGen.fs](file:///c:/Users/pullm/source/repos/voletasharp/src/VoletaSharp.Compiler/CodeGen.fs): LLVM IR generation logic.
- [src/VoletaSharp.Backend/](file:///c:/Users/pullm/source/repos/voletasharp/src/VoletaSharp.Backend): Entry-point console application.
  - [Program.fs](file:///c:/Users/pullm/source/repos/voletasharp/src/VoletaSharp.Backend/Program.fs): Driver code that accepts input, invokes the parser, and prints the AST and LLVM IR.
- [src/VoletaSharp.Tests/](file:///c:/Users/pullm/source/repos/voletasharp/src/VoletaSharp.Tests): NUnit & FSUnit unit test suite.

---

## 3. Toolchain & Dependencies

- **.NET SDK**: `.NET 8.0` (or newer).
- **Lexer/Parser**: `FsLexYacc` (v11.3.0) and `FsLexYacc.Runtime`.
- **LLVM Binding**: `LLVMSharp` (v5.0.0). Note that LLVMSharp 5.0.0 uses **static methods on the `LLVM` class** (e.g., `LLVM.CreateBuilderInContext`) rather than object-oriented instance methods.

---

## 4. Common Commands

### Restore & Build Solution
```powershell
dotnet restore
dotnet build
```

### Run Compiler (Manual Testing)
```powershell
dotnet run --project src/VoletaSharp.Backend/VoletaSharp.Backend.fsproj "5 * 5 - (10 + 2)"
```

### Run Tests
```powershell
dotnet test
```

### Publish stand-alone Native AOT executable
```powershell
dotnet publish src/VoletaSharp.Backend/VoletaSharp.Backend.fsproj -r win-x64 -c Release
```

---

## 5. Guidelines for Code Modifications
1. **FsLexYacc Indentation**: Do not add leading spaces or indentation to F# helper blocks `{ ... }` inside `.fsl` or `.fsy` files. Indentations are copied verbatim and will violate F#'s offside rule, causing compile failures in the generated `.fs` files.
2. **Unmanaged String Marshalling**: Native LLVM functions like `LLVM.PrintModuleToString` return `IntPtr`/`nativeint`. Always convert these to standard .NET strings using `System.Runtime.InteropServices.Marshal.PtrToStringAnsi` and free the memory via `LLVM.DisposeMessage`.
3. **AST Updates**: Keep AST modifications in `Syntax.fs` simple and clear. Ensure union constructor names do not conflict with type names.
