namespace MyNotes.Common.Messaging;

internal class ExtendedAsyncRequestMessage<TResponse> : AsyncRequestMessage<TResponse>
{
  public object? Request { get; init; }
}

internal class ExtendedAsyncRequestMessage<TRequest, TResponse> : AsyncRequestMessage<TResponse>
{
  public TRequest Request { get; init; }
}
