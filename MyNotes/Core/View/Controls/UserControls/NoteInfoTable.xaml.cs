using MyNotes.Core.Model;

namespace MyNotes.Core.View;

internal sealed partial class NoteInfoTable : UserControl
{
  public NoteInfoTable()
  {
    this.InitializeComponent();
  }

  public static readonly DependencyProperty NoteProperty = DependencyProperty.Register("Note", typeof(Note), typeof(NoteInfoTable), new PropertyMetadata(null));
  public Note Note
  {
    get => (Note)GetValue(NoteProperty);
    set => SetValue(NoteProperty, value);
  }
}
