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

internal record UpdateGlobalSettingsDto
{
  public required GlobalSettingsUpdateFields UpdateFields { get; init; }
  public int? AppTheme { get; init; }
  public int? AppLanguage { get; init; }
  public int? PaneDisplayMode { get; init; }
  public bool? DeleteConfirmation { get; init; }
  public bool? StartupLaunch { get; init; }
}

internal record UpdateNoteSettingsDto
{
  public required NoteSettingsUpdateFields UpdateFields { get; init; }
  public string? Background { get; init; }
  public int? Backdrop { get; init; }
  public int? Width { get; init; }
  public int? Height { get; init; }
}

[Flags]
internal enum GlobalSettingsUpdateFields
{
  None = 0,
  AppTheme = 1 << 0,
  AppLanguage = 1 << 1,
  PaneDisplayMode = 1 << 2,
  DeleteConfirmation = 1 << 3,
  StartupLaunch = 1 << 4,
  All = int.MaxValue
}

[Flags]
internal enum NoteSettingsUpdateFields
{
  None = 0,
  Background = 1 << 0,
  Backdrop = 1 << 1,
  Width = 1 << 2,
  Height = 1 << 3,
  All = int.MaxValue
}