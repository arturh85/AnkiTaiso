namespace ankitaiso.in_game_ui;

using app.domain;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using game_typing;
using game.domain;
using game_typing.domain;
using GameDemo;
using Godot;
using state;
using InGameUILogic = state.InGameUILogic;

public interface IInGameUI : IControl {
  void UpdateLabel(int leftVocab, int totalVocab);
}

[Meta(typeof(IAutoNode))]
public partial class InGameUI : Control, IInGameUI {
  public override void _Notification(int what) => this.Notify(what);

  #region Dependencies

  [Dependency] public IAppRepo AppRepo => DependentExtensions.DependOn<IAppRepo>(this);
  [Dependency] public IGameRepo GameRepo => DependentExtensions.DependOn<IGameRepo>(this);
  [Dependency] public IGameTypingRepo GameTypingRepo => DependentExtensions.DependOn<IGameTypingRepo>(this);

  [Dependency] public GameTypingSystem GameTypingSystem => this.DependOn<GameTypingSystem>();

  #endregion Dependencies

  #region Nodes

  [Node] public ILabel CoinsLabel { get; set; } = default!;

  #endregion Nodes

  #region State

  public IInGameUILogic InGameUILogic { get; set; } = default!;

  public InGameUILogic.IBinding InGameUIBinding { get; set; } = default!;

  #endregion State

  public void Setup() {
    InGameUILogic = new InGameUILogic();
  }

  public void OnResolved() {
    InGameUILogic.Set(this);
    InGameUILogic.Set(AppRepo);
    InGameUILogic.Set(GameRepo);
    InGameUILogic.Set(GameTypingRepo);

    InGameUIBinding = InGameUILogic.Bind();

    // InGameUIBinding
    //   .Handle((in InGameUILogic.Output.NumCoinsChanged output) =>
    //     SetCoinsLabel(
    //       output.NumCoinsCollected, output.NumCoinsAtStart
    //     )
    //   );

    GameTypingSystem.OnLeftCountChanged += UpdateLabel;

    InGameUILogic.Start();
  }

  public void UpdateLabel(int leftVocab, int totalVocab) =>
    CoinsLabel.Text = $"{totalVocab - leftVocab}/{totalVocab}";

  public void OnExitTree() {
    InGameUILogic.Stop();
    InGameUIBinding.Dispose();
    GameTypingSystem.OnLeftCountChanged -= UpdateLabel;
  }
}
