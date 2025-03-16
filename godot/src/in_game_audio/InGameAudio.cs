namespace ankitaiso.in_game_audio;

using app.domain;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using game_typing;
using game_typing.domain;
using game.domain;
using Godot;
using state;
using InGameAudioLogic = state.InGameAudioLogic;

[Meta(typeof(IAutoNode))]
[SceneTree]
public partial class InGameAudio : Node {
  public override void _Notification(int what) => this.Notify(what);

  #region Dependencies

  [Dependency] public IAppRepo AppRepo => DependentExtensions.DependOn<IAppRepo>(this);

  [Dependency] public IGameRepo GameRepo => DependentExtensions.DependOn<IGameRepo>(this);
  [Dependency] public IGameTypingRepo GameTypingRepo => DependentExtensions.DependOn<IGameTypingRepo>(this);
  [Dependency] public GameTypingSystem GameTypingSystem => this.DependOn<GameTypingSystem>();

  #endregion Dependencies

  #region State

  public IInGameAudioLogic InGameAudioLogic { get; set; } = default!;

  public LogicBlock<InGameAudioLogic.State>.IBinding InGameAudioBinding { get; set; } = default!;

  #endregion State

  public void Setup() => InGameAudioLogic = new InGameAudioLogic();

  public void OnResolved() {
    InGameAudioLogic.Set(AppRepo);
    InGameAudioLogic.Set(GameRepo);

    InGameAudioBinding = InGameAudioLogic.Bind();

    InGameAudioBinding
      .Handle((in InGameAudioLogic.Output.PlayCoinCollected _) =>
        CoinCollected.Play()
      )
      .Handle((in InGameAudioLogic.Output.PlayBounce _) => Bounce.Play()
      )
      .Handle((in InGameAudioLogic.Output.PlayPlayerDied _) => PlayerDied.Play()
      )
      .Handle((in InGameAudioLogic.Output.PlayJump _) =>
        PlayerJumped.Play()
      )
      .Handle((in InGameAudioLogic.Output.PlayMainMenuMusic _) => {
          // StartMainMenuMusic();
        }
      )
      .Handle((in InGameAudioLogic.Output.PlayGameMusic _) => StartGameMusic())
      .Handle((in InGameAudioLogic.Output.StopGameMusic _) =>
        GameMusic.FadeOut()
      );

    GameTypingSystem.OnHit += OnHit;

    InGameAudioLogic.Start();
  }

  private void OnHit(string key, Vocab? vocab) {
    if (vocab is { State: VocabState.Completed, Entry.AudioFilename: not null } && GameTypingRepo.ActiveScenario is
        {
          Config: not null
        }) {
      var audioPath = $"{GameTypingRepo.ActiveScenario.Config.AudioDirectoryPath}/{vocab.Entry.AudioFilename}";
      ClearedWordPlayer.Stream = AudioStreamMP3.LoadFromFile(audioPath);
      ClearedWordPlayer.Play();
    }
  }

  public void OnExitTree() {
    InGameAudioLogic.Stop();
    InGameAudioBinding.Dispose();
  }

  public void StartMainMenuMusic() {
    GameMusic.FadeOut();
    MainMenuMusic.Stop();
    MainMenuMusic.FadeIn();
  }

  public void StartGameMusic() {
    MainMenuMusic.FadeOut();
    GameMusic.Stop();
    GameMusic.FadeIn();
  }
}
