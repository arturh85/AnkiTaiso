namespace kyoukaitansa.enemy;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;

[SceneTree]
public partial class Enemy : Node3D {
  public Vector3 MovementTarget;
  public string Prompt;
  public string Input;
  public bool Moving;

  [OnInstantiate]
  private void Initialise(string prompt, Vector3 movementTarget) {
    MovementTarget = movementTarget;
    Prompt = prompt;
    Input = "";
  }

  public Vector3 GetGuiOffset() {
    return _.GuiPosition.Position;
  }

  public override void _Ready() {
    var player = _.scene.GetNode("AnimationPlayer") as AnimationPlayer;
    var anim = player!.GetAnimation("WalkZ");
    anim.LoopMode = Animation.LoopModeEnum.Linear;
    player.Play("ClimbGraveZ");
    player.AnimationFinished += OnAnimationFinished;
  }

  private void OnAnimationFinished(StringName animname) {
    if (animname == "ClimbGraveZ") {
      Moving = true;
      var player = _.scene.GetNode("AnimationPlayer") as AnimationPlayer;
      player.Play("WalkZ");
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
    LookAt(MovementTarget, Vector3.Up);

    if (!Moving) return;
    //
    // if (MovementTarget == null) {
    //   GD.Print("NO movement target");
    //   return;
    // }
    Position = Position.MoveToward(MovementTarget, (float)delta * 1.1f);
    var distance = Position.DistanceTo(MovementTarget);
    if (distance < 2.1) {
      // GD.Print(_.Label.Text  + " freed");
      QueueFree();
    }
  }
}
