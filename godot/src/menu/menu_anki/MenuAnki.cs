namespace ankitaiso.menu.menu_anki;

using System;
using System.Linq;
using System.Threading.Tasks;
using app.domain;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.Log;
using Chickensoft.Log.Godot;
using domain;
using game_typing;
using game_typing.domain;
using Godot;
using utils;

public interface IMenuAnki : IControl {
  event MenuAnki.BackEventHandler Back;
  public Task UpdateDialog();
}

[Meta(typeof(IAutoNode))]
[SceneTree]
public partial class MenuAnki : Control, IMenuAnki {
  public override void _Notification(int what) => this.Notify(what);
  private readonly Log _log = new (nameof(MenuAnki), new GDWriter());

  private CardInfo? _cardInfo;
  private string? _deckName;

  [Signal]
  public delegate void BackEventHandler();

  public void OnBackPressed() => EmitSignal(SignalName.Back);

  public void OnReady() {
    ExampleAnkiDeck.Hide();
    BackButton.Pressed += OnBackPressed;
    FieldPromptSelect.ItemSelected += OnConfigChanged;
    FieldKanjiSelect.ItemSelected += OnConfigChanged;
    RandomizeEnemyPanelButton.Pressed += RandomizeEnemyPanel;
    ImportUpdateButton.Pressed += OnImportUpdate;
  }

  public void OnExitTree() {
    BackButton.Pressed -= OnBackPressed;
    FieldPromptSelect.ItemSelected -= OnConfigChanged;
    FieldKanjiSelect.ItemSelected -= OnConfigChanged;
    RandomizeEnemyPanelButton.Pressed -= RandomizeEnemyPanel;
    ImportUpdateButton.Pressed -= OnImportUpdate;
  }


  public async Task UpdateDialog() {
    var ankiUrl = new Uri(AnkiUrlEdit.Text.Trim());
    var ankiService = AnkiConnectApi.GetInstance();

    var deckNames = await ankiService.ListDeckNames(ankiUrl);
    // clear existing childs
    foreach (var child in AnkiDecksContainer.GetChildren()) {
      child.QueueFree();
    }
    foreach (var deckName in deckNames) {
      var control = (ExampleAnkiDeck.Duplicate() as Control)!;
      var button = (
        control.GetNode("Button") as
          Button)!;
      button.Text = deckName;
      button.Pressed += () => OnDeckSelected(deckName);
      var label = (control.GetNode("Label") as Label)!;
      label.Text = deckName;
      control.Show();
      AnkiDecksContainer.AddChild(control);
    }
  }
  private async void RandomizeEnemyPanel() {
    if (_deckName == null) {
      return;
    }
    var ankiUrl = new Uri(AnkiUrlEdit.Text.Trim());
    var ankiService = AnkiConnectApi.GetInstance();
    ErrorLabel.Text = "";

    var cardIds = await ankiService.FindCardsByDeck(ankiUrl, _deckName);
    if (cardIds.Length == 0) {
      ErrorLabel.Text = "No cards found!";
      return;
    }
    var cardId = CollectionUtils.RandomEntry(cardIds);
    var cardInfos = await ankiService.CardsInfo(ankiUrl, [cardId]);
    if (cardInfos.Length == 0) {
      ErrorLabel.Text = "No cardInfo found!";
      return;
    }
    _cardInfo = cardInfos[0];
    OnConfigChanged(0);
  }
  private async void OnImportUpdate() {
    if (_deckName == null) {
      return;
    }
    ErrorLabel.Text = "";
    try {
      ImportUpdateButton.Disabled = true;
      var ankiUrl = new Uri(AnkiUrlEdit.Text.Trim());
      var ankiService = AnkiConnectApi.GetInstance();
      var cardIds = await ankiService.FindCardsByDeck(ankiUrl, _deckName);
      if (cardIds.Length == 0) {
        ErrorLabel.Text = "No cards found!";
        // return;
      }
      // const int batchSize = 10;



    }
    catch (Exception e) {
      ErrorLabel.Text = e.Message;
      _log.Err(e.ToString());
    }
    finally {
      ImportUpdateButton.Disabled = false;
    }
  }


  private void OnConfigChanged(long _index) {
    if (_cardInfo == null) {
      return;
    }
    var prompt = FieldPromptSelect.GetItemText(FieldPromptSelect.Selected);
    var kanji = FieldKanjiSelect.GetItemText(FieldKanjiSelect.Selected);
    var value = _cardInfo.Fields[prompt].Value;
    if (value == null) {
      return;
    }
    var vocab = new Vocab(value);
    PreviewEnemyPanel.UpdateVocab(vocab);
  }

  private async void OnDeckSelected(string deckName) {
    _deckName = deckName;
    ErrorLabel.Text = "";

    try {
      var ankiUrl = new Uri(AnkiUrlEdit.Text.Trim());
      var ankiService = AnkiConnectApi.GetInstance();
      var cardIds = await ankiService.FindCardsByDeck(ankiUrl, deckName);
      if (cardIds.Length == 0) {
        ErrorLabel.Text = "No cards found!";
        return;
      }
      var cardId = cardIds[0];
      var cardInfos = await ankiService.CardsInfo(ankiUrl, [cardId]);
      if (cardInfos.Length == 0) {
        ErrorLabel.Text = "No cardInfo found!";
        return;
      }

      _cardInfo = cardInfos[0];
      NodeUtils.ClearOptions(FieldPromptSelect);
      NodeUtils.ClearOptions(FieldKanjiSelect);
      foreach (var (fieldId,fieldInfo) in _cardInfo.Fields.OrderBy(f => f.Value.Order)) {
        FieldPromptSelect.AddItem(fieldId);
        FieldKanjiSelect.AddItem(fieldId);
      }
    }
    catch (Exception e) {
      ErrorLabel.Text = e.Message;
      _log.Err(e.ToString());
    }
  }

  [Dependency] public IAppRepo AppRepo => DependentExtensions.DependOn<IAppRepo>(this);
  [Dependency] public IGameTypingRepo GameTypingRepo => DependentExtensions.DependOn<IGameTypingRepo>(this);
  [Dependency] public IMenuRepo MenuRepo => DependentExtensions.DependOn<IMenuRepo>(this);

  public override void _Input(InputEvent @event) {
    if (!Visible) {
      return;
    }
    //
    // if (Input.IsActionJustPressed(BuiltinInputActions.UIAccept)) {
    //   OnNewGamePressed();
    // }
    // else if (Input.IsActionJustPressed(BuiltinInputActions.UICancel)) {
    //   OnQuitPressed();
    // }
  }


}
