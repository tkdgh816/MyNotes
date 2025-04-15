using MyNotes.Core.Models;
using MyNotes.Core.ViewModels;

namespace MyNotes.Core.Views;

public sealed partial class NavigationEditor : UserControl
{
  public NavigationEditor()
  {
    this.InitializeComponent();
    ViewModel = App.Current.GetService<MainViewModel>();
  }

  public MainViewModel ViewModel { get; }
}

public class NavigationEditorTreeViewItemTemplateSelector : DataTemplateSelector
{
  public DataTemplate? GroupItemTemplate { get; set; }
  public DataTemplate? BoardItemTemplate { get; set; }

  protected override DataTemplate? SelectTemplateCore(object item)
  {
    Debug.WriteLine(item.GetType());
    return item switch
    {
      NavigationGroupItem => GroupItemTemplate,
      NavigationBoardItem => BoardItemTemplate,
      _ => null
    };
  }
}