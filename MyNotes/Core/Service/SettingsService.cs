using MyNotes.Core.Dto;
using MyNotes.Core.Model;
using MyNotes.Core.Shared;

using Windows.Foundation.Collections;

using ToolkitColorHelper = CommunityToolkit.WinUI.Helpers.ColorHelper;

namespace MyNotes.Core.Service;
internal class SettingsService
{
  private readonly ApplicationDataContainer _localSettingsContainer = ApplicationData.Current.LocalSettings;
  private readonly ApplicationDataContainer _globalSettingsContainer;
  private readonly ApplicationDataContainer _noteSettingsContainer;
  public IPropertySet GlobalSettings => _globalSettingsContainer.Values;
  public IPropertySet NoteSettings => _noteSettingsContainer.Values;

  public SettingsService()
  {
    _globalSettingsContainer = _localSettingsContainer.CreateContainer("Global", ApplicationDataCreateDisposition.Always);
    _noteSettingsContainer = _localSettingsContainer.CreateContainer("Note", ApplicationDataCreateDisposition.Always);
    InitializeSettings();
  }

  private void InitializeSettings()
  {
    _globalSettingsContainer.Values.TryAdd(AppSettingsKeys.AppTheme, (int)AppTheme.Default);
    _globalSettingsContainer.Values.TryAdd(AppSettingsKeys.AppLanguage, (int)AppLanguage.Default);
    _globalSettingsContainer.Values.TryAdd(AppSettingsKeys.StartupLaunch, false);

    _noteSettingsContainer.Values.TryAdd(AppSettingsKeys.NoteBackground, "#FFFAFAD2");
    _noteSettingsContainer.Values.TryAdd(AppSettingsKeys.NoteBackdrop, (int)BackdropKind.None);
    _noteSettingsContainer.Values.TryAdd(AppSettingsKeys.NoteSize, new Size(300, 300));
  }

  public GetGlobalSettingsDto GetGlobalSettings()
  {
    var theme = (int)GlobalSettings[AppSettingsKeys.AppTheme];
    var language = (int)GlobalSettings[AppSettingsKeys.AppLanguage];
    var startup = (bool)GlobalSettings[AppSettingsKeys.StartupLaunch];

    return new()
    {
      AppTheme = (AppTheme)theme,
      AppLanguage = (AppLanguage)language,
      StartupLaunch = startup
    };
  }

  public GetNoteSettingsDto GetNoteSettings()
  {
    var background = (string)NoteSettings[AppSettingsKeys.NoteBackground];
    var backdrop = (int)NoteSettings[AppSettingsKeys.NoteBackdrop];
    var size = (Size)NoteSettings[AppSettingsKeys.NoteSize];

    return new()
    {
      Background = ToolkitColorHelper.ToColor(background),
      Backdrop = (BackdropKind)backdrop,
      Size = new SizeInt32((int)size.Width, (int)size.Height)
    };
  }

  public void SetGlobalSettings(string key, object value)
  {
    if (_globalSettingsContainer.Values.ContainsKey(key))
      _globalSettingsContainer.Values[key] = value;
  }

  public void SetNoteSettings(string key, object value)
  {
    if (_noteSettingsContainer.Values.ContainsKey(key))
      _noteSettingsContainer.Values[key] = value;
  }
}
