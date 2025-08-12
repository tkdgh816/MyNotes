using MyNotes.Common.Collections;
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
  private readonly ApplicationDataContainer _boardSettingsContainer;
  public IPropertySet GlobalSettings => _globalSettingsContainer.Values;
  public IPropertySet NoteSettings => _noteSettingsContainer.Values;
  public IPropertySet BoardSettings => _boardSettingsContainer.Values;

  public SettingsService()
  {
    _globalSettingsContainer = _localSettingsContainer.CreateContainer("Global", ApplicationDataCreateDisposition.Always);
    _noteSettingsContainer = _localSettingsContainer.CreateContainer("Note", ApplicationDataCreateDisposition.Always);
    _boardSettingsContainer = _localSettingsContainer.CreateContainer("Board", ApplicationDataCreateDisposition.Always);
    InitializeSettings();
  }

  private void InitializeSettings()
  {
    GlobalSettings.TryAdd(AppSettingsKeys.AppTheme, (int)AppTheme.Default);
    GlobalSettings.TryAdd(AppSettingsKeys.AppLanguage, (int)AppLanguage.Default);
    GlobalSettings.TryAdd(AppSettingsKeys.StartupLaunch, false);

    NoteSettings.TryAdd(AppSettingsKeys.NoteBackground, "#FFFAFAD2");
    NoteSettings.TryAdd(AppSettingsKeys.NoteBackdrop, (int)BackdropKind.None);
    NoteSettings.TryAdd(AppSettingsKeys.NoteSize, new Size(300, 300));

    BoardSettings.TryAdd(AppSettingsKeys.BoardNoteSortField, (int)NoteSortField.Created);
    BoardSettings.TryAdd(AppSettingsKeys.BoardNoteSortDirection, (int)SortDirection.Ascending);
    BoardSettings.TryAdd(AppSettingsKeys.BoardViewStyle, (int)BoardViewStyle.Grid_320_100);
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

  public GetBoardSettingsDto GetBoardSettings()
  {
    var sortField = (int)BoardSettings[AppSettingsKeys.BoardNoteSortField];
    var sortDirection = (int)BoardSettings[AppSettingsKeys.BoardNoteSortDirection];
    var viewStyle = (int)BoardSettings[AppSettingsKeys.BoardViewStyle];

    return new()
    {
      SortField = (NoteSortField)sortField,
      SortDirection = (SortDirection)sortDirection,
      ViewStyle = (BoardViewStyle)viewStyle
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

  public void SetBoardSettings(string key, object value)
  {
    if (_boardSettingsContainer.Values.ContainsKey(key))
      _boardSettingsContainer.Values[key] = value;
  }
}
