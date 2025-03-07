namespace ankitaiso.player_camera.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public interface IPlayerCameraLogic : ILogicBlock<PlayerCameraLogic.State>;

[Meta, Id("player_camera_logic")]
[LogicBlock(typeof(State), Diagram = true)]
public partial class PlayerCameraLogic :
  LogicBlock<PlayerCameraLogic.State>, IPlayerCameraLogic {
  public override Transition GetInitialState() => To<State.InputDisabled>();
}
