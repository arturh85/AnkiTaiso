namespace kyoukaitansa.game_typing.state;

using domain;
using GameDemo;

public partial class GameTypingLogic {
  public static class Input {
    public readonly record struct Initialize;

    public readonly record struct OnKeyDown(int key);
  }
}
