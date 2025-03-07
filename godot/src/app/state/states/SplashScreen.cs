namespace ankitaiso.app.state;

using ankitaiso.app.domain;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class AppLogic {
  public partial record State {
    [Meta]
    public partial record SplashScreen : state.AppLogic.State, LogicBlock<state.AppLogic.State>.IGet<state.AppLogic.Input.FadeOutFinished> {
      public SplashScreen() {
        this.OnEnter(() => Output(new state.AppLogic.Output.ShowSplashScreen()));

        OnAttach(
          () => Get<IAppRepo>().SplashScreenSkipped += OnSplashScreenSkipped
        );

        OnDetach(
          () => Get<IAppRepo>().SplashScreenSkipped -= OnSplashScreenSkipped
        );
      }

      public LogicBlock<state.AppLogic.State>.Transition On(in state.AppLogic.Input.FadeOutFinished input) => To<MainMenu>();

      public void OnSplashScreenSkipped() =>
        Output(new state.AppLogic.Output.HideSplashScreen());
    }
  }
}
