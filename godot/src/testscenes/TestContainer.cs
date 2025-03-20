using ankitaiso.game_typing;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Godot;

namespace ankitaiso.testscenes;

[Meta(typeof(IAutoNode))]
public partial class TestContainer : Control, IProvide<GameTypingSystem>
{
  GameTypingSystem IProvide<GameTypingSystem>.Value() => GameTypingSystem;
  public GameTypingSystem GameTypingSystem { get; set; } = default!;

  public void Initialize() {
    GameTypingSystem = new GameTypingSystem();
    this.Provide();
  }
}
