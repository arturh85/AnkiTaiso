using Godot;
using System;

[Icon("res://src/components/icon.svg")]
public abstract partial class BaseComponent : Node {
  [Export]
  public bool Enabled = true;

  public void SetEnabled(bool enabled) => Enabled = enabled;
}
