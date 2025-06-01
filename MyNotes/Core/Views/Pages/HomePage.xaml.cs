using System.Text;

using MyNotes.Core.Services;

namespace MyNotes.Core.Views;
public sealed partial class HomePage : Page
{
  public HomePage()
  {
    this.InitializeComponent();
    WindowService = App.Current.GetService<WindowService>();
    DatabaseService = App.Current.GetService<DatabaseService>();
  }

  public WindowService WindowService { get; }
  public DatabaseService DatabaseService { get; }

  private void ReadTableButton_Click(object sender, RoutedEventArgs e)
  {
    ReadTableTextBlock0.Text = "";
    ReadTableTextBlock1.Text = "";
    ReadTableTextBlock2.Text = "";
    ReadTableTextBlock3.Text = "";
    switch (DatabaseTableRadioButtons.SelectedIndex)
    {
      case 0:
        ReadBoardsTableData();
        break;
      case 1:
        ReadNotesTableData();
        break;
      case 2:
        ReadBoardsTableData();
        ReadNotesTableData();
        ReadTagsTableData();
        ReadNotesTagsTableData();
        break;
    }
  }

  private void ReadBoardsTableData()
  {
    StringBuilder sb0 = new(), sb1 = new();
    int countBoard = 0;
    foreach (string text in DatabaseService.ReadTableData("Boards"))
      if (countBoard++ % 2 == 0)
        sb0.AppendLine(text);
      else
        sb1.AppendLine(text);
    ReadTableTextBlock0.Text = sb0.ToString();
    ReadTableTextBlock1.Text = sb1.ToString();
  }

  private void ReadNotesTableData()
  {
    StringBuilder sb2 = new(), sb3 = new();
    int countNote = 0;
    foreach (string text in DatabaseService.ReadTableData("Notes"))
      if (countNote++ % 2 == 0)
        sb2.AppendLine(text);
      else
        sb3.AppendLine(text);
    ReadTableTextBlock2.Text = sb2.ToString();
    ReadTableTextBlock3.Text = sb3.ToString();
  }

  private void ReadTagsTableData()
  {
    StringBuilder sb = new();
    foreach (string text in DatabaseService.ReadTableData("Tags"))
      sb.Append(text);
    ReadTableTextBlock4.Text = sb.ToString();
  }

  private void ReadNotesTagsTableData()
  {
    StringBuilder sb = new();
    foreach (string text in DatabaseService.ReadTableData("NotesTags"))
      sb.Append(text);
    ReadTableTextBlock5.Text = sb.ToString().TrimStart();
  }

  private async void DeleteDataButton_Click(object sender, RoutedEventArgs e)
  {
    ContentDialog dialog = new()
    {
      XamlRoot = this.XamlRoot,
      Title = "Delete data",
      Content = "Delete data",
      PrimaryButtonText = "Yes",
      CloseButtonText = "Cancel",
      DefaultButton = ContentDialogButton.Close
    };

    if (await dialog.ShowAsync() == ContentDialogResult.Primary)
    {
      switch (DatabaseTableRadioButtons.SelectedIndex)
      {
        case 0:
          DatabaseService.DeleteTableData("Boards");
          break;
        case 1:
          DatabaseService.DeleteTableData("Notes");
          break;
        case 2:
          DatabaseService.DeleteTableData("Boards");
          DatabaseService.DeleteTableData("Notes");
          break;
      }
    }
  }

  private async void DropTableButton_Click(object sender, RoutedEventArgs e)
  {
    ContentDialog dialog = new()
    {
      XamlRoot = this.XamlRoot,
      Title = "Drop table",
      Content = "Drop table",
      PrimaryButtonText = "Yes",
      CloseButtonText = "Cancel",
      DefaultButton = ContentDialogButton.Close
    };

    if (await dialog.ShowAsync() == ContentDialogResult.Primary)
    {
      switch (DatabaseTableRadioButtons.SelectedIndex)
      {
        case 0:
          DatabaseService.DropTable("Boards");
          break;
        case 1:
          DatabaseService.DropTable("Notes");
          break;
        case 2:
          DatabaseService.DropTable("Boards");
          DatabaseService.DropTable("Notes");
          break;
      }
    }
  }

  private void ViewWindowsButton_Click(object sender, RoutedEventArgs e)
  {
    WindowService.ReadActiveWindows();
  }

  private void TestWindowButton_Click(object sender, RoutedEventArgs e)
  {
    TestWindow window = new();
    window.Activate();
    WindowService.WindowReferences.Add(new(window));
  }

  public double CalculateContrastRatio(Color first, Color second)
  {
    var relLuminanceOne = GetRelativeLuminance(first);
    var relLuminanceTwo = GetRelativeLuminance(second);
    return (Math.Max(relLuminanceOne, relLuminanceTwo) + 0.05)
        / (Math.Min(relLuminanceOne, relLuminanceTwo) + 0.05);
  }

  public double GetRelativeLuminance(Color c)
  {
    var rSRGB = c.R / 255.0;
    var gSRGB = c.G / 255.0;
    var bSRGB = c.B / 255.0;

    var r = rSRGB <= 0.04045 ? rSRGB / 12.92 : Math.Pow((rSRGB + 0.055) / 1.055, 2.4);
    var g = gSRGB <= 0.04045 ? gSRGB / 12.92 : Math.Pow((gSRGB + 0.055) / 1.055, 2.4);
    var b = bSRGB <= 0.04045 ? bSRGB / 12.92 : Math.Pow((bSRGB + 0.055) / 1.055, 2.4);
    return 0.2126 * r + 0.7152 * g + 0.0722 * b;
  }

  private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
  {
    CheckContrast();
  }

  private void CheckContrast()
  {
    double ratio = CalculateContrastRatio(BackgroundColorPicker.Color, ForegroundColorPicker.Color);
    ContrastTestTextBlock.Text = $"{Math.Round(ratio, 2)}";
    RegularTestTextBlock.Text = (ratio >= 4.5) ? "Pass" : "Fail";
    RegularTestTextBlock.Foreground = (ratio >= 4.5) ? new SolidColorBrush(Colors.DarkGreen) : new SolidColorBrush(Colors.DarkRed);
    LargeTestTextBlock.Text = (ratio >= 3.0) ? "Pass" : "Fail";
    LargeTestTextBlock.Foreground = (ratio >= 3.0) ? new SolidColorBrush(Colors.DarkGreen) : new SolidColorBrush(Colors.DarkRed);
    double luminance = GetRelativeLuminance(BackdropColorPicker.Color);
    LuminanceTestTextBlock.Text = $"{Math.Round(luminance, 2)}";
  }
}
