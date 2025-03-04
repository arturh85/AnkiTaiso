using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Godot;
using kyoukaitansa.game_typing.domain;
using kyoukaitansa.game_typing.state;
using kyoukaitansa.game.state;
using Enemy = kyoukaitansa.enemy.Enemy;

namespace kyoukaitansa.game_typing;

[SceneTree]
public partial class GameTyping : Node3D {
  #region State

  public IGameTypingRepo GameTypingRepo { get; set; } = default!;
  public IGameTypingLogic GameTypingLogic { get; set; } = default!;

  public GameTypingLogic.IBinding GameTypingBinding { get; set; } = default!;

  #endregion State


  public Vector3 PlayerPosition;
  public Vector3 SpawnPosition;

  private Enemy? activeEnemy;
  private const int MaxEnemyPanels = 4;
  public List<string> WordList = new List<string>();

  // public PackedScene myEnemy;

  public void Setup() {
    GameTypingRepo = new GameTypingRepo();
    GameTypingLogic = new GameTypingLogic();
    GameTypingLogic.Set(GameTypingRepo);
    GameTypingBinding = GameTypingLogic.Bind();
    GameTypingBinding
      .Handle(
        (in GameTypingLogic.Output.StartGame _) => {
        });

    // Trigger the first state's OnEnter callbacks so our bindings run.
    // Keeps everything in sync from the moment we start!
    GameTypingLogic.Start();
    GD.Print("Game State Start");

    GameTypingLogic.Input(new GameLogic.Input.Initialize());
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
    GD.Print("GameTyping Ready");
    LoadWordlist();
    SetPaused(true);
    Setup();

    for (int i = 0; i < MaxEnemyPanels; i++) {
      var panel = EnemyPanel.Instantiate();
      panel.Hide();
      _.GuiControls.AddChild(panel);
    }
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) {
    if (paused) return;
    var nearest = Enemy.GetNearestEnemies(PlayerPosition, _.Enemies, MaxEnemyPanels);

    var idx = 0;
    foreach (var child in _.GuiControls.GetChildren()) {
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
  }


  public bool paused = true;
  public bool game_started = false;


  public void StartGame() {
    game_started = true;
    SetPaused(false);
  }

  public void SetPaused(bool _paused) {
    if (!game_started) {
      return;
    }

    paused = _paused;
    if (_paused) {
      _.Timer.Stop();
    }
    else {
      _.Timer.Start();
    }
  }

  public override void _Input(InputEvent @event)
  {
    if (@event is InputEventKey keyEvent && keyEvent.Pressed)
    {
      if (activeEnemy != null) {
        activeEnemy.OnInput(keyEvent);
      }
      else {
        var s = OS.GetKeycodeString(keyEvent.Keycode);
        GD.Print(s);
        foreach (var enemy in _.Enemies.GetChildren().OfType<Enemy>()) {
          if (enemy.Prompt.ToUpper().StartsWith(s)) {
            activeEnemy = enemy;
            activeEnemy.OnInput(keyEvent);
            activeEnemy.OnDelete += _on_active_enemy_deleted;
            break;
          }
        }
      }
    }
  }

  public void LoadWordlist() {
    var file = FileAccess.Open("res://assets/top_english_nouns_lower_10000.txt", FileAccess.ModeFlags.Read);
    var content = file.GetAsText();
    WordList.Clear();
    var lines = Regex.Split(content, "\r\n|\r|\n");
    foreach (var line in lines) {
      WordList.Add(line);
    }

    GD.Print("WordList Loaded");
  }


  public void SpawnEnemy() {
    if (_.Enemies.GetChildren().Count >= 5) {
      return;
    }

    const float spawnRadius = 5.0f;
    const float minDistanceBetweenEnemies = 2.0f;
    const int maxSpawnAttempts = 10;

    var rng = new RandomNumberGenerator();
    rng.Randomize();

    Vector3 spawnPosition = FindValidSpawnPosition3D(rng, spawnRadius, minDistanceBetweenEnemies, maxSpawnAttempts);
    var enemy = CreateEnemy3D(spawnPosition);
    _.Enemies.AddChild(enemy);
    // var start = Stopwatch.GetTimestamp();
    // GD.Print("Created Enemy " + enemy.Prompt + " in " + Stopwatch.GetElapsedTime(start).TotalMilliseconds + " ms");
  }

  private Vector3 FindValidSpawnPosition3D(RandomNumberGenerator rng, float radius, float minDistance,
    int maxAttempts) {
    var existingEnemies = Enumerable.Cast<Enemy>(_.Enemies.GetChildren()).ToList();

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

  private Vector3 GenerateRandomPosition3D(RandomNumberGenerator rng, Vector3 center, float radius) {
    // Random point in spherical coordinates
    float azimuth = rng.RandfRange(0, Mathf.Tau);
    float polar = rng.RandfRange(0, Mathf.Pi);
    float distance = rng.RandfRange(0, radius);

    return new Vector3(
      center.X + distance * Mathf.Sin(polar) * Mathf.Cos(azimuth),
      center.Y,
      center.Z + distance * Mathf.Sin(polar) * Mathf.Sin(azimuth)
    );
  }

  private bool IsPositionOccupied3D(Vector3 position, List<Enemy> existingEnemies, float minDistance) {
    foreach (var enemy in existingEnemies) {
      if (enemy.Position.DistanceTo(position) < minDistance) {
        return true;
      }
    }

    return false;
  }

  private Enemy CreateEnemy3D(Vector3 position) {
    var enemy = Enemy.Instantiate(GetRandomWord(), PlayerPosition);
    enemy.Position = position;
    return enemy;
  }

  private string GetRandomWord() {
    return WordList[Random.Shared.Next(0, WordList.Count)];
  }

  public void _on_timer_timeout() {
    SpawnEnemy();
  }
  public void _on_active_enemy_deleted() {
    activeEnemy = null;
  }
}
