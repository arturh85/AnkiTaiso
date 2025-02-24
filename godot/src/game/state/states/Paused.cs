namespace kyoukaitansa.game.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using kyoukaitansa.game.domain;

public partial class GameLogic {
  public partial record State {
    [Meta]
    public partial record Paused : State, LogicBlock<State>.IGet<state.GameLogic.Input.PauseButtonPressed>, LogicBlock<State>.IGet<state.GameLogic.Input.GoToMainMenu>, LogicBlock<State>.IGet<state.GameLogic.Input.SaveRequested> {
      public Paused() {
        this.OnEnter(
          () => {
            Get<IGameRepo>().Pause();
            Output(new state.GameLogic.Output.ShowPauseMenu());
          }
        );

        // We don't resume on exit because we can leave this state for
        // a menu and we want to remain paused.
        this.OnExit(() => Output(new state.GameLogic.Output.ExitPauseMenu()));
      }

      public virtual LogicBlock<State>.Transition On(in state.GameLogic.Input.PauseButtonPressed input)
        => To<Resuming>();

      public LogicBlock<State>.Transition On(in state.GameLogic.Input.SaveRequested input) => To<Saving>();

      public LogicBlock<State>.Transition On(in state.GameLogic.Input.GoToMainMenu input) => To<Quit>();
    }
  }
}
