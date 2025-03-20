namespace ankitaiso.enemy;

using System;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using components.movement;
using enemies;
using game_typing;
using Godot;
using utils;

[Meta(typeof(IAutoNode))]
[SceneTree]
public partial class Enemy : CharacterBody3D, IEnemy {
  public override void _Notification(int what) => this.Notify(what);

  [Export] public bool TestMode = false;
  [Export] public Texture2D? Texture = null;
  // [Export] public Node3D? MovementTarget = null;

  private double _startTime;
  public bool Moving;
  public Vocab? Vocab;
  public bool Dead;
  [Dependency] public GameTypingSystem GameTypingSystem => this.DependOn<GameTypingSystem>();

  [Signal]
  public delegate void OnDeleteEventHandler();

  [OnInstantiate]
  private void Initialise(Vocab vocab, Node3D movementTarget, double startTime) {
    _startTime = startTime;
    Vocab = vocab;
    StartDelay.WaitTime = Mathf.Max(_startTime, 0.01);
    // MovementTarget = movementTarget;
    MoveTo3DComponent.Target = movementTarget;
  }

  public override void _Ready() => Model.Hide();

  private void OnAnimationStarted(StringName animname) => Model.Show();

  public void OnReady() {
    GD.Print("onReady");
    ImpactSprite.Hide();

    StartDelay.Timeout += OnReadyDelayed;
    StartDelay.Start();

    var textureIdx = Random.Shared.Next(0, 5);
    if (textureIdx == 1) {
      LoadTexture("res://src/enemies/zombie1/skins/Japanese_Zombie_Kimono.jpg");
    }
    else if (textureIdx == 2) {
      LoadTexture("res://src/enemies/zombie1/skins/Police_Officer_Zombie1.jpg");
    }
    else if (textureIdx == 3) {
      LoadTexture("res://src/enemies/zombie1/skins/PS1_Zombie_Alternative1.png");
    }
    else if (textureIdx == 4) {
      LoadTexture("res://src/enemies/zombie1/skins/PS1_Zombie_Soldier.png");
    }
  }

  private void LoadTexture(string resourcePath) {
    var material = Zombie1.Mesh.SurfaceGetMaterial(0);
    if (material is not StandardMaterial3D material3D) {
      return;
    }
    material3D.AlbedoTexture = ResourceLoader.Load<Texture2D>($"{resourcePath}");
  }

  private void OnReadyDelayed() {
    AnimationTree.AnimationStarted += OnAnimationStarted;
    AnimationTree.AnimationFinished += OnAnimationFinished;
    AnimationTree.Active = true;

    GameTypingSystem.OnHit += OnVocabHit;
    GameTypingSystem.OnMistake += OnVocabMistake;
  }


  public void OnExitTree() {
    AnimationTree.AnimationStarted -= OnAnimationStarted;
    AnimationTree.AnimationFinished -= OnAnimationFinished;
    GameTypingSystem.OnHit -= OnVocabHit;
    GameTypingSystem.OnMistake -= OnVocabMistake;
  }

  private void OnVocabHit(string key, Vocab? vocab) {
    if (vocab != Vocab) {
      return;
    }

    AnimationTree.Set("parameters/OneShot/request", true);


    if (Vocab is { State: VocabState.Completed }) {
      AnimationTree.Set("parameters/StateMachine/conditions/die", true);
      Dead = true;
      Moving = false;
    }

    var offset = NodeUtils.RandomChild<Node3D>(BulletHitOffsets);
    if (offset == null) {
      GD.Print("failed to find spawn location for impact sprite");
      return;
    }

    var sprite = SpawnImpactSprite(offset.Position);
    sprite.Modulate = Color.Color8(0, 255, 0);
  }

  private void OnVocabMistake(string key, Vocab? vocab) {
    if (vocab != Vocab) {
      return;
    }

    var offset = NodeUtils.RandomChild<Node3D>(BulletMissOffsets);
    if (offset == null) {
      GD.Print("failed to find spawn location for impact sprite");
      return;
    }

    var sprite = SpawnImpactSprite(offset.Position);
    sprite.Modulate = Color.Color8(255, 0, 0);
  }

  private AnimatedSprite3D SpawnImpactSprite(Vector3 position) {
    var sprite = new AnimatedSprite3D();
    sprite.SpriteFrames = ImpactSprite.SpriteFrames;
    sprite.SpeedScale = ImpactSprite.SpeedScale;
    sprite.Animation = ImpactSprite.Animation;
    sprite.Position = position;
    sprite.Play("default");
    sprite.Show();
    sprite.AnimationFinished += () => sprite.QueueFree();
    AddChild(sprite);
    return sprite;
  }

  public Vector3 GetGuiOffset() => GuiOffset.Position;

  private void OnAnimationFinished(StringName animname) {
    if (animname == EnemyAnimations.ClimbGrave) {
      Moving = true;
      if (Vocab != null) {
        Vocab.State = VocabState.Visible;
      }
    }
    else if (animname == EnemyAnimations.Die) {
      EmitSignal(SignalName.OnDelete);
      QueueFree();
    }
  }

  public void UpdateMovementTarget(Node3D newTargetPos) {
    MoveTo3DComponent.Target = newTargetPos;
  }

  public override void _Process(double delta) {

    // var PosXZ = GlobalPosition;
    // PosXZ.Y = 0;
    // var targetXZ = MoveTo3DComponent.Target.Position;
    // targetXZ.Y = GlobalPosition.Y;

    // LookAt(targetXZ, Vector3.Up);

    // targetXZ.Y = 0;

    if (!Moving) {
      return;
    }

    // var distance = PosXZ.DistanceTo(targetXZ);
    // if (distance > 2.1) {
      // var newPos = PosXZ.MoveToward(targetXZ, (float)delta * 1.1f);
      //
      // var spaceState = GetWorld3D().DirectSpaceState;
      //
      // var query = PhysicsRayQueryParameters3D.Create(GlobalPosition + new Vector3(0, 999, 0), GlobalPosition + new Vector3(0, -999, 0));
      // var result = spaceState.IntersectRay(query);
      //
      // if (result.Count > 0) {
      //   newPos.Y = result["position"].AsVector3().Y;
      // }
      // GlobalPosition = newPos;

      //var direction = (Position - _movementTarget).Normalized();
      //GD.Print(direction);
      //Set("constant_linear_velocity", direction * 2.0f);
    // }
    // else {
    //   AnimationTree.Set("parameters/StateMachine/conditions/arrived", true);
    //   AnimationTree.Set("parameters/StateMachine/conditions/bite", true);
    // }
    // var distance = Position.DistanceTo(MovementTarget.Position);
    // if (distance <= 2.1) {
    //   MoveTo3DComponent.SetEnabled(false);
    //   AnimationTree.Set("parameters/StateMachine/conditions/arrived", true);
    //   AnimationTree.Set("parameters/StateMachine/conditions/bite", true);
    // }
  }
}
