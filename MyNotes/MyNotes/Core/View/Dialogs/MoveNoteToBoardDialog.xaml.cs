using MyNotes.Core.Model;
using MyNotes.Core.Service;

namespace MyNotes.Core.View;
internal sealed partial class MoveNoteToBoardDialog : ContentDialog
{
  public MoveNoteToBoardDialog(DialogService.NavigationTreeDialogViewModel viewModel)
  {
    InitializeComponent();
    ViewModel = viewModel;
  }

  public DialogService.NavigationTreeDialogViewModel ViewModel { get; }
}

internal class NavigationUserBoardTreeViewItemTemplateSelector : DataTemplateSelector
{
  public DataTemplate? UserGroupTemplate { get; set; }
  public DataTemplate? UserBoardTemplate { get; set; }
  protected override DataTemplate? SelectTemplateCore(object item) => item switch
  {
    NavigationUserGroup => UserGroupTemplate,
    NavigationUserBoard => UserBoardTemplate,
    _ => throw new ArgumentException("")
  };
}
