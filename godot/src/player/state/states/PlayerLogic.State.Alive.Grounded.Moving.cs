namespace ankitaiso.player.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class PlayerLogic {
  public abstract partial record State {
    [Meta, Id("player_logic_state_alive_grounded_moving")]
    public partial record Moving : Grounded, LogicBlock<state.PlayerLogic.State>.IGet<state.PlayerLogic.Input.StoppedMovingHorizontally> {
      public Moving() {
        this.OnEnter(() => Output(new state.PlayerLogic.Output.Animations.Move()));
      }

      public LogicBlock<state.PlayerLogic.State>.Transition On(in state.PlayerLogic.Input.StoppedMovingHorizontally input) =>
        To<Idle>();
    }
  }
}
