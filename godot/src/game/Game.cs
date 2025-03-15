namespace ankitaiso.game;

using System;
using System.IO.Abstractions;
using System.Text.Json;
using ankitaiso.domain;
using app.domain;
using Chickensoft.AutoInject;
using Chickensoft.Collections;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.Log;
using Chickensoft.Log.Godot;
using Chickensoft.LogicBlocks;
using Chickensoft.SaveFileBuilder;
using Chickensoft.Serialization;
using Chickensoft.Serialization.Godot;
using domain;
using game_typing;
using game_typing.domain;
using Godot;
using Microsoft.EntityFrameworkCore.Storage;
using state;
using MapData = map.MapData;
using PlayerCameraData = player_camera.PlayerCameraData;
using PlayerData = player.PlayerData;

public interface IGame : INode3D,
IProvide<IGameRepo>, IProvide<ISaveChunk<GameData>>, IProvide<EntityTable> {
  void LoadExistingGame();

  event Game.SaveFileLoadedEventHandler? SaveFileLoaded;
}

[Meta(typeof(IAutoNode))]
[SceneTree]
public partial class Game : Node3D, IGame {
  public override void _Notification(int what) => this.Notify(what);
  private readonly Log _log = new (nameof(Game), new GDWriter());

  #region Save
  [Signal]
  public delegate void SaveFileLoadedEventHandler();
  public JsonSerializerOptions JsonOptions { get; set; } = default!;
  public const string SAVE_FILE_NAME = "game.json";
  public IFileSystem FileSystem { get; set; } = default!;
  public IEnvironmentProvider Environment { get; set; } = default!;
  public string SaveFilePath { get; set; } = default!;
  public EntityTable EntityTable { get; set; } = new();
  EntityTable IProvide<EntityTable>.Value() => EntityTable;
  public ISaveFile<GameData> SaveFile { get; set; } = default!;
  public ISaveChunk<GameData> GameChunk { get; set; } = default!;
  ISaveChunk<GameData> IProvide<ISaveChunk<GameData>>.Value() => GameChunk;
  #endregion Save

  #region State

  public IGameRepo GameRepo { get; set; } = default!;
  public IGameLogic GameLogic { get; set; } = default!;

  public LogicBlock<GameLogic.State>.IBinding GameBinding { get; set; } = default!;

  #endregion State

  #region Provisions

  IGameRepo IProvide<IGameRepo>.Value() => GameRepo;

  #endregion Provisions

  #region Dependencies

  [Dependency] public IAppRepo AppRepo => DependentExtensions.DependOn<IAppRepo>(this);
  [Dependency] public GameTypingSystem GameTypingSystem => DependentExtensions.DependOn<GameTypingSystem>(this);
  [Dependency] public IDatabaseRepo DatabaseRepo => DependentExtensions.DependOn<IDatabaseRepo>(this);
  [Dependency] public IGameTypingRepo GameTypingRepo => DependentExtensions.DependOn<IGameTypingRepo>(this);

  #endregion Dependencies

  public void Setup() {
    FileSystem = new FileSystem();

    SaveFilePath = FileSystem.Path.Join(OS.GetUserDataDir(), SAVE_FILE_NAME);

    GameRepo = new GameRepo();
    GameLogic = new GameLogic();
    GameLogic.Set(GameRepo);
    GameLogic.Set(AppRepo);
    GameTyping.PlayerPosition = Player.Position;
    GameTyping.SpawnPosition = SpawnLocation.Position;

    // This is how to create JsonSerializerOptions for use with LogicBlocks
    // and the Chickensoft serialization utilities.
    var resolver = new SerializableTypeResolver();
    // Tell our type type resolver about the Godot-specific converters.
    GodotSerialization.Setup();

    var upgradeDependencies = new Blackboard();

    // Create a standard JsonSerializerOptions with our introspective type
    // resolver and the logic blocks converter.
    JsonOptions = new JsonSerializerOptions {
      Converters = {
        new SerializableTypeConverter(upgradeDependencies)
      },
      TypeInfoResolver = resolver,
      WriteIndented = true
    };

    DeathMenu.TryAgain += OnStart;
    DeathMenu.MainMenu += OnMainMenu;
    DeathMenu.TransitionCompleted += OnDeathMenuTransitioned;

    WinMenu.MainMenu += OnMainMenu;
    WinMenu.TransitionCompleted += OnWinMenuTransitioned;

    PauseMenu.MainMenu += OnMainMenu;
    PauseMenu.Resume += OnResume;
    PauseMenu.TransitionCompleted += OnPauseMenuTransitioned;
    PauseMenu.Save += OnPauseMenuSaveRequested;
    GameTypingSystem.OnWon += OnGameWon;

    GameChunk = new SaveChunk<GameData>(
      (chunk) => {
        var gameData = new GameData() {
          MapData = chunk.GetChunkSaveData<MapData>(),
          PlayerData = chunk.GetChunkSaveData<PlayerData>(),
          PlayerCameraData = chunk.GetChunkSaveData<PlayerCameraData>()
        };

        return gameData;
      },
        onLoad: (chunk, data) => {
          // chunk.LoadChunkSaveData(data.MapData);
          // chunk.LoadChunkSaveData(data.PlayerData);
          // chunk.LoadChunkSaveData(data.PlayerCameraData);
        }
      );

    // Calling Provide() triggers the Setup/OnResolved on dependent
    // nodes who depend on the values we provide. This means that
    // all nodes registering save managers will have already registered
    // their relevant save managers by now. This is useful when restoring state
    // while loading an existing save file.
  }

  public void OnExitTree() {
    DeathMenu.TryAgain -= OnStart;
    DeathMenu.MainMenu -= OnMainMenu;
    DeathMenu.TransitionCompleted -= OnDeathMenuTransitioned;
    WinMenu.MainMenu -= OnMainMenu;
    PauseMenu.MainMenu -= OnMainMenu;
    PauseMenu.Resume -= OnResume;
    PauseMenu.TransitionCompleted -= OnPauseMenuTransitioned;
    GameTypingSystem.OnWon -= OnGameWon;

    GameLogic.Stop();
    GameBinding.Dispose();
    GameRepo.Dispose();
  }
  public void OnResolved() {
    SaveFile = new SaveFile<GameData>(
      root: GameChunk,
      onSave: async (GameData data) => {
        // Save the game data to disk.
        var json = JsonSerializer.Serialize(data, JsonOptions);
        await FileSystem.File.WriteAllTextAsync(SaveFilePath, json);
      },
      onLoad: async () => {
        // Load the game data from disk.
        if (!FileSystem.File.Exists(SaveFilePath)) {
          _log.Err("No save file to load");
          return null;
        }

        var json = await FileSystem.File.ReadAllTextAsync(SaveFilePath);
        return JsonSerializer.Deserialize<GameData>(json, JsonOptions);
      }
    );

    GameBinding = GameLogic.Bind();
    GameBinding
      .Handle(
        (in GameLogic.Output.StartGame _) => {
          PlayerCamera.UsePlayerCamera();
          InGameUi.Show();
          GameTyping.StartGame();
        })
      .Handle(
        (in GameLogic.Output.SetPauseMode output) => {
          CallDeferred(nameof(SetPauseMode), output.IsPaused);
          GameTyping.SetPaused(output.IsPaused);
        })
      .Handle(
        (in GameLogic.Output.CaptureMouse output) => {
          // Input.MouseMode = output.IsMouseCaptured
          //   ? Input.MouseModeEnum.Captured
          //   : Input.MouseModeEnum.Visible;
        }
      )
      .Handle((in GameLogic.Output.ShowLostScreen _) => {
        DeathMenu.Show();
        DeathMenu.FadeIn();
        DeathMenu.Animate();
      })
      .Handle((in GameLogic.Output.ExitLostScreen _) => DeathMenu.FadeOut())
      .Handle((in GameLogic.Output.ShowPauseMenu _) => {
        PauseMenu.Show();
        PauseMenu.FadeIn();
      })
      .Handle((in GameLogic.Output.ShowWonScreen _) => {
        WinMenu.Show();
        WinMenu.FadeIn();
      })
      .Handle((in GameLogic.Output.ExitWonScreen _) => WinMenu.FadeOut())
      .Handle((in GameLogic.Output.ExitPauseMenu _) => PauseMenu.FadeOut())
      .Handle((in GameLogic.Output.HidePauseMenu _) => PauseMenu.Hide())
      .Handle((in GameLogic.Output.ShowPauseSaveOverlay _) =>
        PauseMenu.OnSaveStarted()
      )
      .Handle((in GameLogic.Output.HidePauseSaveOverlay _) =>
        PauseMenu.OnSaveCompleted()
      )
      .Handle((in GameLogic.Output.StartSaving _) =>
        SaveFile.Save().ContinueWith(
          // Saving is async. The game node is always around, so kicking off
          // an async process is safe. Plus, we block input while saving, so
          // no interruptions.
          (task) => GameLogic.Input(new GameLogic.Input.SaveCompleted())
        )
      );

    // Trigger the first state's OnEnter callbacks so our bindings run.
    // Keeps everything in sync from the moment we start!
    GameLogic.Start();
    GameLogic.Input(
      new GameLogic.Input.Initialize(NumCoinsInWorld: Map.GetCoinCount())
    );

    this.Provide();
  }

  public override void _Input(InputEvent @event) {
    if (Input.IsActionJustPressed("ui_cancel")) {
      GameLogic.Input(new GameLogic.Input.PauseButtonPressed());
    }
  }

  public void OnGameWon() {
    GameLogic.Input(new GameLogic.Input.EndGame(GameOverReason.Won));
    DatabaseRepo.StoreRun(GameTypingSystem, GameTypingRepo.ActiveScenario!);
  }

  public void OnGameLost() =>
    GameLogic.Input(new GameLogic.Input.EndGame(GameOverReason.Lost));

  public void OnMainMenu() =>
    GameLogic.Input(new GameLogic.Input.GoToMainMenu());

  public void OnResume() =>
    GameLogic.Input(new GameLogic.Input.PauseButtonPressed());

  public void OnStart() =>
    GameLogic.Input(new GameLogic.Input.Start());

  public void OnWinMenuTransitioned() =>
    GameLogic.Input(new GameLogic.Input.WinMenuTransitioned());

  public void OnPauseMenuTransitioned() =>
    GameLogic.Input(new GameLogic.Input.PauseMenuTransitioned());

  public void OnPauseMenuSaveRequested() =>
    GameLogic.Input(new GameLogic.Input.SaveRequested());

  public void OnDeathMenuTransitioned() =>
    GameLogic.Input(new GameLogic.Input.DeathMenuTransitioned());


  public void LoadExistingGame() {
    SaveFile.Load()
      .ContinueWith((_) => CallDeferred(nameof(FinishedLoadingSaveFile)));
  }

  private void FinishedLoadingSaveFile() {
    EmitSignal(SignalName.SaveFileLoaded);
  }

  private void SetPauseMode(bool isPaused) => GetTree().Paused = isPaused;
}
