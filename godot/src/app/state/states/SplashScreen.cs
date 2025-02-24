namespace kyoukaitansa.app.state.states;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using kyoukaitansa.app.domain;

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
