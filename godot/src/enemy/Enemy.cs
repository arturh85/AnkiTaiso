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
  public bool Moving;
  public Vocab Vocab = null!;

  [Signal]
  public delegate void OnDeleteEventHandler();

  private GameTypingSystem _gameTypingSystem = null!;

  [OnInstantiate]
  private void Initialise(Vocab vocab, GameTypingSystem gameTypingSystem, Vector3 movementTarget) {
    _movementTarget = movementTarget;
    Vocab = vocab;
    _gameTypingSystem = gameTypingSystem;
  }

  public void OnEnterTree() {
    _gameTypingSystem.OnHit += OnVocabHit;
    _gameTypingSystem.OnMistake += OnVocabMistake;
  }

  public void OnExitTree() {
    _gameTypingSystem.OnHit -= OnVocabHit;
    _gameTypingSystem.OnMistake -= OnVocabMistake;
  }

  public override void _Ready() {
    var player = GetAnimationPlayer();
    var anim = player.GetAnimation(EnemyAnimations.Walk);
    anim.LoopMode = Animation.LoopModeEnum.Linear;
    player.Play(EnemyAnimations.ClimbGrave);
    player.AnimationFinished += OnAnimationFinished;
    ImpactSprite.Hide();
  }

  private void OnVocabHit(string key, Vocab? vocab) {
    if (vocab != Vocab) {
      return;
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

  public AnimationPlayer GetAnimationPlayer() => (Model.GetNode(nameof(AnimationPlayer)) as AnimationPlayer)!;

  private void OnAnimationFinished(StringName animname) {
    if (animname == EnemyAnimations.ClimbGrave) {
      Moving = true;
      Vocab.State = VocabState.Visible;
      var player = GetAnimationPlayer();
      player.Play(EnemyAnimations.Walk);
    }
  }

  public override void _Process(double delta) {
    if (Vocab.State == VocabState.Completed) {
      EmitSignal(SignalName.OnDelete);
      QueueFree();
    }

    LookAt(_movementTarget, Vector3.Up);
    if (!Moving) {
      return;
    }

    var distance = Position.DistanceTo(_movementTarget);
    if (distance > 2.1) {
      Position = Position.MoveToward(_movementTarget, (float)delta * 1.1f);
    }
  }
}
