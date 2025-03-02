namespace kyoukaitansa.game.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using domain;
using GameDemo;

public partial class GameLogic {
  public partial record State {
    [Meta]
    public partial record Resuming : State, IGet<Input.PauseMenuTransitioned> {
      public Resuming() {
        this.OnEnter(() => Get<IGameRepo>().Resume());
        this.OnExit(() => Output(new Output.HidePauseMenu()));
      }

      public Transition On(in Input.PauseMenuTransitioned input) =>
        To<Playing>();
    }
  }
}
