namespace ankitaiso.game;

using System.Linq;
using ankitaiso.domain;
using app.domain;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.Log;
using Chickensoft.Log.Godot;
using Chickensoft.LogicBlocks;
using domain;
using game_typing;
using game_typing.domain;
using Godot;
using map;
using state;
using utils;

public interface IGame : INode3D, IProvide<IGameRepo> {
}

[Meta(typeof(IAutoNode))]
[SceneTree]
public partial class Game : Node3D, IGame {
  public override void _Notification(int what) => this.Notify(what);
  private readonly Log _log = new (nameof(Game), new GDWriter());

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
    GameRepo = new GameRepo();
    GameLogic = new GameLogic();
    GameLogic.Set(GameRepo);
    GameLogic.Set(AppRepo);
    GameTyping.PlayerPosition = ((CameraWaypoint)((Node3D)Map.GetNode("WayPoints").GetChildren()[0])).CameraPosition;
    GameTyping.Waypoints = Map.GetNode("WayPoints").GetChildren().Cast<CameraWaypoint>().ToList();
    GameTyping.Camera = PlayerCamera;
    //PlayerCamera.GlobalPosition = GameTyping.PlayerPosition;
    GameTyping.SpawnPosition = SpawnLocation.Position;
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

    // Calling Provide() triggers the Setup/OnResolved on dependent
    // nodes who depend on the values we provide. This means that
    // all nodes registering save managers will have already registered
    // their relevant save managers by now. This is useful when restoring state
    // while loading an existing save file.

    GetViewport().Msaa3D = Viewport.Msaa.Msaa8X;
    GetViewport().ScreenSpaceAA = Viewport.ScreenSpaceAAEnum.Fxaa;
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
        GameLogic.Input(new GameLogic.Input.SaveCompleted())
      );

    // Trigger the first state's OnEnter callbacks so our bindings run.
    // Keeps everything in sync from the moment we start!
    GameLogic.Start();
    GameLogic.Input(
      new GameLogic.Input.Initialize(NumCoinsInWorld: 0)
    );

    this.Provide();
  }

  public override void _Input(InputEvent @event) {
    if (Input.IsActionJustPressed(BuiltinInputActions.UICancel)) {
      GameLogic.Input(new GameLogic.Input.PauseButtonPressed());
    }
  }

  public async void OnGameWon() {
    await DatabaseRepo.StoreRun(GameTypingSystem, GameTypingRepo.ActiveScenario!);
    GameLogic.Input(new GameLogic.Input.EndGame(GameOverReason.Won));
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

  private void SetPauseMode(bool isPaused) => GetTree().Paused = isPaused;
}
