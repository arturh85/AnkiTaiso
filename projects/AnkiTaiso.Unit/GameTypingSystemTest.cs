namespace anki_taiso.unit;

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
  private static readonly IEnumerable<string> _koreanWords = [
    "살다"
  ];
  private static readonly IEnumerable<string> _chineseWords = [
    "昨天"
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

    var korean = "음";
    WanaKana.ToRomaji(korean).ShouldBe(korean);
    WanaKana.IsHiragana(korean).ShouldBeFalse();
    WanaKana.IsKatakana(korean).ShouldBeFalse();
    WanaKana.IsKana(korean).ShouldBeFalse();
    WanaKana.IsKanji(korean).ShouldBeFalse();
    WanaKana.IsRomaji(korean).ShouldBeFalse();
    WanaKana.IsJapanese(korean).ShouldBeFalse();
    WanaKana.IsMixed(korean).ShouldBeFalse();
  }

  [Fact]
  public void EnglishClearOneEntryTest() {
    var game = new GameTypingSystem(_englishWords.Select(w => new VocabEntry(w)));
    var words = game.NextEntries(4);
    words.Count.Should().Be(4);
    game.GetEntriesInUse().Count.Should().Be(4);
    // make sure "acd" is skipped so 2 words don't start with 'a'
    words[0].Entry.Prompt.Should().Be("abc");
    words[1].Entry.Prompt.Should().Be("bce");
    words[3].Entry.Prompt.Should().Be("cde");
    game.GetActiveEntry().ShouldBeNull();

    game.StatisticTotalError.ShouldBe(0);
    game.OnInput(Key.X).ShouldBeFalse();
    game.StatisticTotalError.ShouldBe(1);

    game.OnInput(Key.A).ShouldBeTrue();
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.Prompt.ShouldBe("abc");
    game.StatisticTotalError.ShouldBe(1);
    game.GetActiveEntry()!.ShowHint.ShouldBeFalse();
    game.OnInput(Key.Y).ShouldBeFalse();
    game.GetActiveEntry()!.ShowHint.ShouldBeFalse();
    game.StatisticByChar["b"].FailCount.ShouldBe(1);
    game.StatisticTotalError.ShouldBe(2);
    game.OnInput(Key.B).ShouldBeTrue();
    game.OnInput(Key.C).ShouldBeTrue();
    game.GetActiveEntry().ShouldBeNull();
    game.GetEntriesInUse().Count.Should().Be(3);
    game.StatisticTotalSuccess.ShouldBe(3);
    game.StatisticTotalError.ShouldBe(2);
  }

  [Fact]
  public void EnglishSpecialCharsTest() {
    var game = new GameTypingSystem(_englishWords.Select(w => new VocabEntry(w)));
    var words = game.NextEntries(_englishWords.Count());
    words.Count.ShouldBe(_englishWords.Count());
    game.OnInput(Key.S).ShouldBeTrue();
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.Prompt.ShouldBe("s ./,_-+()[]{}");
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
    game.StatisticTotalError.ShouldBe(0);
  }

  [Fact]
  public void JapaneseKatakanaTest() {
    var game = new GameTypingSystem(_japaneseWords.Select(w => new VocabEntry(w)));
    var words = game.NextEntries(2);
    words.Count.Should().Be(2);
    game.OnInput(Key.K).ShouldBeTrue();
    game.Buffer.Should().Be("k");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.A).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.Prompt.ShouldBe("カタカナ");
    game.GetActiveEntry()!.InputBuffer.ShouldBe("カ");
    game.GetActiveEntry()!.ShowHint.ShouldBeFalse();
    game.OnInput(Key.X).ShouldBeFalse();
    game.GetActiveEntry()!.ShowHint.ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.OnInput(Key.T).ShouldBeTrue();
    game.Buffer.Should().Be("t");
    game.GetActiveEntry()!.ShowHint.ShouldBeTrue();
    game.OnInput(Key.A).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry()!.ShowHint.ShouldBeFalse();
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
    game.StatisticTotalError.ShouldBe(1);
    game.StatisticByChar.ShouldContainKey("タ");
  }

  [Fact]
  public void ClearBufferWithoutActiveEntryOnMistakeTest() {
    var game = new GameTypingSystem([new VocabEntry("カタカナ")]);
    game.NextEntries(1);
    game.OnInput(Key.K).ShouldBeTrue();
    game.Buffer.Should().Be("k");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.X).ShouldBeFalse();
    game.Buffer.Should().Be("");
  }

  [Fact]
  public void HiraganaTest() {
    var game = new GameTypingSystem(_japaneseWords.Select(w => new VocabEntry(w)));
    var words = game.NextEntries(2);
    words.Count.Should().Be(2);
    game.OnInput(Key.H).ShouldBeTrue();
    game.Buffer.Should().Be("h");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.I).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.Prompt.ShouldBe("ひらがな");
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
    game.StatisticTotalError.ShouldBe(0);
  }

  [Fact]
  public void HiraganaTsuTest() {
    var game = new GameTypingSystem(_japaneseWords.Select(w => new VocabEntry(w)));
    var words = game.NextEntries(4);
    words.Count.Should().Be(4);
    game.OnInput(Key.M).ShouldBeTrue();
    game.Buffer.Should().Be("m");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.I).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.Prompt.ShouldBe("みっつ");
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
    game.StatisticTotalError.ShouldBe(0);
  }

  [Fact]
  public void KatakanaTsuTest() {
    var game = new GameTypingSystem(_japaneseWords.Select(w => new VocabEntry(w)));
    var words = game.NextEntries(4);
    words.Count.Should().Be(4);
    game.OnInput(Key.R).ShouldBeTrue();
    game.Buffer.Should().Be("r");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.O).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.Prompt.ShouldBe("ロボット");
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
    game.StatisticTotalError.ShouldBe(0);
  }
  [Fact]
  public void JapanesePyuAndDashTest() {
    var game = new GameTypingSystem(_japaneseWords.Select(w => new VocabEntry(w)));
    var words = game.NextEntries(5);
    words.Count.Should().Be(5);
    game.OnInput(Key.K).ShouldBeTrue();
    game.Buffer.Should().Be("k");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.O).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.Prompt.ShouldBe("コンピュータ");
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
    game.StatisticTotalError.ShouldBe(0);
  }
  [Fact]
  public void KoreanTest() {
    var game = new GameTypingSystem(_koreanWords.Select(w => new VocabEntry(w)));
    var words = game.NextEntries(1);
    words.Count.Should().Be(1);
    game.OnInput(Key.S).ShouldBeTrue();
    game.Buffer.Should().Be("s");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.A).ShouldBeTrue();
    game.Buffer.Should().Be("sa");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.L).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.Prompt.ShouldBe("살다");
    game.GetActiveEntry()!.InputBuffer.ShouldBe("살");
    game.GetActiveEntry()!.NextVariants.Should().NotBeNull();
    game.GetActiveEntry()!.NextVariants.Should().NotBeEmpty();
    game.GetActiveEntry()!.NextVariants![0].ShouldBe("da");
    game.OnInput(Key.D).ShouldBeTrue();
    game.Buffer.Should().Be("d");
    game.OnInput(Key.A).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldBeNull();
    game.StatisticTotalError.ShouldBe(0);
  }
  [Fact]
  public void ChineseTest() {
    var game = new GameTypingSystem(_chineseWords.Select(w => new VocabEntry(w)));
    var words = game.NextEntries(1);
    words.Count.Should().Be(1);
    words[0].NextVariants.Should().NotBeNull();
    words[0].NextVariants.Should().NotBeEmpty();
    words[0].NextVariants![0].Should().Be("zuo");
    game.OnInput(Key.Z).ShouldBeTrue();
    game.Buffer.Should().Be("z");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.U).ShouldBeTrue();
    game.Buffer.Should().Be("zu");
    game.GetActiveEntry().ShouldBeNull();
    game.OnInput(Key.O).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldNotBeNull();
    game.GetActiveEntry()!.Entry.Prompt.ShouldBe("昨天");
    game.GetActiveEntry()!.InputBuffer.ShouldBe("昨");
    game.GetActiveEntry()!.NextVariants.Should().NotBeNull();
    game.GetActiveEntry()!.NextVariants.Should().NotBeEmpty();
    game.GetActiveEntry()!.NextVariants![0].ShouldBe("tian");
    game.OnInput(Key.T).ShouldBeTrue();
    game.Buffer.Should().Be("t");
    game.OnInput(Key.I).ShouldBeTrue();
    game.Buffer.Should().Be("ti");
    game.OnInput(Key.A).ShouldBeTrue();
    game.Buffer.Should().Be("tia");
    game.OnInput(Key.N).ShouldBeTrue();
    game.Buffer.Should().Be("");
    game.GetActiveEntry().ShouldBeNull();
    game.StatisticTotalError.ShouldBe(0);
  }
}
