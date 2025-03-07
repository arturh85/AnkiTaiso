namespace ankitaiso.player.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class PlayerLogic {
  [Meta]
  public abstract partial record State : StateLogic<State>;
}
