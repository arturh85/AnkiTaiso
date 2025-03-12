namespace ankitaiso.in_game_ui;

using app.domain;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using game_typing;
using game_typing.domain;
using game.domain;
using Godot;

public interface IInGameUI : IControl {
  void UpdateProgressLabel(int leftVocab, int totalVocab);
}

[Meta(typeof(IAutoNode))]
[SceneTree]
public partial class InGameUI : Control, IInGameUI {
  public override void _Notification(int what) => this.Notify(what);

  #region Dependencies

  [Dependency] public IAppRepo AppRepo => DependentExtensions.DependOn<IAppRepo>(this);
  [Dependency] public IGameRepo GameRepo => DependentExtensions.DependOn<IGameRepo>(this);
  [Dependency] public IGameTypingRepo GameTypingRepo => DependentExtensions.DependOn<IGameTypingRepo>(this);

  [Dependency] public GameTypingSystem GameTypingSystem => this.DependOn<GameTypingSystem>();

  #endregion Dependencies

  public void Setup() {
  }

  public void OnResolved() {
    GameTypingSystem.OnLeftCountChanged += UpdateProgressLabel;
    UpdateTimer.Timeout += UpdateStatisticLabel;
  }

  public void OnExitTree() {
    GameTypingSystem.OnLeftCountChanged -= UpdateProgressLabel;
    UpdateTimer.Timeout -= UpdateStatisticLabel;
  }

  public void UpdateStatisticLabel() {
    // if (GameTypingSystem.Start == null) {
    //
    // }

    StatisticLabel.Text = $"";
  }

  public void UpdateProgressLabel(int leftVocab, int totalVocab) =>
    ProgressLabel.Text = $"{totalVocab - leftVocab}/{totalVocab}";
}
