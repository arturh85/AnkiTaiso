namespace anki_taiso.unit;

using System.Drawing.Printing;
using ankitaiso.game_typing;
using FluentAssertions;
using Godot;
using Shouldly;
using WanaKanaNet;
using Xunit;

public class GameTypingSystemTest {
  private static readonly IEnumerable<string> _englishWords = [
    "abc", "acd", "bce",
    "s ./,_-+()[]{}", "cde"
  ];
  private static readonly IEnumerable<string> _japaneseWords = [
    "カタカナ", "ひらがな", "みっつ", "ロボット", "コンピュータ", "きゅうじつ"
  ];



  [Fact]
  public void WanaKanaAssumptionsTest() {
    WanaKana.ToRomaji("コンピュータ").ShouldBe("konpyuuta");
    WanaKana.ToRomaji("ピュ").ShouldBe("pyu");
    WanaKana.ToRomaji("ピ").ShouldBe("pi");

    WanaKana.ToRomaji("ふ").ShouldBe("fu");
    WanaKana.ToRomaji("じ").ShouldBe("ji");
    WanaKana.ToRomaji("ぢ").ShouldBe("ji");
    WanaKana.ToRomaji("づ").ShouldBe("zu");
    WanaKana.ToRomaji("を").ShouldBe("wo");
  }

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

    game.ErrorCount.ShouldBe(0);
    game.OnInput(Key.X).ShouldBeFalse();
    game.ErrorCount.ShouldBe(1);

    game.OnInput(Key.A).ShouldBeTrue();
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.ShouldBe("abc");
    game.ErrorCount.ShouldBe(1);
    game.OnInput(Key.Y).ShouldBeFalse();
    game.StatisticByChar['y'].FailCount.ShouldBe(1);
    game.ErrorCount.ShouldBe(2);
    game.OnInput(Key.B).ShouldBeTrue();
    game.OnInput(Key.C).ShouldBeTrue();
    game.GetActiveEntry().ShouldBeNull();
    game.GetEntriesInUse().Count.Should().Be(3);
    game.ErrorCount.ShouldBe(2);
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
    game.ErrorCount.ShouldBe(0);
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
    game.ErrorCount.ShouldBe(0);
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
    game.ErrorCount.ShouldBe(0);
  }

  [Fact]
  public void HiraganaTsuTest() {
    var game = new GameTypingSystem(_japaneseWords);
    var words = game.NextEntries(4);
    words.Count.Should().Be(4);
    game.OnInput(Key.M).ShouldBeTrue();
    game.Buffer.Should().Be("m");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.I).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.ShouldBe("みっつ");
    game.GetActiveEntry()!.InputBuffer.ShouldBe("み");
    game.OnInput(Key.T).ShouldBeTrue();
    game.Buffer.Should().Be("t");
    game.OnInput(Key.T).ShouldBeTrue();
    game.Buffer.Should().Be("tt");
    game.OnInput(Key.S).ShouldBeTrue();
    game.Buffer.Should().Be("tts");
    game.OnInput(Key.U).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldBeNull();
    game.ErrorCount.ShouldBe(0);
  }

  [Fact]
  public void KatakanaTsuTest() {
    var game = new GameTypingSystem(_japaneseWords);
    var words = game.NextEntries(4);
    words.Count.Should().Be(4);
    game.OnInput(Key.R).ShouldBeTrue();
    game.Buffer.Should().Be("r");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.O).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.ShouldBe("ロボット");
    game.GetActiveEntry()!.InputBuffer.ShouldBe("ロ");
    game.OnInput(Key.B).ShouldBeTrue();
    game.Buffer.Should().Be("b");
    game.OnInput(Key.O).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry()!.InputBuffer.ShouldBe("ロボ");
    game.OnInput(Key.T).ShouldBeTrue();
    game.Buffer.Should().Be("t");
    game.OnInput(Key.T).ShouldBeTrue();
    game.Buffer.Should().Be("tt");
    game.OnInput(Key.O).ShouldBeTrue();
    game.GetActiveEntry().ShouldBeNull();
    game.ErrorCount.ShouldBe(0);
  }
  [Fact]
  public void JapanesePyuAndDashTest() {
    var game = new GameTypingSystem(_japaneseWords);
    var words = game.NextEntries(5);
    words.Count.Should().Be(5);
    game.OnInput(Key.K).ShouldBeTrue();
    game.Buffer.Should().Be("k");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.O).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.ShouldBe("コンピュータ");
    game.GetActiveEntry()!.InputBuffer.ShouldBe("コ");
    game.OnInput(Key.N).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry()!.InputBuffer.ShouldBe("コン");
    game.OnInput(Key.P).ShouldBeTrue();
    game.Buffer.Should().Be("p");
    game.OnInput(Key.Y).ShouldBeTrue();
    game.Buffer.Should().Be("py");
    game.OnInput(Key.U).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry()!.InputBuffer.ShouldBe("コンピュ");
    game.OnInput(Key.Minus).ShouldBeTrue();
    game.GetActiveEntry()!.InputBuffer.ShouldBe("コンピュー");
    game.OnInput(Key.T).ShouldBeTrue();
    game.Buffer.Should().Be("t");
    game.OnInput(Key.A).ShouldBeTrue();
    game.GetActiveEntry().ShouldBeNull();
    game.ErrorCount.ShouldBe(0);
  }
}
