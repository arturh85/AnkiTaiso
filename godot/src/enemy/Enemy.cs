namespace ankitaiso.enemy;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using game_typing;
using Godot;
using utils;

[Meta(typeof(IAutoNode))]
[SceneTree]
public partial class Enemy : Node3D {
  public override void _Notification(int what) => this.Notify(what);

  private Vector3 _movementTarget;
  private double _startTime;
  public bool Moving;
  public Vocab Vocab = null!;
  public bool dead;
  [Dependency] public GameTypingSystem GameTypingSystem => this.DependOn<GameTypingSystem>();

  [Signal]
  public delegate void OnDeleteEventHandler();

  [OnInstantiate]
  private void Initialise(Vocab vocab, Vector3 movementTarget, double startTime) {
    _movementTarget = movementTarget;
    _startTime = startTime;
    Vocab = vocab;
    StartDelay.WaitTime = _startTime;
  }

  public override void _Ready() => Model.Hide();

  private void OnAnimationStarted(StringName animname) => Model.Show();

  public void OnReady() {

    ImpactSprite.Hide();

    StartDelay.Timeout += StartDelayTimeout;
    StartDelay.Start();
  }

  void StartDelayTimeout() {

    var player = GetAnimationTree();
    player.AnimationStarted += OnAnimationStarted;
    player.AnimationFinished += OnAnimationFinished;
    player.Active = true;


    GameTypingSystem.OnHit += OnVocabHit;
    GameTypingSystem.OnMistake += OnVocabMistake;

  }


  public void OnExitTree() {
    GameTypingSystem.OnHit -= OnVocabHit;
    GameTypingSystem.OnMistake -= OnVocabMistake;
    var player = GetAnimationTree();
    player.AnimationStarted -= OnAnimationStarted;
    player.AnimationFinished -= OnAnimationFinished;
  }

  private void OnVocabHit(string key, Vocab? vocab) {
    if (vocab != Vocab) {
      return;
    }
    GetAnimationTree().Set("parameters/OneShot/request", true);


    if (Vocab.State == VocabState.Completed) {
      GetAnimationTree().Set("parameters/StateMachine/conditions/die", true);
      dead = true;
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

  public AnimationTree GetAnimationTree() => (GetNode(nameof(AnimationTree)) as AnimationTree)!;

  private void OnAnimationFinished(StringName animname) {
    if (animname == EnemyAnimations.ClimbGrave) {
      Moving = true;
      Vocab.State = VocabState.Visible;
    }
    else if (animname == EnemyAnimations.Die) {
      EmitSignal(SignalName.OnDelete);
      QueueFree();
    }
  }

  public override void _Process(double delta) {

    var PosXZ = GlobalPosition;
    PosXZ.Y = 0;
    var targetXZ = _movementTarget;
    targetXZ.Y = GlobalPosition.Y;

    LookAt(targetXZ, Vector3.Up);

    targetXZ.Y = 0;

    if (!Moving) {
      return;
    }

    var distance = PosXZ.DistanceTo(targetXZ);
    if (distance > 2.1) {
      var newPos = PosXZ.MoveToward(targetXZ, (float)delta * 1.1f);

      var spaceState = GetWorld3D().DirectSpaceState;

      var query = PhysicsRayQueryParameters3D.Create(GlobalPosition + new Vector3(0, 999, 0), GlobalPosition + new Vector3(0, -999, 0));
      var result = spaceState.IntersectRay(query);

      if (result.Count > 0) {
        newPos.Y = result["position"].AsVector3().Y;
      }
      GlobalPosition = newPos;

      //var direction = (Position - _movementTarget).Normalized();
      //GD.Print(direction);
      //Set("constant_linear_velocity", direction * 2.0f);
    }
    else {
      GetAnimationTree().Set("parameters/StateMachine/conditions/arrived", true);
      GetAnimationTree().Set("parameters/StateMachine/conditions/bite", true);
    }
  }
}
