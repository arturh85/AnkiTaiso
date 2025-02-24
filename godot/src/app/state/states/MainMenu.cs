namespace kyoukaitansa.app.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using kyoukaitansa.app.domain;

public partial class AppLogic {
  public partial record State {
    [Meta]
    public partial record MainMenu : state.AppLogic.State, LogicBlock<state.AppLogic.State>.IGet<state.AppLogic.Input.NewGame>, LogicBlock<state.AppLogic.State>.IGet<state.AppLogic.Input.LoadGame> {
      public MainMenu() {
        this.OnEnter(
          () => {
            Get<state.AppLogic.Data>().ShouldLoadExistingGame = false;

            Output(new state.AppLogic.Output.SetupGameScene());

            Get<IAppRepo>().OnMainMenuEntered();

            Output(new state.AppLogic.Output.ShowMainMenu());
          }
        );
      }

      public LogicBlock<state.AppLogic.State>.Transition On(in state.AppLogic.Input.NewGame input) => To<LeavingMenu>();

      public LogicBlock<state.AppLogic.State>.Transition On(in state.AppLogic.Input.LoadGame input) {
        Get<state.AppLogic.Data>().ShouldLoadExistingGame = true;

        return To<LeavingMenu>();
      }
    }
  }
}
