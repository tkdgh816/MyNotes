using System.Globalization;
using System.Text;

using Windows.ApplicationModel;

namespace MyNotes.Debugging;

internal class TextGenerator
{
  public static readonly List<string> Words = new();

  static TextGenerator()
  {
    using var sr = new StreamReader(Path.Combine(Package.Current.InstalledLocation.Path, @"Assets\text\words_en_10000.txt"));
    string? text;
    while ((text = sr.ReadLine()) is not null)
      Words.Add(text);
  }

  public static string GenerateTexts(int num)
  {
    Random random = new();
    int WordsMaxIndex = Words.Count;
    StringBuilder sb = new();
    var ti = new CultureInfo("en-US", false).TextInfo;

    
    for (int i = 0; i < num; i++)
    {
      string word = Words[random.Next(WordsMaxIndex)];
      if (sb.Length == 0 || sb[^1] == '\n')
        sb.Append(ti.ToTitleCase(word));
      else
        sb.Append(word);

      switch (random.Next(50) % 50)
      {
        case 5:
        case 15:
        case 25:
        case 35:
          sb.Append(", ");
          break;
        case 7:
          sb.Append('.');
          sb.AppendLine();
          break;
        default:
          sb.Append(' ');
          break;
      }
    }

    return sb.ToString();
  }

  public static string GenerateTitle()
  {
    Random random = new();
    int WordsMaxIndex = Words.Count;
    StringBuilder sb = new();
    var ti = new CultureInfo("en-US", false).TextInfo;

    int num = random.Next(2, 8);
    for (int i = 0; i < num; i++)
    {
      string word = Words[random.Next(WordsMaxIndex)];
      if (i == 0)
        sb.Append(ti.ToTitleCase(word));
      else
        sb.Append($" {word}");
    }

    return sb.ToString();
  }
}