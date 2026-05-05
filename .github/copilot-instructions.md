# Copilot instructions for SerialCommunicationVS

## Purpose
Short, focused guidance for Copilot sessions: how to build/run this project, high-level architecture, and repository-specific conventions.

## Build / Run / Test / Lint
- Build (Developer Command Prompt / MSBuild):
  - msbuild ".\SerialCommunication\SerialCommunication.csproj" /p:Configuration=Debug
  - msbuild ".\SerialCommunication\SerialCommunication.csproj" /t:Rebuild /p:Configuration=Release
- Quick run (after build):
  - .\SerialCommunication\bin\Debug\SerialCommunication.exe
- dotnet CLI: not an SDK-style project (targets .NET Framework 4.7.2). dotnet build may work only if you have the appropriate targeting packs installed; prefer msbuild/Visual Studio.
- Tests: no test projects found. To run a single test (if tests are later added): dotnet test --filter "FullyQualifiedName~MyTestName" or use msbuild/VSTest arguments.
- Lint/format: none configured. Optionally run dotnet format if adding SDK-style tooling.

## High-level architecture
- Single Windows Forms application (WinForms) in project SerialCommunication targeting .NET Framework 4.7.2.
- Entry point: Program.Main -> Application.Run(new Form1()).
- UI & behavior: Form1 (Form1.cs + Form1.Designer.cs) contains UI controls and event handlers; Designer file is auto-generated.
- Serial I/O: uses System.IO.Ports.SerialPort (see Form1.cs). Port enumeration is done with SerialPort.GetPortNames(); default baud shown in UI is 115200.
- Resources: images and .resx files live under Resources/ and Properties/ and are embedded per the csproj.

## Key conventions and repo-specific notes
- Naming: UI controls use a mix of English and Dutch identifiers (e.g., comboBoxPoort for port). Search terms should include both languages.
- Do NOT edit Form1.Designer.cs manually — change the form via the WinForms designer or Form1.cs partial class.
- Project file is non-SDK (classic) csproj; prefer Visual Studio or MSBuild for builds.
- Binary outputs: in\Debug\ and in\Release\ as set in the csproj.
- If adding tests/tools, prefer creating separate projects (e.g., *.Tests) and add them to source control.

## Other AI assistant configs
Checked for common assistant config files (CLAUDE.md, .cursorrules, AGENTS.md, .windsurfrules, CONVENTIONS.md, etc.) — none were found in the repository root.

---
Created by an automated Copilot assistant to help future sessions start quickly. Keep this file focused and update only for repository-specific workflow changes.
