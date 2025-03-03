namespace kyoukaitansa.enemy;

using System.Text;
using Godot;

[SceneTree]
public partial class Enemy : Node3D {
  public Vector3 MovementTarget;

  private bool mooving = false;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
    _.Gui.VBoxContainer.Input.Text = "a";
    var player = _.scene.GetNode("AnimationPlayer") as AnimationPlayer;
    var anim = player!.GetAnimation("WalkZ");
    anim.LoopMode = Animation.LoopModeEnum.Linear;
    player.Play("ClimbGraveZ");

    player.AnimationFinished += OnAnimationFinished;
    var container = _.Gui.Get();
    container.Visible = false;
  }

  private void OnAnimationFinished(StringName animname) {
    if (animname == "ClimbGraveZ") {
      mooving = true;
      var player = _.scene.GetNode("AnimationPlayer") as AnimationPlayer;
      player.Play("WalkZ");
    }
  }

  public void EnableA() {
    Show();
    ProcessMode = ProcessModeEnum.Inherit;
    // SetProcess(true);
  }
  public void DisableA() {
    Hide();
    ProcessMode = ProcessModeEnum.Disabled;
    // SetProcess(false);
  }

  public string GetText() {
    return _.Gui.VBoxContainer.Prompt.Text;
  }
  public void SetText(string text) {
    _.Gui.VBoxContainer.Prompt.Text = text;
  }
  public void SetTarget(Vector3 target) {
    MovementTarget = target;
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) {
    var pos_3d = GlobalPosition + _.GuiPosition.Position;
    var cam = GetViewport().GetCamera3D();
    var pos_2d = cam.UnprojectPosition(pos_3d);
    var container = _.Gui.Get();
    container.GlobalPosition = pos_2d;
    container.Visible = mooving && !cam.IsPositionBehind(pos_3d);
    LookAt(MovementTarget, Vector3.Up);

    if (!mooving) return;
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
