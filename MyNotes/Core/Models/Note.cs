namespace MyNotes.Core.Models;

public class Note : ObservableObject
{
  public Note(string title)
  {
    _title = title;
  }

  private string _title;
  public string Title
  {
    get => _title;
    set => SetProperty(ref _title, value);
  }
}
