namespace kyoukaitansa.player.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class PlayerLogic {
  public abstract partial record State {
    [Meta]
    public abstract partial record Grounded : Alive, LogicBlock<state.PlayerLogic.State>.IGet<state.PlayerLogic.Input.Jump>, LogicBlock<state.PlayerLogic.State>.IGet<state.PlayerLogic.Input.LeftFloor> {
      public virtual LogicBlock<state.PlayerLogic.State>.Transition On(in state.PlayerLogic.Input.Jump input) {
        // We can jump from any grounded state if the jump button was just
        // pressed.
        var player = Get<IPlayer>();
        var settings = Get<state.PlayerLogic.Settings>();

        var velocity = player.Velocity;

        // Start the jump.
        velocity.Y += settings.JumpImpulseForce;
        Output(new state.PlayerLogic.Output.VelocityChanged(velocity));

        return To<Jumping>();
      }

      public LogicBlock<state.PlayerLogic.State>.Transition On(in state.PlayerLogic.Input.LeftFloor input) {
        if (input.IsFalling) {
          return To<Falling>();
        }
        // We got pushed into the air by something that isn't the player's jump
        // input, so we have a separate state for that.
        return To<Liftoff>();
      }
    }
  }
}
