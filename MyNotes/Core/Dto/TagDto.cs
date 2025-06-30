using MyNotes.Core.Model;

namespace MyNotes.Core.Dto;

internal record TagDto
{
  public required Guid Id { get; init; }
  public required string Text { get; init; }
  public required int Color { get; init; }
}

internal record GetTagCountDto
{
  public required int Count { get; init; }
}

internal record DeleteTagDto
{
  public required Guid Id { get; init; }
}

internal record TagToNoteDto
{
  public required Guid NoteId { get; init; }
  public required Guid TagId { get; init; }
}

internal class TagCommandParameterDto
{
  public string Text { get; set; } = "";
  public TagColor Color { get; set; }
}