namespace kyoukaitansa.game.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public interface IGameLogic : ILogicBlock<GameLogic.State>;

[Meta]
[LogicBlock(typeof(GameLogic.State), Diagram = true)]
public partial class GameLogic : LogicBlock<GameLogic.State>, IGameLogic {
  public override Transition GetInitialState() => To<GameLogic.State.MenuBackdrop>();
}
