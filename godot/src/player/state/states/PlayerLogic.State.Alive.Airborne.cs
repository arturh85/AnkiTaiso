namespace ankitaiso.player.state;

using Chickensoft.Introspection;

public partial class PlayerLogic {
  public partial record State {
    [Meta]
    public abstract partial record Airborne : Alive, IGet<Input.HitFloor>, IGet<Input.StartedFalling> {
      public Transition On(in Input.HitFloor input) =>
        input.IsMovingHorizontally ? To<Moving>() : To<Idle>();

      public Transition On(in Input.StartedFalling input) => To<Falling>();
    }
  }
}
