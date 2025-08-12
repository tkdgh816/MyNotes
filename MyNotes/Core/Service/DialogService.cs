using MyNotes.Core.Model;
using MyNotes.Core.View;
using MyNotes.Core.ViewModel;

namespace MyNotes.Core.Service;

internal class DialogService
{
  public DialogService()
  { }

  private XamlRoot? _mainXamlRoot;
  private ContentDialog? _mainDialog;

  public void SetMainXamlRoot(XamlRoot xamlRoot) => _mainXamlRoot = xamlRoot;

  public async void ShowEditNoteTagsDialog(NoteViewModel noteViewModel)
  {
    if (noteViewModel is not null)
    {
      var dialog = new EditNoteTagsDialog(noteViewModel) { XamlRoot = _mainXamlRoot };
      await dialog.ShowAsync();
    }
  }

  public async void ShowRenameNoteTitleDialog(NoteViewModel noteViewModel)
  {
    if (noteViewModel is not null)
    {
      var dialog = new RenameNoteTitleDialog(noteViewModel) { XamlRoot = _mainXamlRoot };
      await dialog.ShowAsync();
    }
  }

  public async void ShowNoteInformationDialog(NoteViewModel noteViewModel, XamlRoot? xamlRoot = null)
  {
    if (noteViewModel is not null)
    {
      xamlRoot ??= _mainXamlRoot;
      var dialog = new NoteInformationDialog(noteViewModel) { XamlRoot = xamlRoot };
      await dialog.ShowAsync();
    }
  }

  public async Task<(bool DialogResult, Icon? Icon, string? Name)> ShowRenameBoardDialog(NavigationUserBoard board)
  {
    NavigationDialogViewModel viewModel = new(board.Icon, board.Name);
    (bool, Icon?, string?) result;

    var dialog = new RenameBoardDialog(viewModel) { XamlRoot = _mainXamlRoot };
    if (await dialog.ShowAsync() == ContentDialogResult.Primary)
    {
      Icon? icon = viewModel.Icon == board.Icon ? null : viewModel.Icon;
      string? name = viewModel.Name == board.Name ? null : viewModel.Name;
      result = (true, icon, name);
    }
    else
      result = (false, null, null);
    return result;
  }

  public async Task<bool> ShowDeleteBoardDialog(NavigationUserBoard board)
  {
    var dialog = new DeleteBoardDialog(board) { XamlRoot = _mainXamlRoot };
    return await dialog.ShowAsync() == ContentDialogResult.Primary;
  }

  public async Task<(bool DialogResult, Icon? Icon, string? Name)> ShowCreateBoardDialog()
  {
    NavigationDialogViewModel viewModel = new(IconManager.FindGlyph("\uE70B"), "New Board");
    (bool, Icon?, string?) result;
    var dialog = new CreateBoardDialog(viewModel) { XamlRoot = _mainXamlRoot };
    if (await dialog.ShowAsync() == ContentDialogResult.Primary)
      result = (true, viewModel.Icon, viewModel.Name);
    else
      result = (false, null, null);
    return result;
  }

  public async Task<(bool DialogResult, Icon? Icon, string? Name)> ShowCreateBoardGroupDialog()
  {
    NavigationDialogViewModel viewModel = new(IconManager.FindGlyph("\uE82D"), "New Group");
    (bool, Icon?, string?) result;
    var dialog = new CreateBoardDialog(viewModel) { XamlRoot = _mainXamlRoot };
    if (await dialog.ShowAsync() == ContentDialogResult.Primary)
      result = (true, viewModel.Icon, viewModel.Name);
    else
      result = (false, null, null);
    return result;
  }

  public async Task<bool> ShowRemoveNoteDialog(int noteCount)
  {
    var dialog = new RemoveNoteDialog(noteCount) { XamlRoot = _mainXamlRoot };
    return await dialog.ShowAsync() == ContentDialogResult.Primary;
  }

  public async Task<(bool DialogResult, BoardId? Id)> ShowMoveNoteToBoardDialog()
  {
    var mainViewModel = App.Instance.GetService<MainViewModel>();
    NavigationTreeDialogViewModel viewModel = new(mainViewModel.UserRootNavigation);
    (bool, BoardId?) result;
    var dialog = new MoveNoteToBoardDialog(viewModel) { XamlRoot = _mainXamlRoot };
    if (await dialog.ShowAsync() == ContentDialogResult.Primary && viewModel.SelectedNavigation is not null)
      result = (true, viewModel.SelectedNavigation.Id);
    else
      result = (false, null);
    return result;
  }

  public class NavigationDialogViewModel : ViewModelBase
  {
    private Icon _icon;
    public Icon Icon
    {
      get => _icon;
      set => SetProperty(ref _icon, value);
    }

    private string _name;
    public string Name
    {
      get => _name;
      set => SetProperty(ref _name, value);
    }

    public NavigationDialogViewModel(Icon icon, string name)
    {
      _icon = icon;
      _name = name;
    }
  }

  public class NavigationTreeDialogViewModel : ViewModelBase
  {
    public IReadOnlyList<NavigationUserBoard> Navigations { get; }

    private NavigationUserBoard? _selectedNavigation = null;
    public NavigationUserBoard? SelectedNavigation
    {
      get => _selectedNavigation;
      set => SetProperty(ref _selectedNavigation, value);
    }

    public NavigationTreeDialogViewModel(NavigationUserRootGroup rootNavigation)
    {
      Navigations = rootNavigation.Children;
    }
  }
}
