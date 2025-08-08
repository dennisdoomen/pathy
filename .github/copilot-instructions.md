# Pathy - Fluent File Path Library for .NET

Pathy is a source-only .NET library providing fluent file and directory path manipulation using chainable operators. It supports .NET 8.0, .NET 4.7, and .NET Standard 2.0/2.1 without binary dependencies.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Required Environment Setup
- .NET SDK 8.0+ is already installed and working
- Git repository must be unshallowed for full functionality:
  ```bash
  git fetch --unshallow  # Takes ~30 seconds. Required for GitVersion.
  ```

### Essential Build Commands (VALIDATED)
These commands are verified to work and include measured timings:

- **Restore dependencies**: `dotnet restore` -- takes 2 seconds. Always run first.
- **Build all projects**: `dotnet build --configuration Release --no-restore` -- takes 4 seconds. Builds all target frameworks.
- **Run API verification tests**: `dotnet test Pathy.ApiVerificationTests/Pathy.ApiVerificationTests.csproj --configuration Debug` -- takes 4 seconds. 8 tests, all pass.
- **Create NuGet packages**: `dotnet pack --configuration Release --output Artifacts --no-build -p:Version=1.0.0-test` -- takes 2 seconds. Generates Pathy and Pathy.Globbing packages.

### CRITICAL Build Issues and Workarounds
- **NUKE Build Scripts are BROKEN**: Both `build.ps1` and `build.sh` have incorrect paths (`build/_build.csproj` instead of `Build/_build.csproj`) and GitVersion failures.
- **DO NOT use**: `./build.ps1`, `./build.sh`, or `nuke` commands. They will fail.
- **Main unit tests have failures**: `Pathy.Specs` has 6 failing tests on Linux due to platform-specific path handling. API tests work perfectly.
- **NEVER try to run .NET Framework tests on Linux**: They require mono and will fail.

### Working Test Commands
- **API verification (recommended)**: `dotnet test Pathy.ApiVerificationTests/Pathy.ApiVerificationTests.csproj --configuration Debug --verbosity normal` -- takes 4 seconds. NEVER CANCEL.
- **Specific framework tests**: `dotnet test Pathy.Specs/Pathy.Specs.csproj --framework net8.0 --configuration Debug` -- 50 pass, 6 fail (expected on Linux).

## Validation Scenarios

### ALWAYS Test Core Functionality After Changes
Create a test program to validate the library works:

```csharp
// Test program template - save as /tmp/test_pathy/Program.cs
using System;
using Pathy;

class Program
{
    static void Main()
    {
        // Test basic path operations  
        var currentDir = ChainablePath.Current;
        var tempDir = ChainablePath.Temp;
        
        // Test path chaining
        var testPath = ChainablePath.New / "test" / "directory" / "file.txt";
        Console.WriteLine($"Chained path: {testPath}");
        
        // Test path methods
        var path = ChainablePath.From("/tmp") / "test";
        Console.WriteLine($"Exists: {path.Exists}, IsDirectory: {path.IsDirectory}");
        
        // Test extension checking
        var filePath = ChainablePath.From("/tmp/test.txt");  
        Console.WriteLine($"Has .txt extension: {filePath.HasExtension(".txt")}");
        
        Console.WriteLine("Pathy validation successful!");
    }
}
```

**Create and run validation**:
```bash
mkdir -p /tmp/test_pathy
# Create Program.cs and project file with Pathy project references
dotnet run --project /tmp/test_pathy/test_pathy.csproj  # Takes 2 seconds
```

### API Change Validation
- **Before changing APIs**: Run `dotnet test Pathy.ApiVerificationTests/` to establish baseline
- **After API changes**: Run tests again, then use `AcceptApiChanges.ps1` (Windows) or `AcceptApiChanges.sh` (Linux) to accept changes
- **ALWAYS verify**: API changes are intentional and documented

## Project Structure

### Key Projects (Repository Root)
- `Pathy/` - Main library with ChainablePath class
- `Pathy.Globbing/` - File globbing extension (separate package)  
- `Pathy.Specs/` - Unit tests using xUnit and FluentAssertions
- `Pathy.ApiVerificationTests/` - API approval tests using Verify
- `Build/` - NUKE build system (contains broken scripts)

### Important Files
- `Pathy.sln` - Main solution file
- `global.json` - Specifies .NET SDK 8.0
- `AcceptApiChanges.ps1`/`.sh` - API approval scripts
- `.github/workflows/build.yml` - CI/CD pipeline (works in CI)

## Common Tasks

### Building for Development
```bash
dotnet restore                                          # 2 seconds
dotnet build --configuration Debug --no-restore        # 4 seconds  
```

### Building for Release  
```bash
dotnet restore                                          # 2 seconds
dotnet build --configuration Release --no-restore      # 4 seconds
dotnet pack --configuration Release --output Artifacts --no-build -p:Version=1.0.0-dev  # 2 seconds
```

### Running Tests
```bash
# API verification (most reliable)
dotnet test Pathy.ApiVerificationTests/ --configuration Debug  # 4 seconds, 8/8 pass

# Unit tests (.NET 8 only, some failures expected on Linux)  
dotnet test Pathy.Specs/ --framework net8.0 --configuration Debug  # 4 seconds, 50/56 pass
```

### Making API Changes
```bash
# 1. Run baseline API tests
dotnet test Pathy.ApiVerificationTests/

# 2. Make your changes

# 3. Run tests again (will show differences)
dotnet test Pathy.ApiVerificationTests/

# 4. If changes are intentional, accept them
./AcceptApiChanges.sh  # Linux
# or 
./AcceptApiChanges.ps1  # Windows
```

## Framework Support and Multi-Targeting
The library targets:
- .NET 8.0 (primary)
- .NET Framework 4.7 
- .NET Standard 2.0
- .NET Standard 2.1

**Build outputs**: All target frameworks build successfully on Linux. Testing only works reliably on .NET 8.0 for Linux environments.

## DO NOT
- Use `build.ps1`, `build.sh`, or `nuke` commands (they are broken)
- Expect all unit tests to pass on Linux (platform differences cause 6 test failures)
- Try to run .NET Framework tests on Linux without mono
- Cancel builds - they complete quickly (under 10 seconds each)