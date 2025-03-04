namespace kyoukaitansa.enemy_panel;

using enemy;
using Godot;

[SceneTree]
public partial class EnemyPanel : Control {
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
      PromptLabelBbcode = enemy.Prompt;
    }

    var currentInput = InputLabelBbcode;
    if (enemy.Active) {
      BackgroundContainer.Color = _activeColor;
      var rest = enemy.Prompt.Substring(enemy.Input.Length + 1);
      var targetInput = string.Concat(enemy.Input, "[red]", enemy.Prompt.Substring(enemy.Input.Length, 1), "[]",
        rest.Length > 0 ? "~" + rest + "~" : "");
      if (currentInput != targetInput) {
        InputLabelBbcode = targetInput;
      }
    }
    else {
      BackgroundContainer.Color = _inactiveColor;
      if (currentInput != "") {
        InputLabelBbcode = "";
      }
    }

    _currentEnemy = enemy;
  }
}
