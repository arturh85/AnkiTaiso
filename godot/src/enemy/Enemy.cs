namespace kyoukaitansa.enemy;

using System.Collections.Generic;
using System.Linq;
using Godot;

[SceneTree]
public partial class Enemy : Node3D {
  private Vector3 _movementTarget;
  public string Prompt;
  public string Input;
  public bool Moving;
  public bool Active;

  [Signal]
  public delegate void OnDeleteEventHandler();

  [OnInstantiate]
  private void Initialise(string prompt, Vector3 movementTarget) {
    _movementTarget = movementTarget;
    Prompt = prompt;
    Input = "";
  }

  public Vector3 GetGuiOffset() => GuiOffset.Position;

  public AnimationPlayer GetAnimationPlayer() => (Model.GetNode(nameof(AnimationPlayer)) as AnimationPlayer)!;

  public override void _Ready() {
    var player = GetAnimationPlayer();
    var anim = player!.GetAnimation(EnemyAnimations.Walk);
    anim.LoopMode = Animation.LoopModeEnum.Linear;
    player.Play(EnemyAnimations.ClimbGrave);
    player.AnimationFinished += OnAnimationFinished;
  }

  private void OnAnimationFinished(StringName animname) {
    if (animname == EnemyAnimations.ClimbGrave) {
      Moving = true;
      var player = GetAnimationPlayer();
      player.Play(EnemyAnimations.Walk);
    }
  }


  public static List<Enemy> GetNearestEnemies(Vector3 playerPosition, Node3D enemies, int numberOfEnemies)
  {
    var enemiesWithDistance = enemies
      .GetChildren()
      .OfType<Enemy>()
      .Where(enemy => enemy.Moving)
      .Select(enemy => new {
        Enemy = enemy,
        // Calculate squared distances to avoid expensive square root operations
        SqrDistance = playerPosition.DistanceSquaredTo(enemy.Position)
      })
      .OrderBy(e => e.SqrDistance)
      .Take(numberOfEnemies)
      .Select(e => e.Enemy)
      .ToList();

    return enemiesWithDistance!;
  }

  public override void _Process(double delta) {
    LookAt(_movementTarget, Vector3.Up);
    if (!Moving) return;
    Position = Position.MoveToward(_movementTarget, (float)delta * 1.1f);
    var distance = Position.DistanceTo(_movementTarget);
    if (distance < 2.1) {
      EmitSignal(SignalName.OnDelete);
      QueueFree();
    }
  }

  public void OnInput(InputEventKey keyEvent) {
    var s = OS.GetKeycodeString(keyEvent.Keycode);

    if (!keyEvent.ShiftPressed) {
      s = s.ToLower();
    }

    if (Prompt.StartsWith(Input + s)) {
      Input += s;
      if (!Active) {
        MakeActive();
      }
    }

    if (Prompt == Input) {
      EmitSignal(SignalName.OnDelete);
      QueueFree();
    }
  }

  private void MakeActive() {
    Active = true;
  }
}
