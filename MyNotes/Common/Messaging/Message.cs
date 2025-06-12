namespace MyNotes.Common.Messaging;
internal class Message : MessageBase
{
  public override object? Content { get; }

  public Message() { }
  public Message(object? content = null, object? sender = null)
  {
    Content = content;
    Sender = sender;
  }
}

internal class Message<T> : MessageBase<T>
{
  public override T Content { get; }

  public Message(T content, object? sender = null)
  {
    Content = content;
    Sender = sender;
  }
}