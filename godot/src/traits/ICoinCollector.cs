namespace GameDemo;

using Chickensoft.GodotNodeInterfaces;
using Godot;

public interface ICoinCollector : INode {
  public Vector3 CenterOfMass { get; }
}
