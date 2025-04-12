namespace MyNotes.Core.Models;

public class Icon
{
  static Icon()
  {
    ConfigureSymbolPaths();
    ConfigureEmojiPaths("SmileysAndEmotion");
    ConfigureEmojiPaths("PeopleAndBodyGeneral");
    ConfigureEmojiPaths("PeopleAndBodyLight");
    ConfigureEmojiPaths("PeopleAndBodyMediumLight");
    ConfigureEmojiPaths("PeopleAndBodyMedium");
    ConfigureEmojiPaths("PeopleAndBodyMediumDark");
    ConfigureEmojiPaths("PeopleAndBodyDark");
    ConfigureEmojiPaths("AnimalsAndNature");
    ConfigureEmojiPaths("FoodAndDrink");
    ConfigureEmojiPaths("TravelAndPlaces");
    ConfigureEmojiPaths("ActivitiesAndObjects", "Activities");
    ConfigureEmojiPaths("ActivitiesAndObjects", "Objects");
    ConfigureEmojiPaths("SymbolsAndFlags", "Symbols");
    ConfigureEmojiPaths("SymbolsAndFlags", "Flags");
  }

  private static void ConfigureSymbolPaths()
  {
    foreach (Symbol symbol in Enum.GetValues<Symbol>())
      SymbolPaths.AddRange(symbol);
  }

  private static void ConfigureEmojiPaths(string pathKey, string? rangeKey = null)
  {
    string folderName = "ms-appx:///Assets/emojis/";
    rangeKey ??= pathKey;
    int start = EmojiRanges[rangeKey].Start.Value;
    int end = EmojiRanges[rangeKey].End.Value;
    for (int i = start; i <= end; i++)
      EmojiPaths[pathKey].Add(Path.Combine(folderName, $"{i}.png"));
  }

  public static List<Symbol> SymbolPaths { get; } = new();
  public static Dictionary<string, List<string>> EmojiPaths { get; } = new()
  {
    { "SmileysAndEmotion", new() },          // 0 ~ 167
    { "PeopleAndBodyGeneral", new() },       // 10000 ~ 10384
    { "PeopleAndBodyLight", new() },         // 11000 ~ 11426
    { "PeopleAndBodyMediumLight", new() },   // 12000 ~ 12426
    { "PeopleAndBodyMedium", new() },        // 13000 ~ 13426
    { "PeopleAndBodyMediumDark", new() },    // 14000 ~ 14426
    { "PeopleAndBodyDark", new() },          // 15000 ~ 15426
    { "AnimalsAndNature", new() },           // 2000 ~ 2152
    { "FoodAndDrink", new() },               // 3000 ~ 3134
    { "TravelAndPlaces", new() },            // 4000 ~ 4217
    { "ActivitiesAndObjects", new() },       // 5000 ~ 5084, 6000 ~ 6261
    { "SymbolsAndFlags", new() },            // 7000 ~ 7222, 8000 ~ 8007
  };
  public static Dictionary<string, Range> EmojiRanges { get; } = new()
  {
    { "SmileysAndEmotion", new(0, 167) },
    { "PeopleAndBodyGeneral", new(10000, 10384) },
    { "PeopleAndBodyLight", new(11000, 11426) },
    { "PeopleAndBodyMediumLight", new(12000, 12426) },
    { "PeopleAndBodyMedium", new(13000, 13426) },
    { "PeopleAndBodyMediumDark", new(14000, 14426) },
    { "PeopleAndBodyDark", new(15000, 15426) },
    { "AnimalsAndNature", new(2000, 2152) },
    { "FoodAndDrink", new(3000, 3134) },
    { "TravelAndPlaces", new(4000, 4217) },
    { "Activities", new(5000, 5084) },
    { "Objects", new(6000, 6261) },
    { "Symbols", new(7000, 7222) },
    { "Flags", new(8000, 8007) },
  };
}
