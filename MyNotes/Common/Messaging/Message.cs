namespace MyNotes.Common.Messaging;
internal class Message : MessageBase
{
  public Message() { }
  public Message(object sender, object? content = null)
  {
    Sender = sender;
    Content = content;
  }
}
