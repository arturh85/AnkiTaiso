namespace ankitaiso.map;

using app.domain;
using Chickensoft.AutoInject;
using Chickensoft.GodotNodeInterfaces;
using Chickensoft.Introspection;
using game.domain;
using Godot;
using state;
using MapLogic = state.MapLogic;

public interface IMap : INode3D {
  /// <summary>Get the number of coins in the world.</summary>
  int GetCoinCount();
}

[Meta(typeof(IAutoNode))]
public partial class Map : Node3D, IMap {
  public override void _Notification(int what) => this.Notify(what);

  [Dependency] public IAppRepo AppRepo => this.DependOn<IAppRepo>();

  #region Nodes

  // [Node] public INode3D Coins { get; set; } = default!;

  #endregion Nodes

  #region State

  [Dependency] public IGameRepo GameRepo => DependentExtensions.DependOn<IGameRepo>(this);
  public IMapLogic MapLogic { get; set; } = default!;

  #endregion State

  public int GetCoinCount() => 0;

  public void Setup() => MapLogic = new MapLogic();

  public void OnResolved() {
    MapLogic.Set(new MapLogic.Data());
    MapLogic.Set(GameRepo);

    MapLogic.Start();

    this.Provide();
  }
}
