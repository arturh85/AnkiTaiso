### 🚥 Tests

Tests run directly inside the GitHub runner machine (using [chickensoft-games/setup-godot]) on every push to the repository. If the tests fail to pass, the workflow will also fail to pass.

You can configure which simulated graphics environments (`vulkan` and/or `opengl3`) you want to run tests on in [`.github/workflows/visual_tests.yaml`](.github/workflows/visual_tests.yaml).

Currently, tests can only be run from the `ubuntu` runners. If you know how to make the workflow install mesa and a virtual window manager on macOS and Windows, we'd love to hear from you!

Tests are executed by running the Godot test project in `anki-taiso` from the command line and passing in the relevant arguments to Godot so that [GoDotTest] can discover and run tests.


## 👷 Testing

An example test is included in `test/src/GameTest.cs` that demonstrates how to write a test for your package using [GoDotTest] and [godot-test-driver].

> [GoDotTest] is an easy-to-use testing framework for Godot and C# that allows you to run tests from the command line, collect code coverage, and debug tests in VSCode.

Tests run directly inside the game. The `.csproj` file is already pre-configured to prevent test scripts and test-only package dependencies from being included in release builds of your game!

On CI/CD, software graphics drivers from [mesa] emulate a virtual graphics device for Godot to render to, allowing you to run visual tests in a headless environment.


## 🚦 Test Coverage

Code coverage requires a few `dotnet` global tools to be installed first. You should install these tools from the root of the project directory.

The `nuget.config` file in the root of the project allows the correct version of `coverlet` to be installed from the coverlet nightly distributions. Overriding the coverlet version will be required [until coverlet releases a stable version with the fixes that allow it to work with Godot 4][coverlet-issues].

```sh
dotnet tool install --global coverlet.console
dotnet tool update --global coverlet.console
dotnet tool install --global dotnet-reportgenerator-globaltool
dotnet tool update --global dotnet-reportgenerator-globaltool
```

> Running `dotnet tool update` for the global tool is often necessary on Apple Silicon computers to ensure the tools are installed correctly.

You can collect code coverage and generate coverage badges by running the bash script `coverage.sh` (on Windows, you can use the Git Bash shell that comes with git).

```sh
# Must give coverage script permission to run the first time it is used.
chmod +x ./coverage.sh

# Run code coverage:
./coverage.sh
```

You can also run test coverage through VSCode by opening the command palette and selecting `Tasks: Run Task` and then choosing `coverage`.

If you are having trouble with `coverlet` finding your .NET runtime on Windows, you can use the PowerShell Script `coverage.ps1` instead.

```ps
.\coverage.ps1
```
