namespace MyNotes.Core.Dto;

internal record TagDbDto
{
  public required Guid Id { get; init; }
  public required string Text { get; init; }
  public required int Color { get; init; }
}

internal record GetTagCountDbDto
{
  public required int Count { get; init; }
}

internal record CachedTagDbDto
{
  public required ImmutableList<Guid> Ids { get; init; }
}

internal record DeleteTagDbDto
{
  public required Guid Id { get; init; }
}