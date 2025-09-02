using MyNotes.Core.Model;
using MyNotes.Core.ViewModel;

using Windows.ApplicationModel;

using static MyNotes.Common.Interop.NativeMethods;

namespace MyNotes.Core.View;

internal sealed partial class NoteWindow : Window
{
  public NoteWindow(Note note)
  {
    this.InitializeComponent();
    this.ExtendsContentIntoTitleBar = true;

    string iconPath = Path.Combine(Package.Current.InstalledLocation.Path, "Assets/icons/app/AppIcon_128.ico");
    AppWindow.SetIcon(iconPath);

    View_NotePage.ViewModel = App.Instance.GetService<NoteViewModelFactory>().Resolve(note);
  }
}
