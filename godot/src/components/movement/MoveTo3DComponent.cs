using Godot;

namespace ankitaiso.components.movement;

[Icon("res://src/components/velocity_component.svg")]
public partial class MoveTo3DComponent : BaseComponent {
  [Export] public Node3D? Target = null;
  [Export] public double Speed = 100;

  private Node3D _parent = null!;
  public override void _Ready() {
    _parent = GetParent<Node3D>();
  }
  public override void _Process(double delta) {
    if (!Enabled || Target == null) {
      return;
    }
    if (_parent.GlobalPosition != Target.GlobalPosition) {
      _parent.LookAt(Target.GlobalPosition, Vector3.Up);
    }
    _parent.GlobalPosition = _parent.GlobalPosition.MoveToward(Target.GlobalPosition, (float)(delta * Speed));
  }
}
