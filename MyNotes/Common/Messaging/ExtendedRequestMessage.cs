namespace MyNotes.Common.Messaging;

internal class ExtendedRequestMessage<T> : RequestMessage<T>
{
  public object Content;

  public ExtendedRequestMessage(object content) => Content = content;
}
