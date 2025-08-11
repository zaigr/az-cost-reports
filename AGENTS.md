# AGENTS.md

## Build, Lint, and Test Commands

- **Build solution:**  
  `dotnet build`
- **Run all tests:**  
  `dotnet test`
- **Run a single test class:**  
  `dotnet test tests/AzCostTgBot.Core.Tests.Unit/AzCostTgBot.Core.Tests.Unit.csproj --filter FullyQualifiedName~<TestClassName>`
- **Run locally:**  
  See `README.md` for local.settings.json and dotnet-secrets usage.
- **SDK version:**  
  Uses .NET SDK 8.0.403 (see `global.json`).

## Code Style Guidelines

- **Imports:** Place `using` directives outside namespaces; prefer single-line and file-scoped namespaces.
- **Formatting:** Follow `.editorconfig` and StyleCop; indent with spaces; keep lines concise.
- **Types:** Use explicit types for public APIs; prefer interfaces for DI; enable nullable reference types.
- **Naming:** PascalCase for types/methods/public members; camelCase for fields/locals; prefix interfaces with "I".
- **Error Handling:** Use Polly for HTTP retries; handle edge cases; use global exception middleware for APIs.
- **Comments:** Write clear comments for functions and design decisions; use XML docs for public APIs.
- **Testing:** Use xUnit and Moq; test projects in `tests/`; follow existing test naming conventions.
- **Configuration:** Store secrets in `local.settings.json` or via dotnet-secrets.
- **Copilot/Cursor:** See `.github/copilot-instructions.md` for extended rules; no Cursor rules detected.
