namespace kyoukaitansa.game.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using kyoukaitansa.app.domain;
using kyoukaitansa.game.domain;

public partial class GameLogic {
  public partial record State {
    [Meta]
    public partial record MenuBackdrop : State, LogicBlock<State>.IGet<GameLogic.Input.Start>, LogicBlock<State>.IGet<GameLogic.Input.Initialize> {
      public MenuBackdrop() {
        this.OnEnter(() => Get<IGameRepo>().SetIsMouseCaptured(false));

        OnAttach(() => Get<IAppRepo>().GameEntered += OnGameEntered);
        OnDetach(() => Get<IAppRepo>().GameEntered -= OnGameEntered);
      }

      public void OnGameEntered() => Input(new GameLogic.Input.Start());

      public LogicBlock<State>.Transition On(in GameLogic.Input.Start input) => To<GameLogic.State.Playing>();

      public LogicBlock<State>.Transition On(in GameLogic.Input.Initialize input) {
        // Get<IGameRepo>().SetNumCoinsAtStart(input.NumCoinsInWorld);
        return ToSelf();
      }
    }
  }
}
