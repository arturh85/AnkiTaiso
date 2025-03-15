namespace ankitaiso.utils;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class HangeulRomaniser : IHangeulRomaniser {
  #region Constants

  private const ushort FIRST_HANGEUL_UNICODE = 0xAC00;
  private const ushort LAST_HANGEUL_UNICODE = 0xD79F;

  #endregion Constants

  #region Properties

  private IList<KeyValuePair<char, string>> _initials = new List<KeyValuePair<char, string>>() {
    new('ㄱ', "g"),
    new('ㄲ', "kk"),
    new('ㄴ', "n"),
    new('ㄷ', "d"),
    new('ㄸ', "tt"),
    new('ㄹ', "r"),
    new('ㅁ', "m"),
    new('ㅂ', "b"),
    new('ㅃ', "pp"),
    new('ㅅ', "s"),
    new('ㅆ', "ss"),
    new('ㅇ', ""),
    new('ㅈ', "j"),
    new('ㅉ', "jj"),
    new('ㅊ', "ch"),
    new('ㅋ', "k"),
    new('ㅌ', "t"),
    new('ㅍ', "p"),
    new('ㅎ', "h")
  };

  private IList<KeyValuePair<char, string>> _medials = new List<KeyValuePair<char, string>>() {
    new('ㅏ', "a"),
    new('ㅐ', "ae"),
    new('ㅑ', "ya"),
    new('ㅒ', "yae"),
    new('ㅓ', "eo"),
    new('ㅔ', "e"),
    new('ㅕ', "yeo"),
    new('ㅖ', "ye"),
    new('ㅗ', "o"),
    new('ㅘ', "wa"),
    new('ㅙ', "wae"),
    new('ㅚ', "oe"),
    new('ㅛ', "yo"),
    new('ㅜ', "u"),
    new('ㅝ', "wo"),
    new('ㅞ', "we"),
    new('ㅟ', "wi"),
    new('ㅠ', "yu"),
    new('ㅡ', "eu"),
    new('ㅢ', "ui"),
    new('ㅣ', "i")
  };

  private IList<KeyValuePair<char, string>> _finals = new List<KeyValuePair<char, string>>() {
    new(' ', ""),
    new('ㄱ', "k"),
    new('ㄲ', "k"),
    new('ㄳ', "k"),
    new('ㄴ', "n"),
    new('ㄵ', "n"),
    new('ㄶ', "n"),
    new('ㄷ', "t"),
    new('ㄹ', "l"),
    new('ㄺ', "k"),
    new('ㄻ', "m"),
    new('ㄼ', "l"),
    new('ㄽ', "l"),
    new('ㄾ', "l"),
    new('ㄿ', "p"),
    new('ㅀ', "l"),
    new('ㅁ', "m"),
    new('ㅂ', "p"),
    new('ㅄ', "p"),
    new('ㅅ', "t"),
    new('ㅆ', "t"),
    new('ㅇ', "ng"),
    new('ㅈ', "t"),
    new('ㅊ', "t"),
    new('ㅋ', "k"),
    new('ㅌ', "t"),
    new('ㅍ', "p"),
    new('ㅎ', "t")
  };


  /// <summary>
  /// Gets the list of initials and their romanised as key/value pair.
  /// </summary>
  public IList<KeyValuePair<char, string>> Initials => _initials;

  /// <summary>
  /// Gets the list of medials and their romanised as key/value pair.
  /// </summary>
  public IList<KeyValuePair<char, string>> Medials => _medials;

  /// <summary>
  /// Gets the list of finals and their romanised as key/value pair.
  /// </summary>
  public IList<KeyValuePair<char, string>> Finals => _finals;

  #endregion Properties

  #region Methods

  /// <summary>
  /// Combines each characters to make a letter.
  /// </summary>
  /// <param name="initial">Initial character.</param>
  /// <param name="medial">Medial character.</param>
  /// <param name="final">Final character.</param>
  /// <returns>Returns a letter combined with initial, medial and final.</returns>
  public string Combine(string initial, string medial, string final = "") {
    var indexInitial = this.Initials
      .Select(p => p.Key)
      .ToList()
      .IndexOf(Convert.ToChar(initial));
    var indexMedial = this.Medials
      .Select(p => p.Key)
      .ToList()
      .IndexOf(Convert.ToChar(medial));
    var indexFinal = string.IsNullOrWhiteSpace(final)
      ? 0
      : this.Finals
        .Select(p => p.Key)
        .ToList()
        .IndexOf(Convert.ToChar(final));

    var unicode = FIRST_HANGEUL_UNICODE + (((indexInitial * 21) + indexMedial) * 28) + indexFinal;

    var result = Convert.ToString(Convert.ToChar(unicode));
    return result;
  }

  /// <summary>
  /// Splits the letter into initial, medial and final.
  /// </summary>
  /// <param name="letter">Letter to split.</param>
  /// <returns>Returns the initial, medial and final.</returns>
  public IList<string>? Split(char letter) {
    var indices = this.GetIndices(letter);
    if (indices == null || !indices.Any()) {
      return null;
    }

    var resultInitial = Convert.ToString(this.Initials[indices[0]].Key);
    var resultMedial = Convert.ToString(this.Medials[indices[1]].Key);
    var resultFinal = Convert.ToString(this.Finals[indices[2]].Key).Trim();

    var result = new List<string>() { resultInitial, resultMedial, resultFinal };
    return result;
  }

  /// <summary>
  /// Romanises the letter.
  /// </summary>
  /// <param name="letters">Letters to romanise.</param>
  /// <param name="delimiter">Delimiter for letter.</param>
  /// <returns>Returns the letter romanised.</returns>
  public string Romanise(string letters, string delimiter = " ") {
    var results = letters.ToCharArray()
      .Select(p => this.Romanise(p))
      .Where(p => !string.IsNullOrWhiteSpace(p))
      .ToList();
    return string.Join(delimiter, results);
  }

  /// <summary>
  /// Romanises the letter.
  /// </summary>
  /// <param name="letter">Letter to romanise.</param>
  /// <returns>Returns the letter romanised.</returns>
  public string? Romanise(char letter) {
    var indices = this.GetIndices(letter);
    if (indices == null || !indices.Any()) {
      return null;
    }

    var resultInitial = Convert.ToString(this.Initials[indices[0]].Value);
    var resultMedial = Convert.ToString(this.Medials[indices[1]].Value);
    var resultFinal = Convert.ToString(this.Finals[indices[2]].Value).Trim();

    var result = string.Join("", resultInitial, resultMedial, resultFinal);
    return result;
  }

  /// <summary>
  /// Gets the indices for initial, medial and final.
  /// </summary>
  /// <param name="letter">Letter to get indices.</param>
  /// <returns>Returns the indices.</returns>
  private IList<int>? GetIndices(char letter) {
    var unicode = Convert.ToUInt16(letter);
    //  Checks whether the given letter belongs to Hangeul unicode range.
    if (unicode < FIRST_HANGEUL_UNICODE || unicode > LAST_HANGEUL_UNICODE) {
      //  Checks the given letter only contains initial.
      //  If so, it appends medial of "ㅡ" and return the result.
      if (this.Initials.Select(p => p.Key).Contains(Convert.ToChar(letter))) {
        var initial = Convert.ToString(this.Initials.Single(p => p.Key == Convert.ToChar(letter)).Key);
        letter = Convert.ToChar(this.Combine(initial, "ㅡ", string.Empty));
        return this.GetIndices(letter);
      }

      //  Checks the given letter only contains initial.
      //  If so, it prepends initial of "ㅇ" and return the result.
      if (this.Medials.Select(p => p.Key).Contains(Convert.ToChar(letter))) {
        var medial = Convert.ToString(this.Medials.Single(p => p.Key == Convert.ToChar(letter)).Key);
        letter = Convert.ToChar(this.Combine("ㅇ", medial, string.Empty));
        return this.GetIndices(letter);
      }

      //  Checks the given letter only contains initial.
      //  If so, it prepends initial of "ㅇ" and medial of "ㅡ" and return the result.
      if (this.Finals.Select(p => p.Key).Contains(Convert.ToChar(letter))) {
        var final = Convert.ToString(this.Finals.Single(p => p.Key == Convert.ToChar(letter)).Key);
        letter = Convert.ToChar(this.Combine("ㅇ", "ㅡ", final));
        return this.GetIndices(letter);
      }

      //  Returns NULL, if any of above is applied.
      return null;
    }

    var indexUnicode = unicode - FIRST_HANGEUL_UNICODE;
    var indexInitial = indexUnicode / (21 * 28);
    indexUnicode = indexUnicode % (21 * 28);
    var indexMedial = indexUnicode / 28;
    indexUnicode = indexUnicode % 28;
    var indexFinal = indexUnicode;

    var result = new List<int>() { indexInitial, indexMedial, indexFinal };
    return result;
  }

  /// <summary>
  /// Romanises list of letters in bulk.
  /// </summary>
  /// <param name="list">List of letters.</param>
  /// <param name="delimiter">Delimiter for letter.</param>
  /// <returns>Returns the list of letters romanised.</returns>
  public IList<string> RomaniseInBulk(IList<string> list, string delimiter = " ") {
    var results = list.Select(p => $"{p}\t{this.Romanise(p, delimiter)}")
      .ToList();
    return results;
  }

  /// <summary>
  /// Reads a text file that contains the list of Hangeul letters.
  /// </summary>
  /// <param name="filepath">Full file path.</param>
  /// <returns>Returns the list of letters.</returns>
  public IList<string> ReadFile(string filepath) {
    var results = File.ReadAllLines(filepath).ToList();
    return results;
  }

  /// <summary>
  /// Saves the output to a given file path.
  /// </summary>
  /// <param name="filepath">Full file path.</param>
  /// <param name="results">List of outputs.</param>
  public void SaveFile(string filepath, IList<string> results) {
    using var stream = new FileStream(filepath, FileMode.Create, FileAccess.Write);
    using var writer = new StreamWriter(stream, Encoding.Unicode);
    foreach (var result in results) {
      writer.WriteLine(result);
    }
  }

  #endregion Methods
}
