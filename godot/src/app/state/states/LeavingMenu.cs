namespace kyoukaitansa.app.state.states;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class AppLogic {
  public partial record State {
    [Meta]
    public partial record LeavingMenu : state.AppLogic.State, LogicBlock<state.AppLogic.State>.IGet<state.AppLogic.Input.FadeOutFinished> {
      public LeavingMenu() {
        this.OnEnter(() => Output(new state.AppLogic.Output.FadeToBlack()));
      }

      public LogicBlock<state.AppLogic.State>.Transition On(in state.AppLogic.Input.FadeOutFinished input) =>
        Get<state.AppLogic.Data>().ShouldLoadExistingGame
          ? To<LoadingSaveFile>()
          : To<InGame>();
    }
  }
}
