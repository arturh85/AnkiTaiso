namespace ankitaiso.enemy_panel;

using enemy;
using game_typing;
using Godot;
using WanaKanaNet;

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
      PromptLabelBbcode = enemy.Vocab.Entry;
    }

    var currentInput = InputLabelBbcode;
    if (enemy.Vocab.State == VocabState.Active) {
      ZIndex = -1;
      BackgroundContainer.Color = _activeColor;
      var rest = enemy.Vocab.Entry.Substring(enemy.Vocab.InputBuffer.Length + 1);

      var targetInput = string.Concat(enemy.Vocab.InputBuffer, "[red]", Equalize(enemy, enemy.Vocab.Entry.Substring(enemy.Vocab.InputBuffer.Length, 1)), "[]",
        rest.Length > 0 ? "~" + rest + "~" : "");
      if (currentInput != targetInput) {
        InputLabelBbcode = targetInput;
      }
    }
    else {
      ZIndex = -2;
      BackgroundContainer.Color = _inactiveColor;
      if (currentInput != "") {
        InputLabelBbcode = "";
      }
    }

    _currentEnemy = enemy;
  }

  public string Equalize(Enemy enemy, string input) {
    if (enemy.Vocab.NextIsHiragana) {
      return WanaKana.ToHiragana(input);
    }
    if (enemy.Vocab.NextIsKatakana) {
      return WanaKana.ToKatakana(input);
    }
    return input;
  }
}
