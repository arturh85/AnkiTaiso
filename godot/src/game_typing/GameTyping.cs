using Enemy = ankitaiso.enemy.Enemy;

namespace ankitaiso.game_typing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using app.domain;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using domain;
using enemy_panel;
using Godot;
using state;

[Meta(typeof(IAutoNode))]
public partial class GameTyping : Node3D {
  public override void _Notification(int what) => this.Notify(what);

  public IGameTypingLogic GameTypingLogic { get; set; } = default!;

  public LogicBlock<GameTypingLogic.State>.IBinding GameTypingBinding { get; set; } = default!;

  public Vector3 PlayerPosition;
  public Vector3 SpawnPosition;

  private Enemy? activeEnemy;
  private const int MaxEnemyPanels = 4;
  public Stack<string> WordList = new();

  [Dependency] public IAppRepo AppRepo => this.DependOn<IAppRepo>();
  [Dependency] public IGameTypingRepo GameTypingRepo => this.DependOn<IGameTypingRepo>();
  [Dependency] public GameTypingSystem GameTypingSystem => this.DependOn<GameTypingSystem>();

  [Node] public IRichTextLabel BufferLabel { get; set; } = default!;
  [Node] public IControl GuiControls { get; set; } = default!;
  [Node] public Node3D EnemiesContainer { get; set; } = default!;
  [Node] public ITimer SpawnTimer { get; set; } = default!;
  public string BufferLabelBbcode {
    get => BufferLabel.Get("bbcode").ToString();
    set => BufferLabel.Set("bbcode", value);
  }

  public void Setup() {
    // GameTypingLogic = new GameTypingLogic();
    // GameTypingLogic.Set(GameTypingRepo);
    // GameTypingBinding = GameTypingLogic.Bind();
    // GameTypingBinding
    //   .Handle(
    //     (in GameTypingLogic.Output.StartGame _) => {
    //     });
    //
    // // Trigger the first state's OnEnter callbacks so our bindings run.
    // // Keeps everything in sync from the moment we start!
    // GameTypingLogic.Start();
    // GD.Print("Game State Start");
    //
    // GameTypingLogic.Input(new GameLogic.Input.Initialize());

    this.Provide();
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
    SetPaused(true);
    Setup();

    for (int i = 0; i < MaxEnemyPanels; i++) {
      var panel = EnemyPanel.Instantiate();
      panel.Hide();
      GuiControls.AddChild(panel);
    }
    BufferLabelBbcode = "";
  }

  public void OnResolved() {

  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) {
    if (_paused)
      return;
    var nearest = Enemy.GetNearestEnemies(PlayerPosition, EnemiesContainer, MaxEnemyPanels);

    var idx = 0;
    foreach (var child in GuiControls.GetChildren()) {
      if (child is EnemyPanel panel) {
        if (nearest.Count <= idx) {
          panel.Hide();
        }
        else {
          var enemy = nearest[idx];
          panel.UpdateGui(enemy);
        }

        idx += 1;
      }
    }

    if (BufferLabelBbcode != GameTypingSystem.Buffer) {
      BufferLabelBbcode = GameTypingSystem.Buffer;
    }
  }


  private bool _paused = true;
  private bool _gameStarted;


  public void StartGame() {
    LoadWordlist();
    GameTypingSystem.RestartGame(WordList);
    _gameStarted = true;
    SetPaused(false);
  }

  public void SetPaused(bool paused) {
    if (!_gameStarted) {
      return;
    }
    _paused = paused;
    if (_paused) {
      SpawnTimer.Stop();
    }
    else {
      SpawnTimer.Start();
    }
  }


  public override void _Input(InputEvent @event) {
    if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
      GameTypingSystem?.OnInput(keyEvent.Keycode);
    }
  }

  private static void Shuffle<T>(IList<T> list) {
    var n = list.Count;
    while (n > 1) {
      n--;
      var k = Random.Shared.Next(n + 1);
      (list[k], list[n]) = (list[n], list[k]);
    }
  }

  public void LoadWordlist() {
    var scenario = GameTypingRepo.ActiveScenario;
    if (scenario == null) {
      return;
    }

    var content = scenario.ReadWordList();
    WordList.Clear();
    var lines = LinesRegex().Split(content);
    var words = lines.Select(line => line.Split("|")[0]).ToList();
    Shuffle(words);
    var options = GameTypingRepo.ActiveScenarioOptions;
    if (options == null) {
      return;
    }
    for (int i = 0; i < Math.Min(options.WordsPlayed, words.Count); i++) {
      WordList.Push(words[i]);
    }
  }


  public void SpawnEnemy() {
    if (EnemiesContainer.GetChildren().Count >= 5) {
      return;
    }
    if (GameTypingSystem == null) {
      GD.Print("Missing GameTypingSystem");
      return;
    }
    var vocab = GameTypingSystem.NextEntry(false);
    if (vocab == null) {
      return;
    }
    const float spawnRadius = 5.0f;
    const float minDistanceBetweenEnemies = 2.0f;
    const int maxSpawnAttempts = 10;
    var rng = new RandomNumberGenerator();
    rng.Randomize();
    Vector3 spawnPosition = FindValidSpawnPosition3D(rng, spawnRadius, minDistanceBetweenEnemies, maxSpawnAttempts);
    var enemy = CreateEnemy3D(vocab, spawnPosition);
    EnemiesContainer.AddChild(enemy);
  }

  private Vector3 FindValidSpawnPosition3D(RandomNumberGenerator rng, float radius, float minDistance,
    int maxAttempts) {
    var existingEnemies = Enumerable.Cast<Enemy>(EnemiesContainer.GetChildren()).ToList();

    for (int i = 0; i < maxAttempts; i++) {
      Vector3 candidatePosition = GenerateRandomPositionInArc(rng, SpawnPosition, radius, MathF.PI / 2);

      if (!IsPositionOccupied3D(candidatePosition, existingEnemies, minDistance)) {
        return candidatePosition;
      }
    }

    // Fallback to random position without collision check
    return GenerateRandomPositionInArc(rng, SpawnPosition, radius, MathF.PI / 2);
  }

  private Vector3 GenerateRandomPositionInArc(RandomNumberGenerator rng, Vector3 center, float radius,
    float arcAngleRadians) {
    // Calculate direction from player to the center point
    Vector3 toCenter = center - PlayerPosition;
    // Get the angle of this direction in the XZ plane (azimuth)
    float thetaCenter = Mathf.Atan2(toCenter.Z, toCenter.X);
    // Random angle within the arc around the center angle
    float randomAngle = rng.RandfRange(thetaCenter - arcAngleRadians / 2, thetaCenter + arcAngleRadians / 2);
    // Random distance from the player (0 to radius)
    float distance = toCenter.Length();

    // Calculate position in XZ plane around the player's position
    float x = PlayerPosition.X + distance * Mathf.Cos(randomAngle);
    float z = PlayerPosition.Z + distance * Mathf.Sin(randomAngle);

    // Keep Y coordinate same as the center's Y (or set to PlayerPosition.Y if needed)
    return new Vector3(x, center.Y, z);
  }

  private bool IsPositionOccupied3D(Vector3 position, List<Enemy> existingEnemies, float minDistance) {
    foreach (var enemy in existingEnemies) {
      if (enemy.Position.DistanceTo(position) < minDistance) {
        return true;
      }
    }

    return false;
  }

  private Enemy CreateEnemy3D(Vocab vocab, Vector3 position) {
    var enemy = Enemy.Instantiate(vocab, PlayerPosition);
    enemy.Position = position;
    return enemy;
  }

  private string PopRandomWord() => WordList.Count > 0 ? WordList.Pop() : "empty";

  public void _on_timer_timeout() => SpawnEnemy();

  public void _on_active_enemy_deleted() {
    activeEnemy = null;
    GameTypingRepo.IncreaseClearedWords();
  }

  [GeneratedRegex("\r\n|\r|\n")]
  private static partial Regex LinesRegex();
}
