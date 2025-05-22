using CommunityToolkit.WinUI.Helpers;
using MyNotes.Core.ViewModels;

namespace MyNotes.Core.Views;

public sealed partial class NoteWindow : Window
{
  public NoteWindow()
  {
    this.InitializeComponent();
    this.ExtendsContentIntoTitleBar = true;
    this.SetTitleBar(VIEW_TitleTextGrid);
    this.Activated += NoteWindow_Activated;
    this.Closed += NoteWindow_Closed;

    _presenter = (OverlappedPresenter)AppWindow.Presenter;
    _presenter.SetBorderAndTitleBar(true, false);
    _presenter.PreferredMinimumWidth = (int)(350 * _dpi);
    _presenter.PreferredMinimumHeight = (int)(250 * _dpi);

    _nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(AppWindow.Id);

    ViewModel = App.Current.GetService<NoteViewModel>();
    InitializeProperties();
    _editorTimer.Tick += OnEditorTimerTick;
    AppWindow.Changed += AppWindow_Changed;
  }

  public NoteViewModel ViewModel { get; }

  private void NoteWindow_Closed(object sender, WindowEventArgs args)
  {
    Debug.WriteLine("NoteWindow Closed");
    this.Activated -= NoteWindow_Activated;
    AppWindow.Changed -= AppWindow_Changed;
    ViewModel.UnregisterNotePropertyChangedEvent();
    _editorTimer.Tick -= OnEditorTimerTick;
    UpdateBodyText();
    ViewModel.ForceUpdateNoteProperties();
    ViewModel.CloseWindow();
  }

  private void InitializeProperties()
  {
    SizeInt32 size = ViewModel.Note.Size;
    PointInt32 position = ViewModel.Note.Position;
    AppWindow.MoveAndResize(new RectInt32(_X: position.X, _Y: position.Y, _Width: (int)(size.Width * _dpi), _Height: (int)(size.Height * _dpi)));
    ViewModel.RegisterNotePropertyChangedEvent();
  }

  private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
  {
    if (args.DidSizeChange)
      ViewModel.ResizeWindow(AppWindow.ClientSize);
    if (args.DidPositionChange)
      ViewModel.MoveWindow(AppWindow.Position);
  }

  static double _dpi = 1.25;
  private void NoteWindow_Activated(object sender, WindowActivatedEventArgs args)
  {
    switch (args.WindowActivationState)
    {
      case WindowActivationState.Deactivated:
        VIEW_TitleBarGrid.Focus(FocusState.Programmatic);
        VisualStateManager.GoToState(VIEW_RootUserControl, "WindowDeactivated", false);
        break;
      default:
        VisualStateManager.GoToState(VIEW_RootUserControl, "WindowActivated", false);
        break;
    }
  }

  private readonly OverlappedPresenter _presenter;
  private readonly InputNonClientPointerSource _nonClientInputSrc;

  bool _isLayoutLoaded = false;
  private void VIEW_RootUserControl_Loaded(object sender, RoutedEventArgs e)
  {
    _isLayoutLoaded = true;
  }

  private void VIEW_MinimizeButton_Click(object sender, RoutedEventArgs e)
  {
    _presenter.Minimize();
  }

  private void VIEW_CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();

  private void VIEW_PinButton_Click(object sender, RoutedEventArgs e)
  {
    if (_presenter.IsAlwaysOnTop)
    {
      VIEW_PinButton.Content = "\uEE49";
      _presenter.IsAlwaysOnTop = false;
    }
    else
    {
      VIEW_PinButton.Content = "\uEE47";
      _presenter.IsAlwaysOnTop = true;
    }
  }

  private void VIEW_TextEditorRichEditBox_Loaded(object sender, RoutedEventArgs e)
  {
    VIEW_TextEditorRichEditBox.Document.SetText(TextSetOptions.None, ViewModel.Note.Body);
  }

  private void VIEW_TitleTextBox_LostFocus(object sender, RoutedEventArgs e)
  {
    _nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, null);
    VIEW_TitleTextBox.IsEnabled = false;
  }

  private void VIEW_MoreMenuViewBoardMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    ViewModel.GetMainWindow();
  }

  RectInt32 _titleTextGridRect = new() { X = (int)(64 * _dpi), Y = 0, Height = (int)(32 * _dpi) };
  private void VIEW_MoreMenuRenameMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    _titleTextGridRect.Width = (int)(VIEW_TitleTextGrid.ActualWidth * _dpi);
    _nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, [_titleTextGridRect]);
    VIEW_TitleTextBox.IsEnabled = true;
    VIEW_TitleTextBox.Focus(FocusState.Programmatic);
  }

  private void VIEW_TextEditorRichEditBox_SelectionChanged(object sender, RoutedEventArgs e)
  {
    //VIEW_TextEditorRichEditBox.Document.Selection.GetRect(PointOptions.IncludeInset, out Rect rect, out _);
    //Debug.WriteLine($"{rect.X}, {rect.Width}, {rect.Y}, {rect.Height}");
    var selectedText = VIEW_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
    {
      var characterFormat = selectedText.CharacterFormat;
      VIEW_BoldButton.IsChecked = selectedText.CharacterFormat.Bold == FormatEffect.On;
      VIEW_ItalicButton.IsChecked = selectedText.CharacterFormat.Italic == FormatEffect.On;
      VIEW_UnderlineButton.IsChecked = selectedText.CharacterFormat.Underline == UnderlineType.Single;
      VIEW_StrikethroughButton.IsChecked = selectedText.CharacterFormat.Strikethrough == FormatEffect.On;
    }
  }

  private void VIEW_BoldButton_Click(object sender, RoutedEventArgs e)
  {
    var selectedText = VIEW_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
      selectedText.CharacterFormat.Bold = FormatEffect.Toggle;
  }

  private void VIEW_ItalicButton_Click(object sender, RoutedEventArgs e)
  {
    var selectedText = VIEW_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
      selectedText.CharacterFormat.Italic = FormatEffect.Toggle;
  }

  private void VIEW_UnderlineButton_Click(object sender, RoutedEventArgs e)
  {
    var selectedText = VIEW_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
    {
      if (selectedText.CharacterFormat.Underline == UnderlineType.None)
        selectedText.CharacterFormat.Underline = UnderlineType.Single;
      else
        selectedText.CharacterFormat.Underline = UnderlineType.None;
    }
  }

  private void VIEW_StrikethroughButton_Click(object sender, RoutedEventArgs e)
  {
    var selectedText = VIEW_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
      selectedText.CharacterFormat.Strikethrough = FormatEffect.Toggle;
  }

  private void VIEW_MarkerButton_Click(object sender, RoutedEventArgs e)
  {
    var selectedText = VIEW_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
    {
      MarkerType markerType = VIEW_MarkerFlyoutGridView.SelectedIndex switch
      {
        1 => MarkerType.Arabic,
        2 => MarkerType.LowercaseEnglishLetter,
        3 => MarkerType.UppercaseEnglishLetter,
        4 => MarkerType.LowercaseRoman,
        5 => MarkerType.UppercaseRoman,
        6 => MarkerType.CircledNumber,
        0 or _ => MarkerType.Bullet,
      };
      var paragraphFormat = selectedText.ParagraphFormat;
      paragraphFormat.ListType = (paragraphFormat.ListType == markerType) ? MarkerType.None : markerType;
    }
  }

  private bool _preventComboBoxSelectionChanging;
  private void VIEW_FontSizeButtonFlyout_Opened(object sender, object e)
  {
    var selectedText = VIEW_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
    {
      var startPosition = selectedText.StartPosition;
      var textRange = VIEW_TextEditorRichEditBox.Document.GetRange(startPosition, startPosition + 1);
      _preventComboBoxSelectionChanging = true;
      VIEW_FontSizeComboBox.SelectedItem = textRange.CharacterFormat.Size.ToString();
      _preventComboBoxSelectionChanging = false;
    }
  }

  private void VIEW_FontSizeDownButton_Click(object sender, RoutedEventArgs e)
  {
    VIEW_FontSizeComboBox.SelectedIndex -= 1;
  }

  private void VIEW_FontSizeUpButton_Click(object sender, RoutedEventArgs e)
  {
    VIEW_FontSizeComboBox.SelectedIndex += 1;
  }

  const int FontSizeComboBoxItemsCount = 12;
  private void VIEW_FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    var selectedText = VIEW_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null && !_preventComboBoxSelectionChanging)
    {
      if (float.TryParse((string)VIEW_FontSizeComboBox.SelectedItem, out var fontSize))
        selectedText.CharacterFormat.Size = fontSize;
      else
        selectedText.CharacterFormat.Size = 10.5f;
    }

    VIEW_FontSizeDownButton.IsEnabled = VIEW_FontSizeComboBox.SelectedIndex != 0;
    VIEW_FontSizeUpButton.IsEnabled = VIEW_FontSizeComboBox.SelectedIndex != FontSizeComboBoxItemsCount;

  }

  private void VIEW_FontColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
  {
    VIEW_FontColorButtonFillFontIcon.Foreground = new SolidColorBrush(args.NewColor);
  }

  private void VIEW_FontColorButton_Click(object sender, RoutedEventArgs e)
  {
    var selectedText = VIEW_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
    {
      selectedText.CharacterFormat.ForegroundColor = VIEW_FontColorPicker.Color;
    }
  }

  private void VIEW_HighlightColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
  {
    VIEW_HighlightButtonFillFontIcon.Foreground = new SolidColorBrush(args.NewColor);
  }


  private void VIEW_HighlightButton_Click(object sender, RoutedEventArgs e)
  {
    var selectedText = VIEW_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
    {
      selectedText.CharacterFormat.BackgroundColor = VIEW_HighlightColorPicker.Color;
    }
  }

  private void VIEW_BackdropRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    SystemBackdrop = VIEW_BackdropRadioButtons.SelectedIndex switch
    {
      1 => new DesktopAcrylicBackdrop(),
      2 => new MicaBackdrop(),
      0 or _ => null,
    };
  }

  private void VIEW_BackgroundColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
  {
    return;
    if (args.NewColor.ToHsv().V > 0.5)
      VisualStateManager.GoToState(VIEW_RootUserControl, "BackgroundColorLight", false);
    else
      VisualStateManager.GoToState(VIEW_RootUserControl, "BackgroundColorDark", false);
  }

  private void VIEW_ViewModeEditRadioMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    => ChangeViewMode(true);

  private void VIEW_ViewModeReadRadioMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    => ChangeViewMode(false);

  public void ChangeViewMode(bool isEditMode)
  {
    VIEW_TextEditorRichEditBox.IsReadOnly = !isEditMode;
    VIEW_BoldButton.IsEnabled = isEditMode;
    VIEW_ItalicButton.IsEnabled = isEditMode;
    VIEW_UnderlineButton.IsEnabled = isEditMode;
    VIEW_StrikethroughButton.IsEnabled = isEditMode;
    VIEW_MarkerButton.IsEnabled = isEditMode;
    VIEW_FontSizeButton.IsEnabled = isEditMode;
    VIEW_FontColorButton.IsEnabled = isEditMode;
    VIEW_HighlightButton.IsEnabled = isEditMode;
  }

  private async void VIEW_RemoveMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    await VIEW_RemoveNoteContentDialog.ShowAsync();
  }

  private async void VIEW_AboutMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    await VIEW_NoteInfoContentDialog.ShowAsync();
  }

  private DispatcherTimer _editorTimer = new() { Interval = TimeSpan.FromSeconds(10) };
  private int _editorCounter = 0;
  private const int EditorCounterMax = 50;
  private void VIEW_TextEditorRichEditBox_TextChanged(object sender, RoutedEventArgs e)
  {
    _editorTimer.Stop();
    ViewModel.IsBodyChanged = true;
    if (_editorCounter++ > EditorCounterMax)
    {
      _editorCounter = 0;
      UpdateBodyText();
    }
    else
      _editorTimer.Start();
  }
  private void OnEditorTimerTick(object? sender, object e)
  {
    UpdateBodyText();
  }
  private void UpdateBodyText()
  {
    VIEW_TextEditorRichEditBox.Document.GetText(TextGetOptions.AdjustCrlf, out string body);
    ViewModel.UpdateBody(body);
  }
}
