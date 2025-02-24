namespace kyoukaitansa.game.state;

using app.domain;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GameLogic {
  public partial record State {
    [Meta]
    public partial record Won : State, LogicBlock<State>.IGet<state.GameLogic.Input.GoToMainMenu> {
      public Won() {
        this.OnEnter(() => Output(new state.GameLogic.Output.ShowWonScreen()));
      }

      public LogicBlock<State>.Transition On(in state.GameLogic.Input.GoToMainMenu input) {
        Get<IAppRepo>().OnExitGame(PostGameAction.GoToMainMenu);
        return ToSelf();
      }
    }
  }
}
