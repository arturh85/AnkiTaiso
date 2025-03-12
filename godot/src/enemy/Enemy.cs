namespace ankitaiso.enemy;

using System.Collections.Generic;
using System.Linq;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using game_typing;
using Godot;

[Meta(typeof(IAutoNode))]
public partial class Enemy : Node3D {
  public override void _Notification(int what) => this.Notify(what);

  private Vector3 _movementTarget;

  // public string Prompt = "";
  // // public string RawPrompt;
  // public string Input = "";
  // public string[] NextInputs = [];
  public bool Moving;
  public Vocab Vocab = null!;

  [Signal]
  public delegate void OnDeleteEventHandler();
  [Node] public INode3D GuiOffset { get; set; } = default!;
  [Node] public INode3D Model { get; set; } = default!;

  [OnInstantiate]
  private void Initialise(Vocab vocab, Vector3 movementTarget) {
    _movementTarget = movementTarget;
    Vocab = vocab;
    Vocab.OnMistake += OnVocabMistake;
  }


  public void OnExitTree() {
    Vocab.OnMistake -= OnVocabMistake;
  }

  public void OnVocabMistake(string key) {

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
      Vocab.State = VocabState.Visible;
      var player = GetAnimationPlayer();
      player.Play(EnemyAnimations.Walk);
    }
  }


  public static List<Enemy> GetNearestEnemies(Vector3 playerPosition, Node3D enemies, int numberOfEnemies) {
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
    if (Vocab.State == VocabState.Completed) {
      EmitSignal(SignalName.OnDelete);
      QueueFree();
    }
    LookAt(_movementTarget, Vector3.Up);
    if (!Moving)
      return;
    var distance = Position.DistanceTo(_movementTarget);
    if (distance > 2.1) {
      Position = Position.MoveToward(_movementTarget, (float)delta * 1.1f);
    }
  }
}
