using Godot;

namespace ankitaiso.components.health;

[Icon("res://src/components/health_component.svg")]
public partial class HealthComponent : BaseComponent {
  private CharacterBody3D _parent = null!;


  [Export] public int MaxHealth = 100;

  [Export] public int InitialHealth = 100;

  private int Health;

  public override void _Ready() {
    _parent = GetParent<CharacterBody3D>();
    Health = InitialHealth;
  }

  public void TakeDamage(int damage) {
    Health -= damage;
  }
}
