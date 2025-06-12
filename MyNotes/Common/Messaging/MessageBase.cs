namespace MyNotes.Common.Messaging;

internal abstract class MessageBase
{
  public abstract object? Content { get; }
  public virtual object? Sender { get; protected set; }
}

internal abstract class MessageBase<T>
{
  public abstract T Content { get; }
  public virtual object? Sender { get; protected set; }
}