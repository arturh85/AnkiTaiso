namespace kyoukaitansa.app.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class AppLogic {
  public partial record State {
    [Meta]
    public partial record LoadingSaveFile : state.AppLogic.State, LogicBlock<state.AppLogic.State>.IGet<state.AppLogic.Input.SaveFileLoaded> {
      public LoadingSaveFile() {
        this.OnEnter(() => Output(new state.AppLogic.Output.StartLoadingSaveFile()));
      }
      public LogicBlock<state.AppLogic.State>.Transition On(in state.AppLogic.Input.SaveFileLoaded input) => To<InGame>();
    }
  }
}
