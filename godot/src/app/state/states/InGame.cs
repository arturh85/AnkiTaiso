namespace ankitaiso.app.state;

using ankitaiso.app.domain;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class AppLogic {
  public partial record State {
    [Meta]
    public partial record InGame : state.AppLogic.State, LogicBlock<state.AppLogic.State>.IGet<state.AppLogic.Input.EndGame> {
      public InGame() {
        this.OnEnter(() => {
          Get<IAppRepo>().OnEnterGame();
          Output(new state.AppLogic.Output.ShowGame());
        });
        this.OnExit(() => Output(new state.AppLogic.Output.HideGame()));

        OnAttach(() => Get<IAppRepo>().GameExited += OnGameExited);
        OnDetach(() => Get<IAppRepo>().GameExited -= OnGameExited);
      }

      public void OnRestartGameRequested() =>
        Input(new state.AppLogic.Input.EndGame(PostGameAction.RestartGame));

      public void OnGameExited(PostGameAction reason) =>
        Input(new state.AppLogic.Input.EndGame(reason));

      public LogicBlock<state.AppLogic.State>.Transition On(in state.AppLogic.Input.EndGame input) {
        var postGameAction = input.PostGameAction;
        return To<LeavingGame>().With(
          (state) => ((LeavingGame)state).PostGameAction = postGameAction
        );
      }
    }
  }
}
