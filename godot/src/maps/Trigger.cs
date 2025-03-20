using ankitaiso.game_typing;
using ankitaiso.map;
using Godot;
using JetBrains.Annotations;
using System;

[Tool]
[SceneTree]
public partial class Trigger : Area3D {
  [Export] public bool Stop = false;
  [Export] public float WaitTime = 10.0f;
  public bool Triggered = false;
  
  public override void _EnterTree() {

  }

  public override void _PhysicsProcess(double delta) {

    var areas = GetOverlappingAreas();
    if (areas.Count > 0) {
      if (!Triggered) {
        Triggered = true;
        RunTrigger();

      }
        
    }

  }
  public void RunTrigger() {
    GameTyping gameTyping = (GameTyping)GetParent().GetParent().GetParent().GetParent().GetNode("GameTyping");

    foreach (var spawner in GetChildren()) {

      if (spawner is ZombieSpawner) {
        gameTyping.SpawnEnemy((ZombieSpawner)spawner);
      }
    }

    if (Stop) {
      gameTyping.StopCamera(WaitTime);
    }
  }
}
