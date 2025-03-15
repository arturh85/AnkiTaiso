namespace ankitaiso.menu.menu_anki;

using System;
using System.Linq;
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
  public void UpdateDialog();
}

[Meta(typeof(IAutoNode))]
[SceneTree]
public partial class MenuAnki : Control, IMenuAnki {
  public override void _Notification(int what) => this.Notify(what);
  private readonly Log _log = new(nameof(MenuAnki), new GDWriter());

  private CardInfo? _cardInfo;
  private string? _deckName;
  private string[]? _deckNames;
  private bool _showOnlyExistingDecks;

  [Signal]
  public delegate void BackEventHandler();

  public void OnBackPressed() => EmitSignal(SignalName.Back);

  public void OnReady() {
    ExampleAnkiDeck.Hide();
    ImportUpdateProgressBar.Hide();

    OnConnectedContainer.Hide();
    OnSelectedContainer.Hide();

    BackButton.Pressed += OnBackPressed;
    FieldPromptSelect.ItemSelected += OnConfigChanged;
    FieldTitleSelect.ItemSelected += OnConfigChanged;
    FieldTranslationSelect.ItemSelected += OnConfigChanged;
    FieldAudioSelect.ItemSelected += OnConfigChanged;
    RandomizeEnemyPanelButton.Pressed += RandomizeEnemyPanel;
    ImportUpdateButton.Pressed += OnImportUpdate;
    UpdateAnkiUrlButton.Pressed += UpdateDialog;
    ToggleShowAllDecksButton.Pressed += ToggleShowAllDecks;
    ToggleShowAllDecks();
  }

  public void OnExitTree() {
    BackButton.Pressed -= OnBackPressed;
    FieldPromptSelect.ItemSelected -= OnConfigChanged;
    FieldTitleSelect.ItemSelected -= OnConfigChanged;
    FieldTranslationSelect.ItemSelected -= OnConfigChanged;
    FieldAudioSelect.ItemSelected -= OnConfigChanged;
    RandomizeEnemyPanelButton.Pressed -= RandomizeEnemyPanel;
    ImportUpdateButton.Pressed -= OnImportUpdate;
    UpdateAnkiUrlButton.Pressed -= UpdateDialog;
    ToggleShowAllDecksButton.Pressed -= ToggleShowAllDecks;
  }

  private void UpdateDialogDecks(string[] deckNames) {
    foreach (var child in AnkiDecksContainer.GetChildren()) {
      child.QueueFree();
    }

    foreach (var deckName in deckNames) {
      if (_showOnlyExistingDecks && !ScenarioManager.ScenarioExists(deckName)) {
        continue;
      }
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

  public async void UpdateDialog() {
    OnConnectedContainer.Hide();
    OnSelectedContainer.Hide();
    ErrorLabel.Text = "";
    try {
      var ankiUrl = new Uri(AnkiUrlEdit.Text.Trim());
      var ankiService = AnkiConnectApi.GetInstance();

      var deckNames = await ankiService.ListDeckNames(ankiUrl);
      _deckNames = deckNames;
      UpdateDialogDecks(deckNames);

      OnConnectedContainer.Show();
    }
    catch (Exception e) {
      ErrorLabel.Text = e.Message;
      _log.Err(e.ToString());
    }
    finally {
      ImportUpdateButton.Disabled = false;
      ImportUpdateProgressBar.Hide();
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
      ImportUpdateProgressBar.Show();
      var ankiUrl = new Uri(AnkiUrlEdit.Text.Trim());
      ErrorLabel.Text = "";
      try {
        var deck = await ScenarioManager.ImportScenario(ankiUrl, _deckName, BuildMapping(), progress =>
          ImportUpdateProgressBar.Value = progress);
        ErrorLabel.Text = $"Successfully imported {deck.Entries.Length} entries!";
      }
      catch (Exception e) {
        ErrorLabel.Text = e.Message;
      }
    }
    catch (Exception e) {
      ErrorLabel.Text = e.Message;
      _log.Err(e.ToString());
    }
    finally {
      ImportUpdateButton.Disabled = false;
      ImportUpdateProgressBar.Hide();
    }
  }

  private void ToggleShowAllDecks() {
    _showOnlyExistingDecks = !_showOnlyExistingDecks;
    if (_showOnlyExistingDecks) {
      ToggleShowAllDecksButton.Text = "Show all Decks";
    }
    else {
      ToggleShowAllDecksButton.Text = "Show only existing Decks";
    }
    if (_deckNames != null) {
      UpdateDialogDecks(_deckNames);
    }
  }

  private VocabMapping BuildMapping() =>
    new() {
      PromptKey = FieldPromptSelect.Selected == -1 ? null! : FieldPromptSelect.GetItemText(FieldPromptSelect.Selected),
      TranslationKey = FieldTranslationSelect.Selected == -1 ? null : FieldTranslationSelect.GetItemText(FieldTranslationSelect.Selected),
      TitleKey = FieldTitleSelect.Selected == -1 ? null : FieldTitleSelect.GetItemText(FieldTitleSelect.Selected),
      AudioKey = FieldAudioSelect.Selected == -1 ? null : FieldAudioSelect.GetItemText(FieldAudioSelect.Selected)
    };

  private void OnConfigChanged(long _index) {
    if (_cardInfo == null) {
      return;
    }

    var vocabEntry = ScenarioManager.BuildVocabEntry(_cardInfo, BuildMapping());
    var vocab = new Vocab(vocabEntry);
    PreviewEnemyPanel.UpdateVocab(vocab);
  }

  private async void OnDeckSelected(string deckName) {
    _deckName = deckName;
    ErrorLabel.Text = "";
    OnSelectedContainer.Show();
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
      NodeUtils.ClearOptions(FieldTranslationSelect);
      NodeUtils.ClearOptions(FieldTitleSelect);
      NodeUtils.ClearOptions(FieldAudioSelect);
      var fields = _cardInfo.Fields.OrderBy(f => f.Value.Order).ToArray();
      foreach (var (fieldId, _) in fields) {
        FieldPromptSelect.AddItem(fieldId);
        FieldTitleSelect.AddItem(fieldId);
        FieldTranslationSelect.AddItem(fieldId);
        FieldAudioSelect.AddItem(fieldId);
      }

      var mapping = ScenarioManager.LoadMapping(_deckName);
      if (mapping != null) {
        FieldPromptSelect.Selected = Array.FindIndex(fields, f => f.Key == mapping.PromptKey);
        FieldTitleSelect.Selected = Array.FindIndex(fields, f => f.Key == mapping.TitleKey);
        FieldTranslationSelect.Selected = Array.FindIndex(fields, f => f.Key == mapping.TranslationKey);
        FieldAudioSelect.Selected = Array.FindIndex(fields, f => f.Key == mapping.AudioKey);
      }
      else {
        FieldPromptSelect.Selected = -1;
        FieldTitleSelect.Selected = -1;
        FieldTranslationSelect.Selected = -1;
        FieldAudioSelect.Selected = -1;
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

    if (Input.IsActionJustPressed(BuiltinInputActions.UIAccept)) {
      OnImportUpdate();
    }
    else if (Input.IsActionJustPressed(BuiltinInputActions.UICancel)) {
      OnBackPressed();
    }
  }
}
