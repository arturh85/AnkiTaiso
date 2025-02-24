namespace kyoukaitansa.game.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GameLogic {
  public partial record State {
    [Meta]
    public partial record Saving : Paused, LogicBlock<State>.IGet<state.GameLogic.Input.SaveCompleted> {
      public Saving() {
        this.OnEnter(
          () => {
            Output(new state.GameLogic.Output.ShowPauseSaveOverlay());
            Output(new state.GameLogic.Output.StartSaving());
          }
        );

        this.OnExit(() => Output(new state.GameLogic.Output.HidePauseSaveOverlay()));
      }

      public LogicBlock<State>.Transition On(in state.GameLogic.Input.SaveCompleted input) => To<Paused>();

      // Make it impossible to leave the pause menu while saving
      public override LogicBlock<State>.Transition On(in state.GameLogic.Input.PauseButtonPressed input) =>
        ToSelf();
    }
  }
}
