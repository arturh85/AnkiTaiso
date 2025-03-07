namespace ankitaiso.player.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class PlayerLogic {
  public partial record State {
    [Meta]
    public abstract partial record Airborne : Alive, LogicBlock<state.PlayerLogic.State>.IGet<state.PlayerLogic.Input.HitFloor>, LogicBlock<state.PlayerLogic.State>.IGet<state.PlayerLogic.Input.StartedFalling> {
      public LogicBlock<state.PlayerLogic.State>.Transition On(in state.PlayerLogic.Input.HitFloor input) =>
        input.IsMovingHorizontally ? To<Moving>() : To<Idle>();

      public LogicBlock<state.PlayerLogic.State>.Transition On(in state.PlayerLogic.Input.StartedFalling input) => To<Falling>();
    }
  }
}
