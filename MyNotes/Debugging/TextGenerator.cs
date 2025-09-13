using System.Globalization;
using System.Text;

using Windows.ApplicationModel;

namespace MyNotes.Debugging;

internal class TextGenerator
{
  public static readonly List<string> WordsEn = new();
  public static readonly List<string> WordsKr = new();

  static TextGenerator()
  {
    InitializeWordsList("words_en_10000.txt", WordsEn);
    InitializeWordsList("words_kr_2031.txt", WordsKr);
  }

  static void InitializeWordsList(string fileName, List<string> words)
  {
    using var sr = new StreamReader(Path.Combine(Package.Current.InstalledLocation.Path, "Assets", "text", fileName));
    string? text;
    while ((text = sr.ReadLine()) is not null)
      words.Add(text);
  }

  public static string GenerateTexts(int num, string language)
  {
    Random random = new();

    (List<string> Words, TextInfo textInfo) = GetWordsForLanguage(language);

    int WordsMaxIndex = Words.Count;
    StringBuilder sb = new();

    for (int i = 0; i < num; i++)
    {
      string word = Words[random.Next(WordsMaxIndex)];
      if (sb.Length == 0 || sb[^1] == '\n')
        sb.Append(textInfo.ToTitleCase(word));
      else
        sb.Append(word);

      switch (random.NextDouble())
      {
        case < 0.01:
          sb.Append('.').AppendLine();
          break;
        case >= 0.01 and < 0.11:
          sb.Append(", ");
          break;
        default:
          sb.Append(' ');
          break;
      }
    }

    return sb.ToString();
  }

  public static string GenerateTitle(string language)
  {
    Random random = new();

    (List<string> Words, TextInfo textInfo) = GetWordsForLanguage(language);


    int WordsMaxIndex = Words.Count;
    StringBuilder sb = new();

    int num = random.Next(2, 8);
    for (int i = 0; i < num; i++)
    {
      string word = Words[random.Next(WordsMaxIndex)];
      if (i == 0)
        sb.Append(textInfo.ToTitleCase(word));
      else
        sb.Append($" {word}");
    }

    return sb.ToString();
  }

  private static (List<string> Words, TextInfo textInfo) GetWordsForLanguage(string language) => language.ToLower() switch
  {
    "en" or "eng" or "english" => (WordsEn, new CultureInfo("en-US", false).TextInfo),
    "kr" or "kor" or "korean" => (WordsKr, new CultureInfo("ko-KR", false).TextInfo),
    _ => (WordsEn, new CultureInfo("en-US", false).TextInfo)
  };
}