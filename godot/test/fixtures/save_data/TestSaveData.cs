namespace kyoukaitansa.test.fixtures.save_data;

using game;

public static class TestSaveData {
  // public static MapData MapData { get; }
  // public static PlayerData PlayerData { get; }
  // public static PlayerCameraData PlayerCameraData { get; }
  public static GameData GameData { get; }

  static TestSaveData() {
    // MapData = new MapData() {
    //   CoinsBeingCollected = new(),
    //   CollectedCoinIds = new()
    // };
    //
    // PlayerData = new PlayerData() {
    //   GlobalTransform = Transform3D.Identity,
    //   StateMachine = default!,
    //   Velocity = Vector3.Zero
    // };
    //
    // PlayerCameraData = new PlayerCameraData() {
    //   GlobalTransform = Transform3D.Identity,
    //   StateMachine = default!,
    //   LocalPosition = Vector3.Zero,
    //   OffsetPosition = Vector3.Zero
    // };

    GameData = new GameData() {
      // MapData = default!,
      // PlayerData = default!,
      // PlayerCameraData = default!
    };
  }
}
