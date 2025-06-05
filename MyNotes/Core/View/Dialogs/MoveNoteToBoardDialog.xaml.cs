using MyNotes.Core.Model;
using MyNotes.Core.ViewModel;

namespace MyNotes.Core.View;
internal sealed partial class MoveNoteToBoardDialog : ContentDialog
{
  public MoveNoteToBoardDialog()
  {
    InitializeComponent();
    MainViewModel = ((App)Application.Current).GetService<MainViewModel>();
  }

  public MainViewModel MainViewModel { get; }
}

internal class NavigationItemTreeViewItemTemplateSelector : DataTemplateSelector
{
  public DataTemplate? GroupItem { get; set; }
  public DataTemplate? BoardItem { get; set; }

  protected override DataTemplate? SelectTemplateCore(object item)
  {
    return item switch
    {
      NavigationUserGroup => GroupItem,
      NavigationUserBoard => BoardItem,
      _ => null
    };
  }
}