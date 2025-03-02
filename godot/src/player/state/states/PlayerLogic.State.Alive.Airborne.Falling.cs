namespace kyoukaitansa.player.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class PlayerLogic {
  public partial record State {
    [Meta, Id("player_logic_state_alive_airborne_falling")]
    public partial record Falling : Airborne {
      public Falling() {
        this.OnEnter(() => Output(new state.PlayerLogic.Output.Animations.Fall()));
      }
    }
  }
}
