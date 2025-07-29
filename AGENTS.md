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
  Uses .NET SDK 8.0.403 (see `global.json`). Update SDK or global.json if build fails.

## Code Style Guidelines

- **Imports:**  
  Place `using` directives outside namespace (see `stylecop.json`).
- **Formatting:**  
  Follow StyleCop rules. No enforced XML documentation.  
  Indent with spaces, keep lines concise.
- **Types:**  
  Use explicit types for public APIs. Prefer interfaces for DI.
- **Naming:**  
  Use PascalCase for classes/types, camelCase for parameters/fields.
- **Error Handling:**  
  Use Polly for HTTP retry policies. Handle transient errors gracefully.
- **Tests:**  
  Use xUnit, Moq. Test projects in `tests/`.
- **Project Structure:**  
  Organize by feature (BotClients, Core, Drawing, Functions).
- **Configuration:**  
  Store secrets in `local.settings.json` or via dotnet-secrets.
- **Copilot/Cursor:**  
  No Copilot or Cursor rules detected.
