namespace kyoukaitansa.game_typing.state;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using domain;

public partial class GameTypingLogic {
  [Meta]
  public partial record State : StateLogic<State> {
    public State() {
      OnAttach(() => {
        var gameRepo = Get<IGameTypingRepo>();
        // gameRepo.IsMouseCaptured.Sync += OnIsMouseCaptured;
        // gameRepo.IsPaused.Sync += OnIsPaused;
      });

      OnDetach(() => {
        var gameRepo = Get<IGameTypingRepo>();
        // gameRepo.IsMouseCaptured.Sync -= OnIsMouseCaptured;
        // gameRepo.IsPaused.Sync -= OnIsPaused;
      });
    }

    public void OnIsMouseCaptured(bool isMouseCaptured) =>
      Output(new Output.CaptureMouse(isMouseCaptured));

    public void OnIsPaused(bool isPaused) =>
      Output(new Output.SetPauseMode(isPaused));
  }
}
