namespace ankitaiso.utils;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class NodeUtils {
  public static T? RandomChild<T>(Node node) where T : Node {
    var children = node.GetChildren();
    var idx = Random.Shared.Next(0, children.Count);
    var child = children[idx];
    return child as T;
  }
  public static T? RandomChildWhere<T>(Node node, Func<T, bool>? filter) where T : Node {
    var children = node.GetChildren();
    for (var i = 0; i < 10; i++) {
      var idx = Random.Shared.Next(0, children.Count);
      var child = children[idx];
      if (filter != null && child != null && !filter((child as T)!)) {
        continue;
      }
      return child as T;
    }
    return null;
  }

  public static List<T> NearestNodes<T>(Vector3 position, Node3D parent, int count, Func<T, bool>? filter) where T : Node3D {
    var enemiesWithDistance = parent
      .GetChildren()
      .OfType<T>()
      .Where(node3D => filter != null ? filter(node3D) : true)
      .Select(node3D => new {
        Node = node3D,
        // Calculate squared distances to avoid expensive square root operations
        SqrDistance = position.DistanceSquaredTo(node3D.Position)
      })
      .OrderBy(e => e.SqrDistance)
      .Take(count)
      .Select(e => e.Node)
      .ToList();

    return enemiesWithDistance;
  }

  public static Vector3 RandomPositionInArc(RandomNumberGenerator rng, Vector3 target, Vector3 center, float arcAngleRadians) {
    // Calculate direction from player to the center point
    var toCenter = center - target;
    // Get the angle of this direction in the XZ plane (azimuth)
    var thetaCenter = Mathf.Atan2(toCenter.Z, toCenter.X);
    // Random angle within the arc around the center angle
    var randomAngle = rng.RandfRange(thetaCenter - (arcAngleRadians / 2), thetaCenter + (arcAngleRadians / 2));
    // Random distance from the player (0 to radius)
    var distance = toCenter.Length();

    // Calculate position in XZ plane around the player's position
    var x = target.X + (distance * Mathf.Cos(randomAngle));
    var z = target.Z + (distance * Mathf.Sin(randomAngle));

    // Keep Y coordinate same as the center's Y (or set to PlayerPosition.Y if needed)
    return new Vector3(x, center.Y, z);
  }

  public static void ClearOptions(OptionButton button) {
    while (button.ItemCount > 0) {
      button.RemoveItem(0);
    }
  }
}
