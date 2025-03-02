using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using kyoukaitansa.game_typing.domain;
using kyoukaitansa.game_typing.state;
using kyoukaitansa.game.state;
using Enemy = kyoukaitansa.enemy.Enemy;

[SceneTree]
public partial class GameTyping : Node3D {
  #region State

  public IGameTypingRepo GameTypingRepo { get; set; } = default!;
  public IGameTypingLogic GameTypingLogic { get; set; } = default!;

  public GameTypingLogic.IBinding GameTypingBinding { get; set; } = default!;

  #endregion State

  public Vector3 PlayerPosition;
  public Vector3 SpawnPosition;

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
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) {
  }

  public bool paused = true;
  public bool game_started = false;


  public void StartGame() {
    game_started = true;
    SetPaused(false);
    _.Enemy.DisableA();
  }

  public void SetPaused(bool _paused) {
    if (!game_started) {
      return;
    }

    GD.Print("SetPaused(" + _paused);
    paused = _paused;
    if (_paused) {
      GD.Print("Stopping Timer");
      _.Timer.Stop();
    }
    else {
      GD.Print("Starting Timer");
      _.Timer.Start();
    }
  }


  public List<string> WordList = new List<string>();

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


  public void SpawnStuff() {
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

    GD.Print("Spawned Enemy " + enemy.GetText());
  }

  private Vector3 FindValidSpawnPosition3D(RandomNumberGenerator rng, float radius, float minDistance,
    int maxAttempts) {
    var existingEnemies = _.Enemies.GetChildren().Cast<Enemy>().ToList();

    for (int i = 0; i < maxAttempts; i++) {
      Vector3 candidatePosition = GenerateRandomPosition3D(rng, SpawnPosition, radius);

      if (!IsPositionOccupied3D(candidatePosition, existingEnemies, minDistance)) {
        return candidatePosition;
      }
    }

    // Fallback to random position without collision check
    return GenerateRandomPosition3D(rng, SpawnPosition, radius);
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
    if (_.Enemy == null) {
      GD.Print("Enemy null?");
    }

    var enemy = _.Enemy.Duplicate() as Enemy;
    enemy.Position = position;
    enemy.MovementTarget = PlayerPosition;

    // Orient enemy to face player
    enemy.LookAt(PlayerPosition, Vector3.Up);

    enemy.SetText(GetRandomWord());
    enemy.EnableA();
    return enemy;
  }

  private string GetRandomWord() {
    return WordList[Random.Shared.Next(0, WordList.Count)];
  }

  public void _on_timer_timeout() {
    SpawnStuff();
  }
}
