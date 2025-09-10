using MyNotes.Common.Collections;
using MyNotes.Core.Model;

namespace MyNotes.Core.Dto;
internal record NoteDto
{
  public required Guid Id { get; init; }
  public required Guid Parent { get; init; }
  public required DateTimeOffset Created { get; init; }
  public required DateTimeOffset Modified { get; init; }
  public required string Title { get; init; }
  public required string Body { get; init; }
  public required string Preview { get; init; }
  public required string SearchPreview { get; init; }
  public required string Background { get; init; }
  public required int Backdrop { get; init; }
  public required int Width { get; init; }
  public required int Height { get; init; }
  public required int PositionX { get; init; }
  public required int PositionY { get; init; }
  public required bool Bookmarked { get; init; }
  public required bool Trashed { get; init; }
}

internal record GetNoteSearchDto
{
  public required string SearchText { get; init; }
  public required Guid Id { get; init; }
  public required Dictionary<int, Range> Matches { get; init; }
}

internal record GetNotesDto
{
  public required NoteGetFields GetFields { get; init; }
  public required int Offset { get; init; }
  public required int Limit { get; init; }
  public Guid? Parent { get; init; }
  public bool? Bookmarked { get; init; }
  public bool? Trashed { get; init; }
  public NoteSortField? SortField { get; init; }
  public SortDirection? SortDirection { get; init; }
}

internal record UpdateNoteDto
{
  public required NoteUpdateFields UpdateFields { get; init; }
  public required Guid Id { get; init; }
  public Guid? Parent { get; init; }
  public DateTimeOffset? Modified { get; init; }
  public string? Title { get; init; }
  public string? Body { get; init; }
  public string? Preview { get; init; }
  public string? Background { get; init; }
  public int? Backdrop { get; init; }
  public int? Width { get; init; }
  public int? Height { get; init; }
  public int? PositionX { get; init; }
  public int? PositionY { get; init; }
  public bool? Bookmarked { get; init; }
  public bool? Trashed { get; init; }
}

internal record GetNoteTagsDto
{
  public required Guid NoteId { get; init; }
}

internal record DeleteNoteDto
{
  public required Guid Id { get; init; }
}

internal record NoteFileDto
{
  public required string FileName { get; init; }
}

internal record NoteSearchDto
{
  public required Guid Id { get; init; }
  public required string Title { get; init; }
  public required string Body { get; init; }
}

[Flags]
internal enum NoteFields
{
  None = 0,
  Id = 1 << 0,
  Parent = 1 << 1,
  Created = 1 << 2,
  Modified = 1 << 3,
  Title = 1 << 4,
  Body = 1 << 5,
  Preview = 1 << 6,
  Background = 1 << 7,
  Backdrop = 1 << 8,
  Width = 1 << 9,
  Height = 1 << 10,
  PositionX = 1 << 11,
  PositionY = 1 << 12,
  Bookmarked = 1 << 13,
  Trashed = 1 << 14,
  All = int.MaxValue
}

[Flags]
internal enum NoteUpdateFields
{
  None = 0,
  Parent = 1 << 0,
  Modified = 1 << 1,
  Title = 1 << 2,
  Body = 1 << 3,
  Preview = 1 << 4,
  Background = 1 << 5,
  Backdrop = 1 << 6,
  Width = 1 << 7,
  Height = 1 << 8,
  PositionX = 1 << 9,
  PositionY = 1 << 10,
  Bookmarked = 1 << 11,
  Trashed = 1 << 12,
  All = int.MaxValue
}

[Flags]
internal enum NoteGetFields
{
  None = 0,
  Parent = 1 << 0,
  Bookmarked = 1 << 1,
  Trashed = 1 << 2,
  All = int.MaxValue
}