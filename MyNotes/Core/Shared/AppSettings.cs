namespace MyNotes.Core.Shared;
internal static class AppSettingsKeys
{
  public const string AppTheme = "AppTheme";
  public const string AppLanguage = "AppLanguage";
  public const string AppStartupLaunch = "AppStartupLaunch";

  public const string NoteBackground = "NoteBackground";
  public const string NoteBackdrop = "NoteBackdrop";
  public const string NoteSize = "NoteSize";

  public const string BoardNoteSortField = "BoardNotesSortField";
  public const string BoardNoteSortDirection = "BoardNotesSortDirection";
  public const string BoardViewStyle = "BoardViewStyle";
  public const string BoardDisplayNoteCount = "BoardDisplayNoteCount";
}

internal enum AppTheme { Default, Light, Dark }

internal enum AppLanguage { Default, en_US, ko_KR  }