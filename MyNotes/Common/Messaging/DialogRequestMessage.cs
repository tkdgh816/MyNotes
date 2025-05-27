namespace MyNotes.Common.Messaging;

internal class DialogRequestMessage : MessageBase
{
  public DialogRequestMessage() { }
  public DialogRequestMessage(object content) { Content = content; }
}
