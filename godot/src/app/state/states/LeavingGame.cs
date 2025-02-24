namespace kyoukaitansa.app.state.states;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using kyoukaitansa.app.domain;

public partial class AppLogic {
  public partial record State {
    [Meta]
    public partial record LeavingGame : state.AppLogic.State, LogicBlock<state.AppLogic.State>.IGet<state.AppLogic.Input.FadeOutFinished> {
      public PostGameAction PostGameAction { get; set; } = PostGameAction.RestartGame;

      public LogicBlock<state.AppLogic.State>.Transition On(in state.AppLogic.Input.FadeOutFinished input) {
        // We are either supposed to restart the game or go back to the main
        // menu. More complex games might have more post-game destinations,
        // but it's pretty simple for us.
        Output(new state.AppLogic.Output.RemoveExistingGame());

        if (PostGameAction is not PostGameAction.RestartGame) {
          return To<MainMenu>();
        }

        Output(new state.AppLogic.Output.SetupGameScene());
        return To<InGame>();
      }
    }
  }
}
