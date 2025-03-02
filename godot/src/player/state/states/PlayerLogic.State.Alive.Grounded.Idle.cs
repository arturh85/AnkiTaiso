namespace kyoukaitansa.player.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class PlayerLogic {
  public abstract partial record State {
    [Meta, Id("player_logic_state_alive_grounded_idle")]
    public partial record Idle : Grounded, LogicBlock<state.PlayerLogic.State>.IGet<state.PlayerLogic.Input.StartedMovingHorizontally> {
      public Idle() {
        this.OnEnter(() => Output(new state.PlayerLogic.Output.Animations.Idle()));
      }

      public LogicBlock<state.PlayerLogic.State>.Transition On(in state.PlayerLogic.Input.StartedMovingHorizontally input) =>
        To<Moving>();
    }
  }
}
