namespace ankitaiso.enemy_panel;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using enemy;
using game_typing;
using Godot;

[SceneTree]
[Meta(typeof(IAutoNode))]
public partial class EnemyPanel : Control {
  public override void _Notification(int what) => this.Notify(what);
  private Enemy? _currentEnemy;

  private Color _activeColor = Color.FromHtml("604540");
  private Color _inactiveColor = Color.FromHtml("000000");


  public string PromptLabelBbcode {
    get => PromptLabel.Get("bbcode").ToString();
    set => PromptLabel.Set("bbcode", value);
  }

  public string InputLabelBbcode {
    get => InputLabel.Get("bbcode").ToString();
    set => InputLabel.Set("bbcode", value);
  }

  public string DebugLabelBbcode {
    get => DebugLabel.Get("bbcode").ToString();
    set => DebugLabel.Set("bbcode", value);
  }

  [OnInstantiate]
  private void Initialise() {
    _currentEnemy = null;
  }

  public void UpdateGui(Enemy enemy) {
    var pos3D = enemy.GlobalPosition + enemy.GetGuiOffset();
    var cam = GetViewport().GetCamera3D();
    var pos2D = cam.UnprojectPosition(pos3D);
    GlobalPosition = pos2D;
    Visible = !cam.IsPositionBehind(pos3D);
    var vocab = enemy.Vocab;
    if (vocab.State == VocabState.Active) {
      ZIndex = -1;
    }
    else {
      ZIndex = -2;
    }
    UpdateVocab(vocab);
    _currentEnemy = enemy;
  }

  public void UpdateVocab(Vocab vocab) {
    if (PromptLabelBbcode != vocab.Entry) {
      PromptLabelBbcode = vocab.Entry;
    }
    if (vocab.State == VocabState.Active) {
      BackgroundContainer.Color = _activeColor;
      var rest = vocab.Entry[(vocab.InputBuffer.Length + vocab.Next.Length)..];
      var targetInput = string.Concat(vocab.InputBuffer, "[red]", vocab.Next, "[]",
        rest.Length > 0 ? "~" + rest + "~" : "");
      if (InputLabelBbcode != targetInput) {
        InputLabelBbcode = targetInput;
      }
    }
    else {
      BackgroundContainer.Color = _inactiveColor;
      if (InputLabelBbcode != "") {
        InputLabelBbcode = "";
      }
    }

    var targetDebugOutput = vocab.NextVariants != null ? string.Join(", ", vocab.NextVariants) : vocab.Next;
    if (DebugLabelBbcode != targetDebugOutput) {
      DebugLabelBbcode = targetDebugOutput;
    }
  }
}
