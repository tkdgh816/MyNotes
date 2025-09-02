using MyNotes.Common.Commands;
using MyNotes.Core.Model;

namespace MyNotes.Core.ViewModel;

internal partial class MainViewModel : ViewModelBase
{
  public Command<NavigationItem>? NavigateCommand { get; private set; }
  public Command<string>? SearchNavigationCommand { get; private set; }
  public Command? GoBackCommand { get; private set; }
  public Command? SetNavigationEditModeCommand { get; private set; }
  public Command<NavigationUserGroup>? ShowNewBoardDialogCommand { get; private set; }
  public Command<NavigationUserGroup>? ShowNewGroupDialogCommand { get; private set; }
  public Command<NavigationUserBoard>? ShowRenameBoardDialogCommand { get; private set; }
  public Command<NavigationUserBoard>? ShowDeleteBoardDialogCommand { get; private set; }

  public void SetCommands()
  {
    NavigateCommand = new(navigation => _navigationService.Navigate(navigation));

    SearchNavigationCommand = new((queryText) =>
    {
      _navigationService.Navigate(new NavigationSearch(queryText));
    });

    GoBackCommand = new(() => _navigationService.GoBack());

    SetNavigationEditModeCommand = new(() =>
    {
      IsEditMode = !IsEditMode;

      foreach (Navigation item in CoreNavigations)
        item.IsEditable = IsEditMode;
      foreach (Navigation item in SecondaryNavigations)
        item.IsEditable = IsEditMode;
      foreach (NavigationUserBoard item in UserNavigations)
        TraverseUserNavigationsAll(item, x => x.IsEditable = IsEditMode);

      if (!IsEditMode)
        UpdateUserNavigationRelations();

      _navigationService.ChangeNavigationViewSelectionWithoutNavigation(IsEditMode ? null : _navigationService.CurrentNavigation);
      _navigationService.ToggleNavigationViewContentEnabled(!IsEditMode);
    });

    ShowNewBoardDialogCommand = new(async (group) =>
    {
      var result = await _dialogService.ShowCreateBoardDialog();
      if (result.DialogResult)
        group.AddChild(new NavigationUserBoard(result.Name!, result.Icon!, new BoardId(Guid.NewGuid())));
    });

    ShowNewGroupDialogCommand = new(async (group) =>
    {
      var result = await _dialogService.ShowCreateBoardGroupDialog();
      if (result.DialogResult)
        group.AddChild(new NavigationUserGroup(result.Name!, result.Icon!, new BoardId(Guid.NewGuid())));
    });

    ShowRenameBoardDialogCommand = new(async (navigation) =>
    {
      var result = await _dialogService.ShowRenameBoardDialog(navigation);
      if (result.DialogResult)
      {
        if (result.Icon is not null)
          navigation.Icon = result.Icon;
        if (result.Name is not null)
          navigation.Name = result.Name;
      }
    });

    ShowDeleteBoardDialogCommand = new(async (navigation) =>
    {
      if (await _dialogService.ShowDeleteBoardDialog(navigation))
        DeleteUserNavigation(navigation);
    });
  }
}
