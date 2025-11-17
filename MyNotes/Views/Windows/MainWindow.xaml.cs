namespace MyNotes.Views.Windows;

public sealed partial class MainWindow : Window
{
  private readonly IntPtr _hWnd;
  private readonly OverlappedPresenter? _presenter;

  public MainWindow()
  {
    InitializeComponent();
    this.ExtendsContentIntoTitleBar = true;
    this.SetTitleBar(MainWindow_TitleBarGrid);

    // DPI 스케일 가져오기
    _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
    double scaleFactor = Common.Interop.NativeMethods.GetWindowScaleFactor(_hWnd);

    // 창 최소 크기 지정
    _presenter = AppWindow.Presenter as OverlappedPresenter;
    _presenter?.PreferredMinimumWidth = (int)(600 * scaleFactor);
    _presenter?.PreferredMinimumHeight = (int)(300 * scaleFactor);

    // 높은(48epx) 캡션 컨트롤 지원
    AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;

    // 창 초기 크기 지정
    AppWindow.ResizeClient(new((int)(600 * scaleFactor), (int)(800 * scaleFactor)));


    // 타이틀 바에 캡션 컨트롤 여백 및 드래그 제외 영역 지정
    MainWindow_TitleBarGrid.Loaded += MainWindow_TitleBarGrid_Loaded;
    this.SizeChanged += MainWindow_SizeChanged;

    // 창 활성화 변경 시
    this.Activated += MainWindow_Activated;
    this.Closed += (s, e) => this.Activated -= MainWindow_Activated;
  }

  private void MainWindow_TitleBarGrid_Loaded(object sender, RoutedEventArgs e)
  {
    SetRegionsForCustomTitleBar();
  }

  private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
  {
    SetRegionsForCustomTitleBar();
  }

  private void SetRegionsForCustomTitleBar()
  {
    if (MainWindow_TitleBarGrid.XamlRoot is XamlRoot xamlRoot)
    {
      double scaleFactor = xamlRoot.RasterizationScale;

      RightPaddingColumn.Width = new GridLength(AppWindow.TitleBar.RightInset / scaleFactor);
      LeftPaddingColumn.Width = new GridLength(AppWindow.TitleBar.LeftInset / scaleFactor);

      var BackButtonPosition = MainWindow_BackButton.TransformToVisual(null).TransformBounds(new Rect(0, 0, MainWindow_BackButton.ActualWidth, MainWindow_BackButton.ActualHeight));
      var PaneToggleButtonPosition = MainWindow_PaneToggleButton.TransformToVisual(null).TransformBounds(new Rect(0, 0, MainWindow_PaneToggleButton.ActualWidth, MainWindow_PaneToggleButton.ActualHeight));
      var SearchBoxPosition = MainWindow_SearchAutoSuggestBox.TransformToVisual(null).TransformBounds(new Rect(0, 0, MainWindow_SearchAutoSuggestBox.ActualWidth, MainWindow_SearchAutoSuggestBox.ActualHeight));

      RectInt32 BackButtonRect = GetRect(BackButtonPosition, scaleFactor);
      RectInt32 PaneToggleButtonRect = GetRect(PaneToggleButtonPosition, scaleFactor);
      RectInt32 SearchBoxRect = GetRect(SearchBoxPosition, scaleFactor);

      InputNonClientPointerSource inputNonClientPointerSource = InputNonClientPointerSource.GetForWindowId(AppWindow.Id);
      inputNonClientPointerSource.SetRegionRects(NonClientRegionKind.Passthrough, [BackButtonRect, PaneToggleButtonRect, SearchBoxRect]);
    }
  }

  private RectInt32 GetRect(Rect bounds, double scale) =>
    new(
      _X: (int)Math.Round(bounds.X * scale),
      _Y: (int)Math.Round(bounds.Y * scale),
      _Width: (int)Math.Round(bounds.Width * scale),
      _Height: (int)Math.Round(bounds.Height * scale)
    );

  private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
  {
    if (args.WindowActivationState == WindowActivationState.Deactivated)
    {
      InputNonClientPointerSource inputNonClientPointerSource = InputNonClientPointerSource.GetForWindowId(AppWindow.Id);
      inputNonClientPointerSource.SetRegionRects(NonClientRegionKind.Passthrough, null);
      VisualStateManager.GoToState(MainWindow_RootControl, "WindowDeactivated", false);
    }
    else
    {
      SetRegionsForCustomTitleBar();
      VisualStateManager.GoToState(MainWindow_RootControl, "WindowActivated", false);
    }
  }

  private void MainWindow_BackButton_Click(object sender, RoutedEventArgs e)
  {
    if (MainWindow_NavigationFrame.CanGoBack)
      MainWindow_NavigationFrame.GoBack();
  }

  private void MainWindow_PaneToggleButton_Click(object sender, RoutedEventArgs e)
  {
    MainWindow_NavigationView.IsPaneOpen = !MainWindow_NavigationView.IsPaneOpen;
  }
}
