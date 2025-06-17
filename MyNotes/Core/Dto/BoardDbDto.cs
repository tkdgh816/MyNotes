namespace MyNotes.Core.Dto;

internal record BoardDbDto
{
  public required Guid Id { get; init; }
  public required bool Grouped { get; init; }
  public required Guid Parent { get; init; }
  public required Guid? Previous { get; init; }
  public required string Name { get; init; }
  public required int IconType { get; init; }
  public required int IconValue { get; init; }
}

internal record InsertBoardDbDto
{
  public required Guid Id { get; init; }
  public required bool Grouped { get; init; }
  public required Guid Parent { get; init; }
  public required Guid? Previous { get; init; }
  public required Guid? Next { get; init; }
  public required string Name { get; init; }
  public required int IconType { get; init; }
  public required int IconValue { get; init; }
}

internal record DeleteBoardDbDto
{
  public required Guid Id { get; init; }
}

internal record GetBoardNotesDbDto
{
  public required Guid Id { get; init; }
}

internal record UpdateBoardDbDto
{
  public required BoardUpdateFields UpdateFields { get; init; }
  public required Guid Id { get; init; }
  public Guid? Parent { get; init; }
  public Guid? Previous { get; init; }
  public string? Name { get; init; }
  public int? IconType { get; init; }
  public int? IconValue { get; init; }
}

[Flags]
internal enum BoardUpdateFields
{
  None = 0,
  Parent = 1 << 0,
  Previous = 1 << 1,
  Name = 1 << 2,
  IconType = 1 << 3,
  IconValue = 1 << 4,
  All = int.MaxValue
}