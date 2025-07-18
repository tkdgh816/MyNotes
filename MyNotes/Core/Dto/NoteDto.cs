﻿namespace MyNotes.Core.Dto;
internal record NoteDto
{
  public required Guid Id { get; init; }
  public required Guid Parent { get; init; }
  public required DateTimeOffset Created { get; init; }
  public required DateTimeOffset Modified { get; init; }
  public required string Title { get; init; }
  public required string Body { get; init; }
  public required string Background { get; init; }
  public required int Backdrop { get; init; }
  public required int Width { get; init; }
  public required int Height { get; init; }
  public required int PositionX { get; init; }
  public required int PositionY { get; init; }
  public required bool Bookmarked { get; init; }
  public required bool Trashed { get; init; }
}

internal record GetNoteDto
{
  public required Guid Id { get; init; }
}

internal record UpdateNoteDto
{
  public required NoteUpdateFields UpdateFields { get; init; }
  public required Guid Id { get; init; }
  public Guid? Parent { get; init; }
  public DateTimeOffset? Modified { get; init; }
  public string? Title { get; init; }
  public string? Body { get; init; }
  public string? Background { get; init; }
  public int? Backdrop { get; init; }
  public int? Width { get; init; }
  public int? Height { get; init; }
  public int? PositionX { get; init; }
  public int? PositionY { get; init; }
  public bool? Bookmarked { get; init; }
  public bool? Trashed { get; init; }
}

[Flags]
internal enum NoteUpdateFields
{
  None = 0,
  Parent = 1 << 0,
  Modified = 1 << 1,
  Title = 1 << 2,
  Body = 1 << 3,
  Background = 1 << 4,
  Backdrop = 1 << 5,
  Width = 1 << 6,
  Height = 1 << 7,
  PositionX = 1 << 8,
  PositionY = 1 << 9,
  Bookmarked = 1 << 10,
  Trashed = 1 << 11,
  All = int.MaxValue
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