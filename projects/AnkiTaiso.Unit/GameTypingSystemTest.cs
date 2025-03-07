namespace anki_taiso.unit;

using ankitaiso.game_typing;
using FluentAssertions;
using Godot;
using Shouldly;
using Xunit;

public class GameTypingSystemTest {
  private static readonly IEnumerable<string> _englishWords = [
    "abc", "acd", "bce",
    "s ./,_-+()[]{}", "cde"
  ];
  private static readonly IEnumerable<string> _japaneseWords = [
    "カタカナ", "ひらがな"
  ];

  [Fact]
  public void EnglishClearOneEntryTest() {
    var game = new GameTypingSystem(_englishWords);
    var words = game.NextEntries(4);
    words.Count.Should().Be(4);
    game.GetEntriesInUse().Count.Should().Be(4);
    // make sure "acd" is skipped so 2 words don't start with 'a'
    words[0].Entry.Should().Be("abc");
    words[1].Entry.Should().Be("bce");
    words[3].Entry.Should().Be("cde");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.X).ShouldBeFalse();
    game.OnInput(Key.A).ShouldBeTrue();
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.ShouldBe("abc");
    game.OnInput(Key.B).ShouldBeTrue();
    game.OnInput(Key.C).ShouldBeTrue();
    game.GetActiveEntry().ShouldBeNull();
    game.GetEntriesInUse().Count.Should().Be(3);
  }

  [Fact]
  public void EnglishSpecialCharsTest() {
    var game = new GameTypingSystem(_englishWords);
    var words = game.NextEntries(_englishWords.Count());
    words.Count.ShouldBe(_englishWords.Count());
    game.OnInput(Key.S).ShouldBeTrue();
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.ShouldBe("s ./,_-+()[]{}");
    game.OnInput(Key.Space).ShouldBeTrue();
    game.OnInput(Key.Period).ShouldBeTrue();
    game.OnInput(Key.Slash).ShouldBeTrue();
    game.OnInput(Key.Comma).ShouldBeTrue();
    game.OnInput(Key.Underscore).ShouldBeTrue();
    game.OnInput(Key.Minus).ShouldBeTrue();
    game.OnInput(Key.Plus).ShouldBeTrue();
    game.OnInput(Key.Parenleft).ShouldBeTrue();
    game.OnInput(Key.Parenright).ShouldBeTrue();
    game.OnInput(Key.Bracketleft).ShouldBeTrue();
    game.OnInput(Key.Bracketright).ShouldBeTrue();
    game.OnInput(Key.Braceleft).ShouldBeTrue();
    game.OnInput(Key.Braceright).ShouldBeTrue();
    game.GetActiveEntry().ShouldBeNull();
  }

  [Fact]
  public void JapaneseKatakanaTest() {
    var game = new GameTypingSystem(_japaneseWords);
    var words = game.NextEntries(2);
    words.Count.Should().Be(2);
    game.OnInput(Key.K).ShouldBeTrue();
    game.Buffer.Should().Be("k");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.A).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.ShouldBe("カタカナ");
    game.GetActiveEntry()!.InputBuffer.ShouldBe("カ");
    game.OnInput(Key.T).ShouldBeTrue();
    game.Buffer.Should().Be("t");
    game.OnInput(Key.A).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry()!.InputBuffer.ShouldBe("カタ");
    game.OnInput(Key.K).ShouldBeTrue();
    game.Buffer.Should().Be("k");
    game.OnInput(Key.A).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry()!.InputBuffer.ShouldBe("カタカ");
    game.OnInput(Key.N).ShouldBeTrue();
    game.Buffer.Should().Be("n");
    game.OnInput(Key.A).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldBeNull();
  }

  [Fact]
  public void HiraganaTest() {
    var game = new GameTypingSystem(_japaneseWords);
    var words = game.NextEntries(2);
    words.Count.Should().Be(2);
    game.OnInput(Key.H).ShouldBeTrue();
    game.Buffer.Should().Be("h");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.I).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.ShouldBe("ひらがな");
    game.GetActiveEntry()!.InputBuffer.ShouldBe("ひ");
    game.OnInput(Key.R).ShouldBeTrue();
    game.Buffer.Should().Be("r");
    game.OnInput(Key.A).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry()!.InputBuffer.ShouldBe("ひら");
    game.OnInput(Key.G).ShouldBeTrue();
    game.Buffer.Should().Be("g");
    game.OnInput(Key.A).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry()!.InputBuffer.ShouldBe("ひらが");
    game.OnInput(Key.N).ShouldBeTrue();
    game.Buffer.Should().Be("n");
    game.OnInput(Key.A).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldBeNull();
  }
}
