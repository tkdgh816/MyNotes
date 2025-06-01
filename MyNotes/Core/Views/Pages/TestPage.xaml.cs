using MyNotes.Core.Services;

namespace MyNotes.Core.Views;
public sealed partial class TestPage : Page
{
  public TestPage()
  {
    InitializeComponent();
    WindowService = ((App)Application.Current).GetService<WindowService>();
    WindowService.PageReferences.Add(new(this));
  }

  public WindowService WindowService { get; }
  public TestModel Model { get; private set; } = new();

  private void CustomButton_Click(object sender, RoutedEventArgs e)
  {
    Model.Text = CustomTextBox.Text;
  }
}

public class TestModel : ObservableObject
{
  private string _text = "";
  public string Text
  {
    get => _text;
    set => SetProperty(ref _text, value);
  }
}