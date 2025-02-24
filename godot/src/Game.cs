namespace kyoukaitansa;

using Godot;

public partial class Game : Control {
  public int ButtonPresses { get; private set; }

  public override void _Ready() {
  }

  public void OnTestButtonPressed() => ButtonPresses++;
}
