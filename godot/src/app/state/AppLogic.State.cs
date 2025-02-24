namespace kyoukaitansa.app.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class AppLogic {
  [Meta]
  public abstract partial record State : StateLogic<State>;
}
