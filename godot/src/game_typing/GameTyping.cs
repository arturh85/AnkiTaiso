using Enemy = ankitaiso.enemy.Enemy;

namespace ankitaiso.game_typing;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using app.domain;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using domain;
using enemy_panel;
using Godot;
using map;
using utils;

[Meta(typeof(IAutoNode))]
[SceneTree]
public partial class GameTyping : Node3D {
  public override void _Notification(int what) => this.Notify(what);

  public CameraWaypoint CurrentWaypoint;
  public Vector3 SpawnPosition;
  public Node3D Levels;
  public Area3D Player;
  public player_camera.PlayerCamera? Camera;

  private Enemy? _activeEnemy;
  private const int MAX_ENEMY_PANELS = 50;
  public Stack<string> WordList = new();

  private double _timeLastWaypoint;
  private double _timeStartcamera;
  private int _currentWaypoint;
  private double _pauseTrigger = 0.0;
  private float _speed = 1.5f;

  private Follower _follower;
  [Dependency] public IAppRepo AppRepo => this.DependOn<IAppRepo>();
  [Dependency] public IGameTypingRepo GameTypingRepo => this.DependOn<IGameTypingRepo>();
  [Dependency] public GameTypingSystem GameTypingSystem => this.DependOn<GameTypingSystem>();

  public string BufferLabelBbcode {
    get => BufferLabel.Get("bbcode").ToString();
    set => BufferLabel.Set("bbcode", value);
  }

  public void OnReady() {
    SetPaused(true);
    var thread = new Thread(delegate() {
      for (var i = 0; i < MAX_ENEMY_PANELS; i++) {
        var panel = EnemyPanel.Instantiate();
        panel.Hide();
        GuiControls.CallDeferred(Node.MethodName.AddChild, [panel]);
      }
    });
    thread.Start();
    BufferLabelBbcode = "";

    _follower = new Follower() { RotationMode = PathFollow3D.RotationModeEnum.Y };
  }

  public void OnResolved() {

  }

  public void StopCamera(float waitTime) {
    _pauseTrigger = waitTime;
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) {
    if (_paused || Camera == null) {
      return;
    }

    if (_pauseTrigger > 0.0) {
      _pauseTrigger = Math.Max(_pauseTrigger - delta, 0.0);
    }
    else {

      Camera.GlobalPosition = _follower.GlobalPosition;
      Player.GlobalPosition = _follower.GlobalPosition;
      Camera.Rotation = _follower.Rotation;

      foreach (Enemy enemy in EnemiesContainer.GetChildren().OfType<Enemy>()) {
        enemy.UpdateMovementTarget(_follower);
      }

      _follower.SetProgress((float)delta * _speed + _follower.Progress);
    }


    var nearest = NodeUtils.NearestNodes<Enemy>(Player.GlobalPosition, EnemiesContainer, MAX_ENEMY_PANELS, e => e.Moving);

    var idx = 0;
    foreach (var child in GuiControls.GetChildren()) {
      if (child is EnemyPanel panel) {
        if (idx >= nearest.Count || nearest[idx].Dead) {
          panel.Hide();
        }
        else {
          var enemy = nearest[idx];
          panel.UpdateGui(enemy, idx);
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
    GameTypingSystem.RestartGame(WordList.Select(w => new VocabEntry(w)));
    _gameStarted = true;
    SetPaused(false);

    Levels.GetNode("Level1").AddChild(_follower);

  }

  public void SetPaused(bool paused) {
    if (!_gameStarted) {
      return;
    }
    _paused = paused;
    if (_paused) {

    }
    else {

    }
  }

  public override void _Input(InputEvent @event) {
    if (@event is InputEventKey { Pressed: true } keyEvent) {
      GameTypingSystem.OnInput(keyEvent.Keycode);
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
    var scenario = this.GameTypingRepo.ActiveScenario;
    if (scenario == null) {
      return;
    }

    var content = scenario.ReadWordList();
    WordList.Clear();
    var lines = LinesRegex().Split(content);
    var words = lines.ToList();
    Shuffle(words);
    var options = GameTypingRepo.ActiveScenarioOptions;
    if (options == null) {
      return;
    }
    for (var i = 0; i < Math.Min(words.Count, words.Count); i++) {
      var word = words[i].Trim();
      if (word.Length > 0) {
        WordList.Push(word);
      }
    }
  }

  public void SpawnEnemy(ZombieSpawner spawner) {
    var vocab = GameTypingSystem.NextEntry(false, spawner.MinWordLength, spawner.MaxWordLength);
    if (vocab == null) {
      return;
    }
    var startTime = spawner.MinSpawnTime + Random.Shared.NextSingle() * (spawner.MaxSpawnTime - spawner.MinSpawnTime);
    var spawnPosition = spawner.GroundPosition;
    var enemy = Enemy.Instantiate(vocab, CurrentWaypoint, startTime);
    EnemiesContainer.AddChild(enemy);
    enemy.GlobalPosition = spawnPosition;
    spawner.Spawned = true;
  }

  private Vector3 FindValidSpawnPosition3D(RandomNumberGenerator rng, float radius, float minDistance,
    int maxAttempts) {
    var existingEnemies = Enumerable.Cast<Enemy>(EnemiesContainer.GetChildren()).ToList();

    for (var i = 0; i < maxAttempts; i++) {
      var candidatePosition = NodeUtils.RandomPositionInArc(rng, Player.GlobalPosition, SpawnPosition, radius);

      if (!IsPositionOccupied3D(candidatePosition, existingEnemies, minDistance)) {
        return candidatePosition;
      }
    }

    // Fallback to random position without collision check
    return NodeUtils.RandomPositionInArc(rng, Player.GlobalPosition, SpawnPosition, radius);
  }


  private static bool IsPositionOccupied3D(Vector3 position, List<Enemy> existingEnemies, float minDistance) {
    foreach (var enemy in existingEnemies) {
      if (enemy.Position.DistanceTo(position) < minDistance) {
        return true;
      }
    }
    return false;
  }

  private string PopRandomWord() => WordList.Count > 0 ? WordList.Pop() : "empty";

  private void _on_active_enemy_deleted() => _activeEnemy = null;

  [GeneratedRegex("\r\n|\r|\n")]
  private static partial Regex LinesRegex();
}
