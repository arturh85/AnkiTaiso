namespace ankitaiso.utils;

using System;
using System.Runtime.Serialization;

public class GameException : ApplicationException {
  public GameException() {
  }

  [Obsolete("Obsolete")]
  protected GameException(SerializationInfo info, StreamingContext context) : base(info, context) {
  }

  public GameException(string? message) : base(message) {
  }

  public GameException(string? message, Exception? innerException) : base(message, innerException) {
  }
}
