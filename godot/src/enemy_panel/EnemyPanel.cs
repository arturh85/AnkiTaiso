namespace ankitaiso.enemy_panel;

using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using enemy;
using game_typing;
using Godot;

// [SceneTree]
public partial class EnemyPanel : Control {
  private Enemy? _currentEnemy;

  private Color _activeColor = Color.FromHtml("604540");
  private Color _inactiveColor = Color.FromHtml("000000");


  [Node] public IRichTextLabel PromptLabel { get; set; } = default!;
  [Node] public IRichTextLabel InputLabel { get; set; } = default!;
  [Node] public IRichTextLabel DebugLabel { get; set; } = default!;
  [Node] public IColorRect BackgroundContainer { get; set; } = default!;

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
  }

  public void UpdateGui(Enemy enemy) {
    var pos3D = enemy.GlobalPosition + enemy.GetGuiOffset();
    var cam = GetViewport().GetCamera3D();
    var pos2D = cam.UnprojectPosition(pos3D);
    GlobalPosition = pos2D;
    Visible = !cam.IsPositionBehind(pos3D);
    if (_currentEnemy != enemy) {
      PromptLabelBbcode = enemy.Vocab.Entry;
    }

    if (enemy.Vocab.State == VocabState.Active) {
      ZIndex = -1;
      BackgroundContainer.Color = _activeColor;
      var rest = enemy.Vocab.Entry[(enemy.Vocab.InputBuffer.Length + enemy.Vocab.Next.Length)..];
      var targetInput = string.Concat(enemy.Vocab.InputBuffer, "[red]", enemy.Vocab.Next, "[]",
        rest.Length > 0 ? "~" + rest + "~" : "");
      if (InputLabelBbcode != targetInput) {
        InputLabelBbcode = targetInput;
      }
    }
    else {
      ZIndex = -2;
      BackgroundContainer.Color = _inactiveColor;
      if (InputLabelBbcode != "") {
        InputLabelBbcode = "";
      }
    }
    var targetDebugOutput = enemy.Vocab.NextVariants != null ? string.Join(", ", enemy.Vocab.NextVariants) : enemy.Vocab.Next;
    if (DebugLabelBbcode != targetDebugOutput) {
      DebugLabelBbcode = targetDebugOutput;
    }
    _currentEnemy = enemy;
  }
}
