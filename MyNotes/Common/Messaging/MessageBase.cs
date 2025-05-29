namespace MyNotes.Common.Messaging;

internal abstract class MessageBase
{
  public virtual object? Content { get; protected set; }
  public virtual object? Sender { get; protected set; }
}
