namespace kyoukaitansa.game_typing.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GameTypingLogic {
  public partial record State {
    [Meta]
    public partial record NothingSelected : State,
      IGet<Input.OnKeyDown> {
      public NothingSelected() {
        this.OnEnter(() => Output(new Output.ShowLostScreen()));
      }

      public Transition On(in Input.OnKeyDown input) => To<EnemySelected>();
      // public Transition On(in Input.GoToMainMenu input) => To<Quit>();
    }
  }
}
