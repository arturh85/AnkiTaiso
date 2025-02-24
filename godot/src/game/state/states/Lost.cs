namespace kyoukaitansa.game.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GameLogic {
  public partial record State {
    [Meta]
    public partial record Lost : State, LogicBlock<State>.IGet<state.GameLogic.Input.Start>, LogicBlock<State>.IGet<state.GameLogic.Input.GoToMainMenu> {
      public Lost() {
        this.OnEnter(() => Output(new state.GameLogic.Output.ShowLostScreen()));
      }

      public LogicBlock<State>.Transition On(in state.GameLogic.Input.Start input) => To<RestartingGame>();
      public LogicBlock<State>.Transition On(in state.GameLogic.Input.GoToMainMenu input) => To<Quit>();
    }
  }
}
