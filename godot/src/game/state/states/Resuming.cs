namespace kyoukaitansa.game.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using kyoukaitansa.game.domain;

public partial class GameLogic {
  public partial record State {
    [Meta]
    public partial record Resuming : State, LogicBlock<State>.IGet<state.GameLogic.Input.PauseMenuTransitioned> {
      public Resuming() {
        this.OnEnter(() => Get<IGameRepo>().Resume());
        this.OnExit(() => Output(new state.GameLogic.Output.HidePauseMenu()));
      }

      public LogicBlock<State>.Transition On(in state.GameLogic.Input.PauseMenuTransitioned input) =>
        To<Playing>();
    }
  }
}
