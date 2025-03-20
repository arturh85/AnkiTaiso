namespace ankitaiso.enemies;

using Chickensoft.GodotNodeInterfaces;
using components.movement;
using Godot;

public interface IEnemy : INode
{
  Vector3 GetGuiOffset();
  Node BulletHitOffsets { get; }
  Node BulletMissOffsets { get; }
  MoveTo3DComponent MoveTo3DComponent { get; }
}
