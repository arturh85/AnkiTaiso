namespace ankitaiso.game.state;

using app.domain;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GameLogic {
  public partial record State {
    [Meta]
    public partial record Quit : State {
      public Quit() {
        this.OnEnter(
          () => Get<IAppRepo>().OnExitGame(PostGameAction.GoToMainMenu)
        );
      }
    }
  }
}
