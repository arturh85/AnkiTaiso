namespace ankitaiso.menu.menu_anki;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
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
  private bool _showAllDecks = true;

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
      if (!_showAllDecks && !FileAccess.FileExists($"{DeckDirPath(deckName)}/{_deckFilename}")) {
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

  private static string DeckDirPath(string deckName) => $"user://scenarios/{deckName}";
  private string _deckFilename = "deck.json";

  private async void OnImportUpdate() {
    if (_deckName == null) {
      return;
    }

    ErrorLabel.Text = "";
    try {
      ImportUpdateButton.Disabled = true;
      ImportUpdateProgressBar.Show();
      var ankiUrl = new Uri(AnkiUrlEdit.Text.Trim());
      var ankiService = AnkiConnectApi.GetInstance();
      var cardIds = await ankiService.FindCardsByDeck(ankiUrl, _deckName);
      if (cardIds.Length == 0) {
        ErrorLabel.Text = "No cards found!";
        // return;
      }

      const int batchSize = 10;

      var promptKey = FieldPromptSelect.GetItemText(FieldPromptSelect.Selected);
      var titleKey = FieldTitleSelect.GetItemText(FieldTitleSelect.Selected);
      var translationKey = FieldTranslationSelect.GetItemText(FieldTranslationSelect.Selected);
      var audioKey = FieldAudioSelect.GetItemText(FieldAudioSelect.Selected);
      var vocabDeck = new VocabDeck {
        Title = _deckName,
        Entries = [],
        PromptKey = promptKey,
        TitleKey = titleKey,
        TranslationKey = translationKey,
        AudioKey = audioKey
      };
      List<VocabEntry> entries = [];

      var dirPath = DeckDirPath(_deckName);
      DirAccess.MakeDirRecursiveAbsolute(dirPath);

      for (var i = 0; i < cardIds.Length; i += batchSize) {
        var batch = cardIds.Skip(i).Take(batchSize).ToArray();
        var cardInfos = await ankiService.CardsInfo(ankiUrl, batch);
        if (cardInfos.Length == 0) {
          ErrorLabel.Text = "No cardInfo found!";
          return;
        }

        foreach (var cardInfo in cardInfos) {
          var entry = BuildVocabEntry(cardInfo);
          entries.Add(entry);

          if (entry.AudioFilename != null && !FileAccess.FileExists($"{DeckDirPath(_deckName)}/{entry.AudioFilename}")) {
            GD.Print(entry.AudioFilename);
            var contents = await ankiService.RetrieveMediaFile(ankiUrl, entry.AudioFilename);
            using var audioFile = FileAccess.Open($"{dirPath}/{entry.AudioFilename}", FileAccess.ModeFlags.Write);
            if (audioFile == null) {
              throw new GameException($"failed to create {_deckFilename} for '{_deckName}'");
            }
            audioFile.StoreBuffer(contents);
          }
        }

        var progress = (double)(i + batch.Length) / cardIds.Length * 100;
        ImportUpdateProgressBar.Value = progress;
      }

      vocabDeck.Entries = entries.ToArray();
      var json = JsonSerializer.Serialize(vocabDeck, JsonSerializerOptions.Web);

      using var file = FileAccess.Open($"{dirPath}/{_deckFilename}", FileAccess.ModeFlags.Write);
      if (file == null) {
        throw new GameException($"failed to create {_deckFilename} for '{_deckName}'");
      }

      file.StoreString(json);
      ErrorLabel.Text = $"Successfully imported {cardIds.Length} cards!";
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
    _showAllDecks = !_showAllDecks;
    if (_showAllDecks) {
      ToggleShowAllDecksButton.Text = "Hide all decks";
    }
    else {
      ToggleShowAllDecksButton.Text = "Show all decks";
    }

    if (_deckNames != null) {
      UpdateDialogDecks(_deckNames);
    }
  }


  private VocabEntry BuildVocabEntry(CardInfo card) {
    var promptKey = FieldPromptSelect.GetItemText(FieldPromptSelect.Selected);
    var titleKey = FieldTitleSelect.GetItemText(FieldTitleSelect.Selected);
    var translationKey = FieldTranslationSelect.GetItemText(FieldTranslationSelect.Selected);
    var audioKey = FieldAudioSelect.GetItemText(FieldAudioSelect.Selected);
    var audioFilename = card.Fields[audioKey].Value;
    var pattern = @"\[sound:(.*?)\]";
    var match = Regex.Match(audioFilename ?? "", pattern);
    if (match.Success) {
      audioFilename = match.Groups[1].ToString();
    } else if (audioFilename == "") {
      audioFilename = null;
    }
    return new VocabEntry {
      Prompt = card.Fields[promptKey].Value ?? "missing",
      Translation = card.Fields[translationKey].Value,
      AudioFilename = audioFilename,
      Title = card.Fields[titleKey].Value
    };
  }

  private void OnConfigChanged(long _index) {
    if (_cardInfo == null) {
      return;
    }

    var vocabEntry = BuildVocabEntry(_cardInfo);
    var vocab = new Vocab(vocabEntry);
    PreviewEnemyPanel.UpdateVocab(vocab);
  }

  private VocabDeck? LoadDeck(string deckName) {
    var dirPath = DeckDirPath(deckName);
    var filePath = $"{dirPath}/{_deckFilename}";
    if (!FileAccess.FileExists(filePath)) {
      return null;
    }

    using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
    if (file == null) {
      throw new GameException($"failed to create {_deckFilename} for '{deckName}'");
    }

    var obj = JsonSerializer.Deserialize<VocabDeck>(file.GetAsText(), JsonSerializerOptions.Web);
    return obj;
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

      var deck = LoadDeck(_deckName);
      if (deck != null) {
        FieldPromptSelect.Selected = Array.FindIndex(fields, f => f.Key == deck.PromptKey);
        FieldTitleSelect.Selected = Array.FindIndex(fields, f => f.Key == deck.TitleKey);
        FieldTranslationSelect.Selected = Array.FindIndex(fields, f => f.Key == deck.TranslationKey);
        FieldAudioSelect.Selected = Array.FindIndex(fields, f => f.Key == deck.AudioKey);
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
