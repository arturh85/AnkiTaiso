namespace kyoukaitansa.game_typing.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public interface IGameTypingLogic : ILogicBlock<GameTypingLogic.State>;

[Meta]
[LogicBlock(typeof(GameTypingLogic.State), Diagram = true)]
public partial class GameTypingLogic : LogicBlock<GameTypingLogic.State>, IGameTypingLogic {
  public override Transition GetInitialState() => To<GameTypingLogic.State.NothingSelected>();
}
