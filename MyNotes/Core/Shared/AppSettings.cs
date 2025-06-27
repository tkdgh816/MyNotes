namespace MyNotes.Core.Shared;
internal static class AppSettingsKeys
{
  public const string AppTheme = "AppTheme";
  public const string AppLanguage = "AppLanguage";
  public const string StartupLaunch = "StartupLaunch";

  public const string NoteBackground = "NoteBackground";
  public const string NoteBackdrop = "NoteBackdrop";
  public const string NoteSize = "NoteSize";
}

internal enum AppTheme { Default, Light, Dark }

internal enum AppLanguage { Default, en_US, ko_KR  }