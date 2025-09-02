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
    (_appTheme, _appLanguage) = (globalSettings.AppTheme, globalSettings.AppLanguage);
    InitialAppLanguage = _appLanguage;
    (_noteBackground, _noteBackdrop) = (noteSettings.Background, noteSettings.Backdrop);
    (_noteWidth, _noteHeight) = (noteSettings.Size.Width, noteSettings.Size.Height);
  }

  private AppTheme _appTheme;
  public AppTheme AppTheme
  {
    get => _appTheme;
    set
    {
      if (_appTheme == value)
        return;
      SetProperty(ref _appTheme, value);
      _settingsService.SetGlobalSettings(AppSettingsKeys.AppTheme, (int)value);
      WeakReferenceMessenger.Default.Send(new Message<AppTheme>(value), Tokens.ChangeTheme);
    }
  }

  private AppLanguage _appLanguage;
  public AppLanguage AppLanguage
  {
    get => _appLanguage;
    set
    {
      if (_appLanguage == value)
        return;
      SetProperty(ref _appLanguage, value);
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

  private Color _noteBackground;
  public Color NoteBackground
  {
    get => _noteBackground;
    set
    {
      if (_noteBackground == value)
        return;
      SetProperty(ref _noteBackground, value);
      _settingsService.SetNoteSettings(AppSettingsKeys.NoteBackground, value.ToString());
    }
  }

  private BackdropKind _noteBackdrop;
  public BackdropKind NoteBackdrop
  {
    get => _noteBackdrop;
    set 
    {
      if (_noteBackdrop == value)
        return;
      SetProperty(ref _noteBackdrop, value);
      _settingsService.SetNoteSettings(AppSettingsKeys.NoteBackdrop, (int)value);
    }
  }

  private int _noteWidth;
  public int NoteWidth
  {
    get => _noteWidth;
    set
    {
      if (_noteWidth == value)
        return;
      SetProperty(ref _noteWidth, value);
      _settingsService.SetNoteSettings(AppSettingsKeys.NoteSize, new Size(value, NoteHeight));
    }
  }

  private int _noteHeight;
  public int NoteHeight
  {
    get => _noteHeight;
    set
    {
      if (_noteHeight == value)
        return;
      SetProperty(ref _noteHeight, value);
      _settingsService.SetNoteSettings(AppSettingsKeys.NoteSize, new Size(NoteWidth, value));
    }
  }
}
