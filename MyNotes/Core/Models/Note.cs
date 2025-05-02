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

  private string _body = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed quis nulla vel dui condimentum tristique et ac justo. Integer condimentum tempus ligula id congue. Fusce sed magna lobortis mi ultricies luctus. Sed gravida tortor et feugiat volutpat. Maecenas tincidunt felis in urna bibendum imperdiet. Praesent bibendum, est vel accumsan tempor, massa ex molestie orci, ac imperdiet sapien risus nec libero. Fusce nec nisi aliquet, pellentesque magna id, dictum sem. Nam finibus mauris et scelerisque accumsan. Donec consequat et elit id fringilla. Nam ut quam cursus, consectetur ipsum ut, pellentesque odio. Suspendisse posuere, quam ac rhoncus mollis, felis velit facilisis ipsum, ac accumsan quam libero egestas augue. Morbi quis dui sapien. Pellentesque quis ligula congue, porttitor ligula eget, finibus dolor. Ut a quam sed tortor ornare eleifend. Vivamus gravida mi vitae molestie tincidunt.\r\nSed tempus mi convallis scelerisque ultrices. Aliquam eget dolor vehicula, feugiat tortor nec, maximus diam. Vestibulum ac nunc imperdiet, efficitur lacus vel, rhoncus ipsum. Cras consectetur, sapien ac maximus venenatis, est erat interdum felis, laoreet scelerisque lacus libero a dui. Sed aliquam gravida quam, ac hendrerit magna mollis id. Quisque ac quam sapien. Curabitur pulvinar diam imperdiet diam sodales ultricies. Donec vel nisi et ante lobortis sollicitudin. Integer eleifend eros eu porta feugiat. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Morbi viverra, felis ut laoreet lacinia, ligula dui condimentum nisi, vitae dapibus lacus tortor ut felis. Morbi quam mauris, gravida id justo sed, consequat consequat dolor.\r\nPellentesque odio eros, hendrerit quis eros vel, rutrum ornare tortor. Nam dignissim, metus non dictum vehicula, magna risus pulvinar nisi, eget congue nisl metus id ligula. Etiam pellentesque mauris lectus, eget tristique nisl luctus id. Nulla quis placerat risus. Phasellus id neque est. Nulla porta euismod erat, non efficitur leo dictum sit amet. Vivamus feugiat eros id mauris commodo euismod sed a metus. Sed faucibus nisl dui. Cras in volutpat neque.\r\nInteger vel nunc velit. Fusce aliquet tincidunt accumsan. Donec ullamcorper elementum nibh. Morbi quis tincidunt sem, nec tristique lacus. Sed commodo tempor pretium. Duis suscipit rutrum sagittis. Vestibulum ullamcorper ultricies mi, sit amet vestibulum mi mollis ut. Curabitur ut dictum felis, sit amet commodo nunc. Phasellus maximus molestie tortor. Cras eget suscipit velit. Ut in ex ut quam pretium pretium. In eget turpis velit. Aenean massa nisl, convallis facilisis nulla vitae, lobortis vehicula ex. Ut vitae ligula in odio egestas aliquet. Suspendisse quis congue felis. Fusce at metus metus.";
  public string Body
  {
    get => _body;
    set => SetProperty(ref _body, value);
  }
}
