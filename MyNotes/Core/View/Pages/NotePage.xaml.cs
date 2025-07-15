using MyNotes.Common.Messaging;
using MyNotes.Core.Shared;
using MyNotes.Core.ViewModel;
using MyNotes.Debugging;

namespace MyNotes.Core.View;

internal sealed partial class NotePage : Page
{
  public NotePage()
  {
    InitializeComponent();
    this.Loaded += NotePage_Loaded;
    this.Unloaded += NotePage_Unloaded;
  }

  private async void NotePage_Loaded(object sender, RoutedEventArgs e)
  {
    await LoadBodyAsync();
    ReferenceTracker.NotePageReferences.Add(new(this.GetType().Name, this));
    ViewModel.SetWindowTitlebar(View_TitleTextGrid);
    RegisterEvents();
    RegisterMessengers();
    ViewModel.UnsetNotePropertyUpdateFieldMap();
  }

  private async void NotePage_Unloaded(object sender, RoutedEventArgs e)
  {
    UnRegisterEvents();
    UnregisterMessengers();

    View_TextEditorRichEditBox.Document.GetText(TextGetOptions.AdjustCrlf, out string body);
    if (ViewModel.Note.Created == ViewModel.Note.Modified && string.IsNullOrWhiteSpace(body))
    {
      ViewModel.DeleteNoteCommand?.Execute();
    }
    else
    {
      UpdateBody(body);
      await SaveBodyAsync();
    }

    ViewModel.SetNotePropertyUpdateFieldMap();

    if (!ViewModel.IsNoteInBoard())
      ViewModel.Dispose();
  }

  private async Task LoadBodyAsync()
  {
    try
    {
      StorageFile file = await ViewModel.GetFile();
      using var stream = await file.OpenAsync(FileAccessMode.Read);
      try
      {
        View_TextEditorRichEditBox.Document.LoadFromStream(TextSetOptions.FormatRtf, stream);
      }
      catch (Exception)
      { }
    }
    catch (Exception)
    { }
  }

  private async Task SaveBodyAsync()
  {
    var file = await ViewModel.CreateFile();
    using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
    View_TextEditorRichEditBox.Document.SaveToStream(TextGetOptions.FormatRtf, stream);
  }

  public NoteViewModel ViewModel { get; set; } = null!;

  #region Handling Events
  private void RegisterEvents()
  {
    _editorInputDebounceTimer.Tick += OnEditorInputDebounceTimerTick;
  }
  private void UnRegisterEvents()
  {
    _editorInputDebounceTimer.Tick -= OnEditorInputDebounceTimerTick;
  }
  #endregion

  static double _dpi = 1.25;

  private void View_TextEditorRichEditBox_Loaded(object sender, RoutedEventArgs e)
  {
    View_TextEditorRichEditBox.Document.SetText(TextSetOptions.None, ViewModel.Note.Body);
  }

  private void View_TitleTextBox_LostFocus(object sender, RoutedEventArgs e)
  {
    ViewModel.SetWindowRegionRects(null);
    View_TitleTextBox.IsEnabled = false;
  }

  private void View_MoreMenuViewBoardMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    ViewModel.GetMainWindow();
  }

  RectInt32 _titleTextGridRect = new() { X = (int)(64 * _dpi), Y = 0, Height = (int)(32 * _dpi) };
  private void View_MoreMenuRenameMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    _titleTextGridRect.Width = (int)(View_TitleTextGrid.ActualWidth * _dpi);
    ViewModel.SetWindowRegionRects([_titleTextGridRect]);
    View_TitleTextBox.IsEnabled = true;
    View_TitleTextBox.Focus(FocusState.Programmatic);
  }

  #region Timers
  private DispatcherTimer _editorInputDebounceTimer = new() { Interval = TimeSpan.FromSeconds(10) };
  private void OnEditorInputDebounceTimerTick(object? sender, object e)
  {
    View_TextEditorRichEditBox.Document.GetText(TextGetOptions.AdjustCrlf, out string body);
    UpdateBody(body);
  }
  #endregion

  #region Editor Text & Style Changed
  private int _editorCounter = 0;
  private const int EditorCounterMax = 50;
  private void View_TextEditorRichEditBox_TextChanged(object sender, RoutedEventArgs e)
  {
    _editorInputDebounceTimer.Stop();
    if (_editorCounter++ > EditorCounterMax)
    {
      _editorCounter = 0;
      View_TextEditorRichEditBox.Document.GetText(TextGetOptions.AdjustCrlf, out string body);
      UpdateBody(body);
    }
    else
      _editorInputDebounceTimer.Start();
  }

  private void UpdateBody(string body) => ViewModel.UpdateBody(body);

  private void View_TextEditorRichEditBox_SelectionChanged(object sender, RoutedEventArgs e)
  {
    //View_TextEditorRichEditBox.Document.Selection.GetRect(PointOptions.IncludeInset, out Rect rect, out _);
    //Debug.WriteLine($"{rect.X}, {rect.Width}, {rect.Y}, {rect.Height}");
    var selectedText = View_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
    {
      var characterFormat = selectedText.CharacterFormat;
      View_BoldButton.IsChecked = characterFormat.Bold == FormatEffect.On;
      View_ItalicButton.IsChecked = characterFormat.Italic == FormatEffect.On;
      View_UnderlineButton.IsChecked = characterFormat.Underline == UnderlineType.Single;
      View_StrikethroughButton.IsChecked = characterFormat.Strikethrough == FormatEffect.On;
    }
  }

  private void View_BoldButton_Click(object sender, RoutedEventArgs e)
  {
    var selectedText = View_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
      selectedText.CharacterFormat.Bold = FormatEffect.Toggle;
  }

  private void View_ItalicButton_Click(object sender, RoutedEventArgs e)
  {
    var selectedText = View_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
      selectedText.CharacterFormat.Italic = FormatEffect.Toggle;
  }

  private void View_UnderlineButton_Click(object sender, RoutedEventArgs e)
  {
    var selectedText = View_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
    {
      if (selectedText.CharacterFormat.Underline == UnderlineType.None)
        selectedText.CharacterFormat.Underline = UnderlineType.Single;
      else
        selectedText.CharacterFormat.Underline = UnderlineType.None;
    }
  }

  private void View_StrikethroughButton_Click(object sender, RoutedEventArgs e)
  {
    var selectedText = View_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
      selectedText.CharacterFormat.Strikethrough = FormatEffect.Toggle;
  }

  private void View_MarkerButton_Click(object sender, RoutedEventArgs e)
  {
    var selectedText = View_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
    {
      MarkerType markerType = View_MarkerFlyoutGridView.SelectedIndex switch
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
  private void View_FontSizeButtonFlyout_Opened(object sender, object e)
  {
    var selectedText = View_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
    {
      var startPosition = selectedText.StartPosition;
      var textRange = View_TextEditorRichEditBox.Document.GetRange(startPosition, startPosition + 1);
      _preventComboBoxSelectionChanging = true;
      View_FontSizeComboBox.SelectedItem = textRange.CharacterFormat.Size.ToString();
      _preventComboBoxSelectionChanging = false;
    }
  }

  private void View_FontSizeDownButton_Click(object sender, RoutedEventArgs e)
  {
    View_FontSizeComboBox.SelectedIndex -= 1;
  }

  private void View_FontSizeUpButton_Click(object sender, RoutedEventArgs e)
  {
    View_FontSizeComboBox.SelectedIndex += 1;
  }

  const int FontSizeComboBoxItemsCount = 12;
  private void View_FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    var selectedText = View_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null && !_preventComboBoxSelectionChanging)
    {
      if (float.TryParse((string)View_FontSizeComboBox.SelectedItem, out var fontSize))
        selectedText.CharacterFormat.Size = fontSize;
      else
        selectedText.CharacterFormat.Size = 10.5f;
    }

    View_FontSizeDownButton.IsEnabled = View_FontSizeComboBox.SelectedIndex != 0;
    View_FontSizeUpButton.IsEnabled = View_FontSizeComboBox.SelectedIndex != FontSizeComboBoxItemsCount;

  }

  private void View_FontColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
  {
    View_FontColorButtonFillFontIcon.Foreground = new SolidColorBrush(args.NewColor);
  }

  private void View_FontColorButton_Click(object sender, RoutedEventArgs e)
  {
    var selectedText = View_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
    {
      selectedText.CharacterFormat.ForegroundColor = View_FontColorPicker.Color;
    }
  }

  private void View_HighlightColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
  {
    View_HighlightButtonFillFontIcon.Foreground = new SolidColorBrush(args.NewColor);
  }


  private void View_HighlightButton_Click(object sender, RoutedEventArgs e)
  {
    var selectedText = View_TextEditorRichEditBox.Document.Selection;
    if (selectedText is not null)
    {
      selectedText.CharacterFormat.BackgroundColor = View_HighlightColorPicker.Color;
    }
  }
  #endregion

  private void View_ViewModeEditRadioMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    => ChangeViewMode(true);

  private void View_ViewModeReadRadioMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    => ChangeViewMode(false);

  public void ChangeViewMode(bool isEditMode)
  {
    View_TextEditorRichEditBox.IsReadOnly = !isEditMode;
    View_BoldButton.IsEnabled = isEditMode;
    View_ItalicButton.IsEnabled = isEditMode;
    View_UnderlineButton.IsEnabled = isEditMode;
    View_StrikethroughButton.IsEnabled = isEditMode;
    View_MarkerButton.IsEnabled = isEditMode;
    View_FontSizeButton.IsEnabled = isEditMode;
    View_FontColorButton.IsEnabled = isEditMode;
    View_HighlightButton.IsEnabled = isEditMode;
  }

  #region Messengers
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<Message<bool>, string>(this, Tokens.ToggleNoteWindowActivation, new((recipient, message) =>
    {
      if (message.Sender == ViewModel.Note)
      {
        if (message.Content)
          VisualStateManager.GoToState(this, "WindowActivated", false);
        else
        {
          View_TitleBarGrid.Focus(FocusState.Programmatic);
          VisualStateManager.GoToState(this, "WindowDeactivated", false);
        }
      }
    }));
  }

  private void UnregisterMessengers() => WeakReferenceMessenger.Default.UnregisterAll(this);
  #endregion

  // Debbuging: Reference Tracker
  private void View_ReferenceTrackerMenuFlyoutItem_Click(object sender, RoutedEventArgs e) => ReferenceTracker.ShowReferences();
}
