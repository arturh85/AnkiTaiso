namespace kyoukaitansa.player.state;

using app.domain;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class PlayerLogic {
  public abstract partial record State {
    [Meta, Id("player_logic_state_disabled")]
    public partial record Disabled : state.PlayerLogic.State, LogicBlock<state.PlayerLogic.State>.IGet<state.PlayerLogic.Input.Enable> {
      public Disabled() {
        this.OnEnter(() => Output(new state.PlayerLogic.Output.Animations.Idle()));

        OnAttach(() => Get<IAppRepo>().GameEntered += OnGameEntered);
        OnDetach(() => Get<IAppRepo>().GameEntered -= OnGameEntered);
      }

      public LogicBlock<state.PlayerLogic.State>.Transition On(in state.PlayerLogic.Input.Enable input) => To<Idle>();
    }

    public void OnGameEntered() => Input(new state.PlayerLogic.Input.Enable());
  }
}
