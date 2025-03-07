namespace ankitaiso.player.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using game.domain;
using GameDemo;

public partial class PlayerLogic {
  public abstract partial record State {
    [Meta, Id("player_logic_state_alive_airborne_jumping")]
    public partial record Jumping : Airborne, LogicBlock<state.PlayerLogic.State>.IGet<state.PlayerLogic.Input.Jump> {
      public Jumping() {
        this.OnEnter(
          () => {
            Output(new state.PlayerLogic.Output.Animations.Jump());
            Get<IGameRepo>().OnJump();
          }
        );
      }

      // Override jump when in the air to allow for bigger jumps if the player
      // keeps holding down the jump button.
      public LogicBlock<state.PlayerLogic.State>.Transition On(in state.PlayerLogic.Input.Jump input) {
        var player = Get<IPlayer>();
        var settings = Get<state.PlayerLogic.Settings>();

        var velocity = player.Velocity;

        // Continue the jump in-air. Very forgiving player physics.
        velocity.Y += settings.JumpForce * (float)input.Delta;
        Output(new state.PlayerLogic.Output.VelocityChanged(velocity));

        return ToSelf();
      }
    }
  }
}
