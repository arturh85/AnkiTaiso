namespace ankitaiso.game_typing.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class GameTypingLogic {
  public partial record State {
    [Meta]
    public partial record EnemySelected : State,
      IGet<Input.Initialize> {
      public EnemySelected() {
        this.OnEnter(() => Output(new Output.ShowLostScreen()));
      }

      public Transition On(in Input.Initialize input) => To<EnemySelected>();
      // public Transition On(in Input.GoToMainMenu input) => To<Quit>();
    }
  }
}
