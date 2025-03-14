namespace ankitaiso.utils;

using System;
using System.Collections.Generic;
using System.Linq;

public class CollectionUtils {
  public static T? RandomEntry<T>(ICollection<T> collection) {
    var idx = Random.Shared.Next(0, collection.Count);
    var child = collection.ElementAt(idx);;
    return child;
  }
}
