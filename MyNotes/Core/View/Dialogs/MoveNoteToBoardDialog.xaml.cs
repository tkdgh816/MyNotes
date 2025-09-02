using MyNotes.Core.Model;
using MyNotes.Core.Service;

namespace MyNotes.Core.View;
internal sealed partial class MoveNoteToBoardDialog : ContentDialog
{
  public MoveNoteToBoardDialog(DialogService.NavigationTreeDialogViewModel viewModel)
  {
    InitializeComponent();
    ViewModel = viewModel;

    this.Unloaded += MoveNoteToBoardDialog_Unloaded;
  }

  private void MoveNoteToBoardDialog_Unloaded(object sender, RoutedEventArgs e) => this.Bindings.StopTracking();

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
