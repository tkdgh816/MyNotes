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
        ReadBoardTableData();
        break;
      case 1:
        ReadNoteTableData();
        break;
      case 2:
        ReadBoardTableData();
        ReadNoteTableData();
        break;
    }
  }

  private void ReadBoardTableData()
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

  private void ReadNoteTableData()
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
    WindowService.Windows.Add(new(window));
  }
}
