using Enemy = ankitaiso.enemy.Enemy;

namespace ankitaiso.game_typing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

  public Vector3 PlayerPosition;
  public Vector3 SpawnPosition;
  public player_camera.PlayerCamera? Camera;
  public List<CameraWaypoint>? Waypoints;

  private Enemy? _activeEnemy;
  private const int MAX_ENEMY_PANELS = 50;
  public Stack<string> WordList = new();

  private double _timeLastWaypoint;
  private double _timeStartcamera;
  private int _currentWaypoint;

  [Dependency] public IAppRepo AppRepo => this.DependOn<IAppRepo>();
  [Dependency] public IGameTypingRepo GameTypingRepo => this.DependOn<IGameTypingRepo>();
  [Dependency] public GameTypingSystem GameTypingSystem => this.DependOn<GameTypingSystem>();

  public string BufferLabelBbcode {
    get => BufferLabel.Get("bbcode").ToString();
    set => BufferLabel.Set("bbcode", value);
  }

  public void OnReady() {
    SetPaused(true);
    for (var i = 0; i < MAX_ENEMY_PANELS; i++) {
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
    if (_paused || Waypoints == null || Camera == null) {
      return;
    }

    var waypoint = Waypoints[_currentWaypoint];
    if (_timeLastWaypoint == 0.0) { // standing at waypoint

      PlayerPosition = waypoint.CameraPosition;
      Camera.Position = PlayerPosition;
      Camera.Rotation = new Vector3(Camera.Rotation.X, waypoint.Rotation.Y, Camera.Rotation.Z);

      foreach (var spawner in waypoint.GetChildren().OfType<ZombieSpawner>()) {

        SpawnEnemy(spawner);
      }

      _timeLastWaypoint += delta;
    }
    else if (EnemiesContainer.GetChildCount() == 0 && _timeStartcamera == 0.0) { // finished waypoint
      _timeStartcamera = _timeLastWaypoint;
      if (_currentWaypoint == Waypoints.Count - 1) {
        _currentWaypoint = 0;
      }
      _timeLastWaypoint += delta;
    }

    if (_timeStartcamera > 0.0 && _currentWaypoint < Waypoints.Count - 1) { // moving to next waypoint
      var elapsed = _timeLastWaypoint - _timeStartcamera;
      var distance = elapsed * waypoint.Speed;
      double realDistance = (waypoint.CameraPosition - Waypoints[_currentWaypoint + 1].CameraPosition).Length();
      var percentDistance = (float)(distance / realDistance);

      Camera.GlobalPosition = waypoint.CameraPosition + ((Waypoints[_currentWaypoint + 1].CameraPosition - waypoint.CameraPosition) * percentDistance);
      var camRotation = waypoint.Rotation.Y + ((Waypoints[_currentWaypoint + 1].Rotation.Y - waypoint.Rotation.Y) * percentDistance);
      Camera.Rotation = new Vector3(Camera.Rotation.X, camRotation, Camera.Rotation.Z);

      if (percentDistance >= 1.0 - (waypoint.Speed * delta * 2.0)) {

        _timeStartcamera = 0.0;
        _timeLastWaypoint = 0.0;
        _currentWaypoint++;
      }
      else {
        _timeLastWaypoint += delta;
      }

    }



    var nearest = NodeUtils.NearestNodes<Enemy>(PlayerPosition, EnemiesContainer, MAX_ENEMY_PANELS, e => e.Moving);

    var idx = 0;
    foreach (var child in GuiControls.GetChildren()) {
      if (child is EnemyPanel panel) {
        if (idx >= nearest.Count || nearest[idx].dead) {
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


  private void SpawnEnemy(ZombieSpawner spawner) {

    Vocab? vocab = GameTypingSystem.NextEntry(false, spawner.MinWordLength, spawner.MaxWordLength);

    //Random.Shared.NextSingle()
    var spawnPosition = spawner.GroundPosition;
    var enemy = CreateEnemy3D(vocab, spawnPosition);
    EnemiesContainer.AddChild(enemy);
  }

  private Vector3 FindValidSpawnPosition3D(RandomNumberGenerator rng, float radius, float minDistance,
    int maxAttempts) {
    var existingEnemies = Enumerable.Cast<Enemy>(EnemiesContainer.GetChildren()).ToList();

    for (var i = 0; i < maxAttempts; i++) {
      var candidatePosition = NodeUtils.RandomPositionInArc(rng, PlayerPosition, SpawnPosition, radius);

      if (!IsPositionOccupied3D(candidatePosition, existingEnemies, minDistance)) {
        return candidatePosition;
      }
    }

    // Fallback to random position without collision check
    return NodeUtils.RandomPositionInArc(rng, PlayerPosition, SpawnPosition, radius);
  }


  private static bool IsPositionOccupied3D(Vector3 position, List<Enemy> existingEnemies, float minDistance) {
    foreach (var enemy in existingEnemies) {
      if (enemy.Position.DistanceTo(position) < minDistance) {
        return true;
      }
    }
    return false;
  }

  private Enemy CreateEnemy3D(Vocab vocab, Vector3 position) {
    var enemy = Enemy.Instantiate(vocab, new Vector3(PlayerPosition.X, 0, PlayerPosition.Z));
    enemy.Position = position;
    return enemy;
  }

  private string PopRandomWord() => WordList.Count > 0 ? WordList.Pop() : "empty";

  private void _on_active_enemy_deleted() => _activeEnemy = null;

  [GeneratedRegex("\r\n|\r|\n")]
  private static partial Regex LinesRegex();
}
