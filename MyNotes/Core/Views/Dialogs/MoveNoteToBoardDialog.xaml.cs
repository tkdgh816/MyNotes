using MyNotes.Core.Models;
using MyNotes.Core.ViewModels;

namespace MyNotes.Core.Views;
public sealed partial class MoveNoteToBoardDialog : ContentDialog
{
  public MoveNoteToBoardDialog()
  {
    InitializeComponent();
    MainViewModel = ((App)Application.Current).GetService<MainViewModel>();
  }

  public MainViewModel MainViewModel { get; }
}

public class NavigationItemTreeViewItemTemplateSelector : DataTemplateSelector
{
  public DataTemplate? GroupItem { get; set; }
  public DataTemplate? BoardItem { get; set; }

  protected override DataTemplate? SelectTemplateCore(object item)
  {
    return item switch
    {
      NavigationBoardGroup => GroupItem,
      NavigationBoard => BoardItem,
      _ => null
    };
  }
}