namespace kyoukaitansa.game.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using kyoukaitansa.game.domain;

public partial class GameLogic {
  public partial record State {
    [Meta]
    public partial record Playing : State, LogicBlock<State>.IGet<state.GameLogic.Input.EndGame>, LogicBlock<State>.IGet<state.GameLogic.Input.PauseButtonPressed> {
      public Playing() {
        this.OnEnter(
          () => {
            Output(new state.GameLogic.Output.StartGame());
            Get<IGameRepo>().SetIsMouseCaptured(true);
          }
        );

        OnAttach(() => Get<IGameRepo>().Ended += OnEnded);
        OnDetach(() => Get<IGameRepo>().Ended -= OnEnded);
      }

      public void OnEnded(GameOverReason reason)
        => Input(new state.GameLogic.Input.EndGame(reason));

      public LogicBlock<State>.Transition On(in state.GameLogic.Input.EndGame input) {
        Get<IGameRepo>().Pause();

        return input.Reason switch {
          GameOverReason.Won => To<Won>(),
          GameOverReason.Lost => To<Lost>(),
          _ => To<Quit>()
        };
      }

      public LogicBlock<State>.Transition On(in state.GameLogic.Input.PauseButtonPressed input) => To<Paused>();
    }
  }
}
