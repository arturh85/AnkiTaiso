# Getting Started

## 🥚 Getting Started

Requirements:

- [Dotnet 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Godot 4.4 Mono](https://godotengine.org/download/)

Then install [GodotEnv](https://github.com/chickensoft-games/GodotEnv) which manages our addons:

    dotnet tool install --global Chickensoft.GodotEnv

And install all addons with this command:

    godotenv addons install

## 🏝 Environment Setup

For the provided debug configurations and test coverage to work correctly, you must setup your development environment correctly. The [Chickensoft Setup Docs][setup-docs] describe how to setup your Godot and C# development environment, using Chickensoft's best practice recommendations.

### VSCode Settings

This template includes some Visual Studio Code settings in `.vscode/settings.json`. The settings facilitate terminal environments on Windows (Git Bash, PowerShell, Command Prompt) and macOS (zsh), as well as fixing some syntax colorization issues that Omnisharp suffers from. You'll also find settings that enable editor config support in Omnisharp and the .NET Roslyn analyzers for a more enjoyable coding experience.

> Please double-check that the provided VSCode settings don't conflict with your existing settings.

## 🏁 Application Entry Point

The `Main.tscn` and `Main.cs` scene and script file are the entry point of the game. In general, you probably won't need to modify these unless you're doing something highly custom.

If the game is running a release build, the `Main.cs` file will just immediately change the scene to `src/Game.tscn`. If the game is running in debug mode *and* GoDotTest has received the correct command line arguments to begin testing, the game will switch to the testing scene and hand off control to GoDotTest to run the game's tests.

In general, prefer editing `src/Game.tscn` over `src/Main.tscn`.

The provided debug configurations in `.vscode/launch.json` allow you to easily debug tests (or just the currently open test, provided its filename matches its class name).



## ⏯ Running the Project

Several launch profiles are included for Visual Studio Code:

- 🕹 **Debug Game**

  Runs the game in debug mode, allowing you to set breakpoints and inspect variables.

- 🎭 **Debug Current Scene**

  Debugs the game and loads the scene with the **same name** and **in the same path** as the C# file that's actively selected in VSCode: e.g., a scene named `MyScene.tscn` must reside in the same directory as `MyScene.cs`, and you must have selected `MyScene.cs` as the active tab in VSCode before running the launch profile.

  If GoDotTest is able to find a `.tscn` file with the same name in the same location, it will run the game in debug mode and load the scene.

  > Naturally, Chickensoft recommends naming scenes after the C# script they use and keeping them in the same directory so that you can take advantage of this launch profile.
  >
  > ⚠️ It's very easy to rename a script class but forget to rename the scene file, or vice-versa. When that happens, this launch profile will pass in the *expected* name of the scene file based on the script's name, but Godot will fail to find a scene with that name since the script name and scene name are not the same.

- 🧪 **Debug Tests**

  Runs the game in debug mode, specifying the command line flags needed by GoDotTest to run the tests. Debugging works the same as usual, allowing you to set breakpoints within the game's C# test files.

- 🔬 **Debug Current Test**

  Debugs the game and loads the test class with the **same name** as the C# file that's actively selected in VSCode: e.g., a test file named `MyTest.cs` must contain a test class named `MyTest`, and you must have selected `MyTest.cs` as the active tab in VSCode before running the launch profile.

  > ⚠️ It's very easy to rename a test class but forget to rename the test file, or vice-versa. When that happens, this launch profile will pass in the name of the file but GoDotTest will fail to find a class with that name since the filename and class name are not the same.

Note that each launch profile will trigger a build (see `./.vscode/tasks.json`) before debugging the game.

> ⚠️ **Important:** You must setup a `GODOT` environment variable for the launch configurations above. If you haven't done so already, please see the [Chickensoft Setup Docs][setup-docs].
