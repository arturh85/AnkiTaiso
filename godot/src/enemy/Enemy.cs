namespace ankitaiso.enemy;

using System.Collections.Generic;
using System.Linq;
using Godot;
using utils;
using WanaKanaNet;

[SceneTree]
public partial class Enemy : Node3D {
  private Vector3 _movementTarget;

  public string Prompt = "";
  // public string RawPrompt;
  public string Input = "";
  public string[] NextInputs = [];
  public bool Moving;
  public bool Active;

  [Signal]
  public delegate void OnDeleteEventHandler();

  [OnInstantiate]
  private void Initialise(string prompt, Vector3 movementTarget) {
    _movementTarget = movementTarget;
    Prompt = prompt;
    // RawPrompt = WanaKana.ToRomaji(prompt).Trim();
    Input = "";
    NextInputs = BuildNextInputs(prompt[..1]);
  }

  private string[] BuildNextInputs(string nextChar) {
    // if (WanaKana.IsHiragana(nextChar)) {
    //
    // }

    return [nextChar];
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
    LookAt(_movementTarget, Vector3.Up);
    if (!Moving)
      return;
    var distance = Position.DistanceTo(_movementTarget);
    if (distance > 2.1) {
      Position = Position.MoveToward(_movementTarget, (float)delta * 1.1f);
    }
  }

  // public bool AcceptsInput(string input) {
  //   return Moving && Prompt.StartsWith(input);
  // }

  public void OnInput(string s) {
    if (Prompt.StartsWith(Input + s)) {
      Input += s;
      NextInputs = BuildNextInputs(Prompt.Substring(Input.Length, 1));
      if (!Active) {
        MakeActive();
      }
    }

    if (Prompt == Input) {
      EmitSignal(SignalName.OnDelete);
      QueueFree();
    }
    else {
      GD.Print(Prompt + " != " + Input);
    }
  }

  private void MakeActive() {
    Active = true;
  }
}
