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

  private string _body = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.\r\nSuspendisse id ex et purus mollis iaculis id vitae enim.\r\nEtiam sed velit at eros rutrum tincidunt.\r\nNulla tincidunt quam vitae nisi hendrerit, molestie consequat lacus interdum.\r\nNullam porta, nisi eget vestibulum efficitur, nunc est hendrerit nunc, vitae hendrerit nulla ligula non magna.\r\nMauris et justo vel leo semper commodo vitae sed urna.\r\nDuis rutrum nulla nec pellentesque vestibulum.\r\nDuis sed ultricies ex.\r\nSed at nunc neque.";
  public string Body
  {
    get => _body;
    set => SetProperty(ref _body, value);
  }
}
