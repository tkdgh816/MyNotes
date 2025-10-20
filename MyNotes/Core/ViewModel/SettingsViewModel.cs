using Microsoft.Windows.Globalization;

using MyNotes.Common.Messaging;
using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Core.Shared;

namespace MyNotes.Core.ViewModel;
internal class SettingsViewModel : ViewModelBase
{
  private readonly SettingsService _settingsService;

  public SettingsViewModel(SettingsService settingsService)
  {
    _settingsService = settingsService;
    var globalSettings = _settingsService.GetGlobalSettings();
    var noteSettings = _settingsService.GetNoteSettings();
    var boardSettings = _settingsService.GetBoardSettings();
    (AppTheme, AppLanguage) = (globalSettings.AppTheme, globalSettings.AppLanguage);
    InitialAppLanguage = AppLanguage;
    (NoteBackground, NoteBackdrop) = (noteSettings.Background, noteSettings.Backdrop);
    (NoteWidth, NoteHeight) = (noteSettings.Size.Width, noteSettings.Size.Height);
    DisplayNoteCount = boardSettings.DisplayNoteCount;
  }

  public AppTheme AppTheme
  {
    get => field;
    set
    {
      if (field == value)
        return;
      SetProperty(ref field, value);
      _settingsService.SetGlobalSettings(AppSettingsKeys.AppTheme, (int)value);
      WeakReferenceMessenger.Default.Send(new Message<AppTheme>(value), Tokens.ChangeTheme);
    }
  }

  public AppLanguage AppLanguage
  {
    get => field;
    set
    {
      if (field == value)
        return;
      SetProperty(ref field, value);
      _settingsService.SetGlobalSettings(AppSettingsKeys.AppLanguage, (int)value);
      ApplicationLanguages.PrimaryLanguageOverride = value switch
      {
        AppLanguage.en_US => "en-US",
        AppLanguage.ko_KR => "ko",
        AppLanguage.Default or _ => ApplicationLanguages.Languages[0]
      };
    }
  }
  public AppLanguage InitialAppLanguage { get; }

  public Color NoteBackground
  {
    get => field;
    set
    {
      if (field == value)
        return;
      SetProperty(ref field, value);
      _settingsService.SetNoteSettings(AppSettingsKeys.NoteBackground, value.ToString());
    }
  }

  public BackdropKind NoteBackdrop
  {
    get => field;
    set 
    {
      if (field == value)
        return;
      SetProperty(ref field, value);
      _settingsService.SetNoteSettings(AppSettingsKeys.NoteBackdrop, (int)value);
    }
  }

  public int NoteWidth
  {
    get => field;
    set
    {
      if (field == value)
        return;
      SetProperty(ref field, value);
      _settingsService.SetNoteSettings(AppSettingsKeys.NoteSize, new Size(value, NoteHeight));
    }
  }

  public int NoteHeight
  {
    get => field;
    set
    {
      if (field == value)
        return;
      SetProperty(ref field, value);
      _settingsService.SetNoteSettings(AppSettingsKeys.NoteSize, new Size(NoteWidth, value));
    }
  }

  public bool DisplayNoteCount
  {
    get => field;
    set 
    {
      if (field == value)
        return;
      SetProperty(ref field, value);
      _settingsService.SetBoardSettings(AppSettingsKeys.BoardDisplayNoteCount, value);
    }
  }
}
