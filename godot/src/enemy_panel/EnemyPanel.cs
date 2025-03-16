namespace ankitaiso.enemy_panel;

using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using enemy;
using game_typing;
using Godot;

[SceneTree]
[Meta(typeof(IAutoNode))]
public partial class EnemyPanel : Control {
  public override void _Notification(int what) => this.Notify(what);
  private Enemy? _currentEnemy;

  private Color _activeColor = Color.FromHtml("604540b3");
  private Color _inactiveColor = Color.FromHtml("00000064");


  public string PromptLabelBbcode {
    get => PromptLabel.Get("bbcode").ToString();
    set => PromptLabel.Set("bbcode", value);
  }

  public string TitleLabelBbcode {
    get => TitleLabel.Get("bbcode").ToString();
    set => TitleLabel.Set("bbcode", value);
  }

  public string InputLabelBbcode {
    get => InputLabel.Get("bbcode").ToString();
    set => InputLabel.Set("bbcode", value);
  }

  public string HintLabelBbcode {
    get => HintLabel.Get("bbcode").ToString();
    set => HintLabel.Set("bbcode", value);
  }

  [OnInstantiate]
  private void Initialise() {
    _currentEnemy = null;
  }

  public void UpdateGui(Enemy enemy, int order_index) {
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
      ZIndex = -1 - order_index;
    }
    UpdateVocab(vocab);
    _currentEnemy = enemy;
  }

  public void UpdateVocab(Vocab vocab) {
    if (PromptLabelBbcode != vocab.Entry.Prompt) {
      PromptLabelBbcode = vocab.Entry.Prompt;
    }
    if (vocab.Entry.Title == null) {
      TitleLabel.Hide();
    }
    else {
      TitleLabel.Show();
    }
    if (TitleLabelBbcode != (vocab.Entry.Title ?? "") ) {
      TitleLabelBbcode = vocab.Entry.Title ?? "";
    }
    if (vocab.State == VocabState.Active) {
      BackgroundContainer.Color = _activeColor;
      var rest = vocab.Entry.Prompt[(vocab.InputBuffer.Length + vocab.Next.Length)..];
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
    if (vocab.ShowHint) {
      var targetDebugOutput = vocab.NextVariants != null ? string.Join(", ", vocab.NextVariants) : vocab.Next;
      if (HintLabelBbcode != targetDebugOutput) {
        HintLabelBbcode = targetDebugOutput;
      }
      HintLabel.Show();
    }
    else {
      HintLabel.Hide();
    }
  }
}
