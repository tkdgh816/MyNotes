using MyNotes.Common.Collections;
using MyNotes.Core.Model;
using MyNotes.Core.Shared;

namespace MyNotes.Core.Dto;

internal record GetGlobalSettingsDto
{
  public required AppTheme AppTheme { get; init; }
  public required AppLanguage AppLanguage { get; init; }
  public required bool StartupLaunch { get; init; }
}

internal record GetNoteSettingsDto
{
  public required Color Background { get; init; }
  public required BackdropKind Backdrop { get; init; }
  public required SizeInt32 Size { get; init; }
}

internal record GetBoardSettingsDto
{
  public required NoteSortField SortField { get; init; }
  public required SortDirection SortDirection { get; init; }
  public required BoardViewStyle ViewStyle { get; init; }
  public required bool DisplayNoteCount { get; init; }
}