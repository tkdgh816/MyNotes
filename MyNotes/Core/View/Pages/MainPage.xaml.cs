using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Common.Messaging;
using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Core.Shared;
using MyNotes.Core.ViewModel;
using MyNotes.Debugging;

using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;

namespace MyNotes.Core.View;

internal sealed partial class MainPage : Page
{
  public MainViewModel ViewModel { get; }
  private readonly NavigationService _navigationService;
  private readonly DialogService _dialogService;
  private readonly SettingsService _settingsService;

  public MainPage()
  {
    this.InitializeComponent();
    ViewModel = App.Instance.GetService<MainViewModel>();

    _navigationService = App.Instance.GetService<NavigationService>();
    _dialogService = App.Instance.GetService<DialogService>();
    _settingsService = App.Instance.GetService<SettingsService>();

    InitializeSettings();
    RegisterEvents();
    RegisterMessengers();
    this.Loaded += MainPage_Loaded;
    this.Unloaded += MainPage_Unloaded;
  }

  private void MainPage_Loaded(object sender, RoutedEventArgs e)
  {
    // 서비스에 뷰 내부 컨트롤 전달
    _navigationService.AttachView(View_NavigationView, View_NavigationContent_RootFrame);
    _dialogService.SetMainXamlRoot(this.XamlRoot);
  }

  private void MainPage_Unloaded(object sender, RoutedEventArgs e)
  {
    _navigationService.DetachView();

    ViewModel.Dispose();
    UnegisterEvents();
    UnregisterMessengers();
  }

  private void InitializeSettings()
  {
    ChangeTheme((AppTheme)_settingsService.GlobalSettings[AppSettingsKeys.AppTheme]);
  }

  private void ChangeTheme(AppTheme theme) 
    => this.RequestedTheme = theme switch
    {
      AppTheme.Light => ElementTheme.Light,
      AppTheme.Dark => ElementTheme.Dark,
      AppTheme.Default or _ => ElementTheme.Default
    };

  private void ChangeNavigationViewPaneDisplayMode(NavigationViewPaneDisplayMode paneDisplayMode)
    => View_NavigationView.PaneDisplayMode = paneDisplayMode;


  #region Handling Events
  private void RegisterEvents()
  {
    _navigationHoldingTimer.Interval = TimeSpan.FromMilliseconds(TimerIntervalMilliseconds);
    _navigationHoldingTimer.Tick += OnTimerTick;
  }

  private void UnegisterEvents()
  {
    _navigationHoldingTimer.Tick -= OnTimerTick;
  }
  #endregion

  private void View_TitleBar_PaneToggleRequested(TitleBar sender, object args)
    => View_NavigationView.IsPaneOpen = !View_NavigationView.IsPaneOpen;

  #region Navigation
  private void View_NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
  {
    if (args.SelectedItem is NavigationItem navigation)
      ViewModel.NavigateCommand?.Execute(navigation);
  }
  #endregion

  #region 각 내비게이션 DataTemplate에 Contex Menu 동작
  // 새 내비게이션 추가
  private void Template_Group_NewGroupMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    => ViewModel.ShowNewGroupDialogCommand?.Execute((NavigationUserGroup)((FrameworkElement)sender).DataContext);

  private void Template_Group_NewBoardMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    => ViewModel.ShowNewBoardDialogCommand?.Execute((NavigationUserGroup)((FrameworkElement)sender).DataContext);

  // 내비게이션 이름 변경
  private void Template_Board_RenameMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    if (((FrameworkElement)sender).DataContext is NavigationUserBoard navigation)
      ViewModel.ShowRenameBoardDialogCommand?.Execute(navigation);
  }

  // 내비게이션 제거
  private void Template_Board_DeleteMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    if (((FrameworkElement)sender).DataContext is NavigationUserBoard navigation)
      ViewModel.ShowDeleteBoardDialogCommand?.Execute(navigation);
  }
  #endregion

  // 태그 내비게이션 클릭 시 TagsEditor 플라이아웃 띄우기
  private void View_NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
  {
    if (args.InvokedItemContainer is NavigationViewItem itemContainer
        && !itemContainer.SelectsOnInvoked
        && itemContainer.DataContext is NavigationItem item)
    {
      switch (item)
      {
        case NavigationTags:
          View_TagsFlyout.ShowAt(itemContainer);
          WeakReferenceMessenger.Default.Send(new Message(), Tokens.RefreshTagGroup);
          break;
        case NavigationUserGroup:
          break;
        case NavigationUserBoard:
          View_EditTeachingTip.IsOpen = true;
          break;
      }
    }
  }

  #region Timers
  private readonly DispatcherTimer _navigationHoldingTimer = new();
  private const int TimerIntervalMilliseconds = 400;
  #endregion

  #region Messengers
  private void RegisterMessengers() 
  {
    WeakReferenceMessenger.Default.Register<Message<AppTheme>, string>(this, Tokens.ChangeTheme, new((recipient, message) => ChangeTheme(message.Content)));
  }

  private void UnregisterMessengers() => WeakReferenceMessenger.Default.UnregisterAll(this);
  #endregion

  #region 뷰 내부에서 User Navigation 이동
  private NavigationUserBoard? _draggingSourceBoard;
  private AppNavigationViewItem? _draggingSourceItem;
  private bool _isDraggingSourceItemExpanded;

  private NavigationViewItem? _hoveredTargetGroupItem;
  private bool _isInsertingIntoHoveredTargetGroup;

  private const double HoveredTargetItemUpperPosition = 10.0;
  private const double HoveredTargetItemLowerPostion = 25.0;

  private async void Template_Board_AppNavigationViewItem_DragStarting(UIElement sender, DragStartingEventArgs args)
  {
    // Dragging Source 저장
    _draggingSourceItem = (AppNavigationViewItem)sender;
    _draggingSourceBoard = (NavigationUserBoard)_draggingSourceItem.DataContext;

    // Dragging Source의 확장 상태 저장, 접힌 상태로 이동
    _isDraggingSourceItemExpanded = _draggingSourceItem.IsExpanded;
    _draggingSourceItem.IsExpanded = false;
    _draggingSourceItem.AllowHoverOverlay = false;

    // DataPackage 동작과 전달 데이터 지정
    args.AllowedOperations = DataPackageOperation.Move;
    args.Data.SetData(DataFormats.Navigation, _draggingSourceBoard.Id.Value.ToString());

    // DragUI를 Dragging Source 캡처 이미지로 지정
    DragOperationDeferral defferal = args.GetDeferral();
    RenderTargetBitmap renderTargetBitmap = new();
    await renderTargetBitmap.RenderAsync(_draggingSourceItem);

    var pixels = await renderTargetBitmap.GetPixelsAsync();

    SoftwareBitmap softwareBitmap = new(BitmapPixelFormat.Bgra8, renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight, BitmapAlphaMode.Premultiplied);
    softwareBitmap.CopyFromBuffer(pixels);
    args.DragUI.SetContentFromSoftwareBitmap(softwareBitmap, new Point(0, 0));
    defferal.Complete();
  }

  private void Template_Board_AppNavigationViewItem_DragEnter(object sender, DragEventArgs e)
  {
    var targetItem = (AppNavigationViewItem)sender;
    var targetBoard = (NavigationUserBoard)targetItem.DataContext;
    if (targetBoard is NavigationUserGroup)
      _hoveredTargetGroupItem = targetItem;

    e.DragUIOverride.IsCaptionVisible = true;
    foreach (string format in e.DataView.AvailableFormats)
    {
      switch (format)
      {
        case DataFormats.Navigation:
          e.AcceptedOperation = DataPackageOperation.Move;
          break;
        case DataFormats.Note:
          if (targetBoard is NavigationUserGroup)
          {
            _isInsertingIntoHoveredTargetGroup = !_isInsertingIntoHoveredTargetGroup;
            _navigationHoldingTimer.Start();
          }
          else
          {
            e.AcceptedOperation = DataPackageOperation.Move;
            e.DragUIOverride.Caption = $"Move note to {targetBoard.Name}";
          }
          break;
      }
    }

    e.Handled = true;
  }

  private void Template_Board_AppNavigationViewItem_DragOver(object sender, DragEventArgs e)
  {
    var targetItem = (AppNavigationViewItem)sender;
    var targetBoard = (NavigationUserBoard)targetItem.DataContext;
    var targetParent = targetBoard.Parent!;

    foreach (string format in e.DataView.AvailableFormats)
    {
      switch (format)
      {
        case DataFormats.Navigation:
          if (_draggingSourceBoard != targetBoard)
          {
            double position = e.GetPosition(targetItem).Y;
            // 드래그 위치: 상 -> 타겟 앞으로, 하 & 타겟이 그룹의 마지막 -> 그룹 뒤로, 나머지 -> 타겟 앞으로 이동
            if (position <= HoveredTargetItemUpperPosition)
              ViewModel.MoveUserNavigation(_draggingSourceBoard!, targetBoard, NavigationBoardItemPosition.Before);
            else if (position >= HoveredTargetItemLowerPostion && targetParent.IndexOfChild(targetBoard) >= targetParent.ChildCount - 2)
              ViewModel.MoveUserNavigation(_draggingSourceBoard!, targetParent, NavigationBoardItemPosition.After);
            else
              ViewModel.MoveUserNavigation(_draggingSourceBoard!, targetBoard, NavigationBoardItemPosition.After);
          }
          break;
        case DataFormats.Note:
          break;
      }
    }

    e.Handled = true;
  }

  private void Template_Group_AppNavigationViewItem_DragOver(object sender, DragEventArgs e)
  {
    var targetItem = (AppNavigationViewItem)sender;
    var targetGroup = (NavigationUserGroup)targetItem.DataContext;

    foreach (string format in e.DataView.AvailableFormats)
    {
      switch (format)
      {
        case DataFormats.Navigation:
          if (_draggingSourceBoard != targetGroup)
          {
            double position = e.GetPosition(targetItem).Y;
            // 드래그 위치: 상 -> 타겟 그룹 앞으로, 중 -> 타겟 그룹 안으로, 하 -> 타겟 그룹 뒤로 이동
            if (position <= HoveredTargetItemUpperPosition)
            {
              _isInsertingIntoHoveredTargetGroup = false;
              _navigationHoldingTimer.Start();
              ViewModel.MoveUserNavigation(_draggingSourceBoard!, targetGroup, NavigationBoardItemPosition.Before);
            }
            else if (position >= HoveredTargetItemLowerPostion)
            {
              _isInsertingIntoHoveredTargetGroup = false;
              _navigationHoldingTimer.Start();
              ViewModel.MoveUserNavigation(_draggingSourceBoard!, targetGroup, NavigationBoardItemPosition.After);
            }
            else
            {
              _isInsertingIntoHoveredTargetGroup = true;
              _navigationHoldingTimer.Start();
              targetGroup.InsertChild(0, _draggingSourceBoard!);
            }
          }
          break;
        case DataFormats.Note:
          break;
      }
    }

    e.Handled = true;
  }

  private void Template_Board_AppNavigationViewItem_DragLeave(object sender, DragEventArgs e)
  {
    _navigationHoldingTimer.Stop();
    e.Handled = true;
  }

  private async void Template_Board_AppNavigationViewItem_Drop(object sender, DragEventArgs e)
  {
    var targetItem = (AppNavigationViewItem)sender;
    var targetBoard = (NavigationUserBoard)targetItem.DataContext;

    foreach (string format in e.DataView.AvailableFormats)
    {
      switch (format)
      {
        case DataFormats.Navigation:
          _navigationHoldingTimer.Stop();
          if (_hoveredTargetGroupItem is not null && _isInsertingIntoHoveredTargetGroup && !_hoveredTargetGroupItem.IsExpanded)
          {
            _hoveredTargetGroupItem.LayoutUpdated += hoveredTargetGroupItem_LayoutUpdated;
            _hoveredTargetGroupItem.IsExpanded = true;
          }
          else
          {
            RevertDraggedNavigationViewItemUI();
            _hoveredTargetGroupItem = null;
          }
          break;
        case DataFormats.Note:
          string id = (string)await e.DataView.GetDataAsync(format);
          ViewModel.MoveNoteToBoard(new NoteId(new Guid(id)), targetBoard.Id);
          break;
        case var _ when format == StandardDataFormats.Text:
          Debug.WriteLine($"{format}: {await e.DataView.GetDataAsync(format)}");
          break;
      }
    }
  }

  private void hoveredTargetGroupItem_LayoutUpdated(object? sender, object e)
  {
    RevertDraggedNavigationViewItemUI();
    _hoveredTargetGroupItem!.LayoutUpdated -= hoveredTargetGroupItem_LayoutUpdated;
    _hoveredTargetGroupItem = null;
  }

  private void RevertDraggedNavigationViewItemUI()
  {
    if (_draggingSourceItem is not null)
    {
      _draggingSourceItem.IsExpanded = _isDraggingSourceItemExpanded;
      _draggingSourceItem.AllowHoverOverlay = true;
      _draggingSourceItem = null;
    }
  }

  private void OnTimerTick(object? sender, object e)
  {
    _navigationHoldingTimer.Stop();
    if (_hoveredTargetGroupItem != null)
      _hoveredTargetGroupItem.IsExpanded = _isInsertingIntoHoveredTargetGroup;
  }
  #endregion

  //TEST: View Windows And Pages
  private void ViewWindowsButton_Click(object sender, RoutedEventArgs e) => ReferenceTracker.ShowReferences();
}

internal class MainNavigationViewMenuItemTemplateSelector : DataTemplateSelector
{
  public DataTemplate? MenuItemTemplate { get; set; }
  public DataTemplate? InvokableMenuItemTemplate { get; set; }
  public DataTemplate? SeparatorTemplate { get; set; }
  public DataTemplate? UserGroupMenuItemTemplate { get; set; }
  public DataTemplate? UserBoardMenuItemTemplate { get; set; }
  protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) => item switch
  {
    NavigationUserGroup => UserGroupMenuItemTemplate,
    NavigationUserBoard => UserBoardMenuItemTemplate,
    NavigationTags => InvokableMenuItemTemplate,
    NavigationItem => MenuItemTemplate,
    NavigationSeparator => SeparatorTemplate,
    _ => throw new ArgumentException("")
  };
}