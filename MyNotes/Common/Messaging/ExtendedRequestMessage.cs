namespace MyNotes.Common.Messaging;

internal class ExtendedRequestMessage<TResponse> : MessageBase
{
  public object? Request { get; init; }
  public override object? Content => Request;

  public bool HasReceivedResponse { get; private set; } = false;
  public bool IsReplied { get; private set; } = false;

  private TResponse? _response;
  public TResponse? Response => IsReplied ? _response : FallbackResponse;

  public TResponse? FallbackResponse { get; init; } = default;

  public ExtendedRequestMessage(TResponse? fallbackResponse = default)
  {
    FallbackResponse = fallbackResponse;
  }
  public ExtendedRequestMessage(object request, TResponse? fallbackResponse = default)
  {
    Request = request;
    FallbackResponse = fallbackResponse;
  }

  public void Reply(TResponse response)
  {
    if (HasReceivedResponse)
      throw new InvalidOperationException("A response has already been issued for the current message.");

    HasReceivedResponse = true;
    IsReplied = true;
    _response = response;
  }
}

internal class ExtendedRequestMessage<TRequest, TResponse> : MessageBase<TRequest>
{
  public TRequest Request { get; init; }
  public override TRequest Content => Request;

  public bool HasReceivedResponse { get; private set; } = false;
  public bool IsReplied { get; private set; } = false;

  private TResponse? _response;
  public TResponse? Response => IsReplied ? _response : FallbackResponse;

  public TResponse? FallbackResponse { get; init; } = default;

  public ExtendedRequestMessage(TRequest request, TResponse? fallbackResponse = default)
  { 
    Request = request;
    FallbackResponse = fallbackResponse;
  }

  public void Reply(TResponse response)
  {
    if (HasReceivedResponse)
      throw new InvalidOperationException("A response has already been issued for the current message.");

    HasReceivedResponse = true;
    IsReplied = true;
    _response = response;
  }
}