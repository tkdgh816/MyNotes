namespace MyNotes.Core.Views;

public sealed partial class IconPicker : UserControl
{
  public IconPicker()
  {
    this.InitializeComponent();
    foreach (Symbol symbol in Enum.GetValues<Symbol>())
      SymbolPaths.AddRange(symbol);
    for (int i = 0; i <= 167; i++)
      EmojiPaths["SmileysAndEmotion"].Add(Path.Combine(FolderName, $"{i++}.png"));
    for (int i = 10000; i <= 10384; i++)
      EmojiPaths["PeopleAndBodyGeneral"].Add(Path.Combine(FolderName, $"{i++}.png"));
    for (int i = 11000; i <= 11426; i++)
      EmojiPaths["PeopleAndBodyLight"].Add(Path.Combine(FolderName, $"{i++}.png"));
    for (int i = 12000; i <= 12426; i++) 
      EmojiPaths["PeopleAndBodyMediumLight"].Add(Path.Combine(FolderName, $"{i++}.png"));
    for (int i = 13000; i <= 13426; i++)
      EmojiPaths["PeopleAndBodyMedium"].Add(Path.Combine(FolderName, $"{i++}.png"));
    for (int i = 14000; i <= 14426; i++)
      EmojiPaths["PeopleAndBodyMediumDark"].Add(Path.Combine(FolderName, $"{i++}.png"));
    for (int i = 15000; i <= 15426; i++)
      EmojiPaths["PeopleAndBodyDark"].Add(Path.Combine(FolderName, $"{i++}.png"));
    for (int i = 2000; i <= 2152; i++)
      EmojiPaths["AnimalsAndNature"].Add(Path.Combine(FolderName, $"{i++}.png"));
    for (int i = 3000; i <= 3134; i++)
      EmojiPaths["FoodAndDrink"].Add(Path.Combine(FolderName, $"{i++}.png"));
    for (int i = 4000; i <= 4217; i++)
      EmojiPaths["TravelAndPlaces"].Add(Path.Combine(FolderName, $"{i++}.png"));
    for (int i = 5000; i <= 5084; i++)
      EmojiPaths["ActivitiesAndObjects"].Add(Path.Combine(FolderName, $"{i++}.png"));
    for (int i = 6000; i <= 6261; i++)
      EmojiPaths["ActivitiesAndObjects"].Add(Path.Combine(FolderName, $"{i++}.png"));
    for (int i = 7000; i <= 7222; i++)
      EmojiPaths["SymbolsAndFlags"].Add(Path.Combine(FolderName, $"{i++}.png"));
    for (int i = 8000; i <= 8007; i++)
      EmojiPaths["SymbolsAndFlags"].Add(Path.Combine(FolderName, $"{i++}.png"));
  }

  readonly string FolderName = "ms-appx:///Assets/emojis/";
  public List<Symbol> SymbolPaths { get; } = new();
  public Dictionary<string, List<string>> EmojiPaths { get; } = new()
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

  public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(IconSource), typeof(IconPicker), new PropertyMetadata(null));
  public IconSource Icon
  {
    get => (IconSource)GetValue(IconProperty);
    set => SetValue(IconProperty, value);
  }

  private void SymbolButton_Click(object sender, RoutedEventArgs e)
  {
    Icon = new SymbolIconSource() { Symbol = (Symbol)((FrameworkElement)sender).DataContext };
  }

  private void EmojiButton_Click(object sender, RoutedEventArgs e)
  {
    Icon = new BitmapIconSource() { UriSource = new Uri((string)((FrameworkElement)sender).DataContext), ShowAsMonochrome = false };
  }

  private void VIEW_PrimarySelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
  {
    string selectedTag = (string)sender.SelectedItem.Tag;
    switch (selectedTag)
    {
      case "Symbol":
        VIEW_IconItemsRepeater.ItemsSource = SymbolPaths;
        break;
      case "PeopleAndBody":
        break;
      default:
        EmojiPaths.TryGetValue(selectedTag, out var items);
        VIEW_IconItemsRepeater.ItemsSource = items;
        break;
    }
  }

  private void VIEW_SecondarySelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
  {

  }
}

public class IconPickerViewItemTemplateSelector : DataTemplateSelector
{
  public DataTemplate? SymbolItemTemplate { get; set; }
  public DataTemplate? EmojiItemTemplate { get; set; }

  protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container)
  {
    return item switch
    {
      Symbol => SymbolItemTemplate,
      string => EmojiItemTemplate,
      _ => null
    };
  }
}