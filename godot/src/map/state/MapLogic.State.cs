namespace ankitaiso.map.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

public partial class MapLogic {
  [Meta]
  public partial record State : StateLogic<State>,
  IGet<Input.GameLoadedFromSaveFile> {
    public State() {
      OnAttach(() => {
      });

      OnDetach(() => {
      });
    }

    public Transition On(in Input.GameLoadedFromSaveFile input) => ToSelf();
  }
}
