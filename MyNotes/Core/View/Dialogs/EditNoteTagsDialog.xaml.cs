using MyNotes.Core.Model;
using MyNotes.Core.Service;
using MyNotes.Core.ViewModel;

namespace MyNotes.Core.View;

internal sealed partial class EditNoteTagsDialog : ContentDialog
{
  public EditNoteTagsDialog(NoteViewModel noteViewModel)
  {
    InitializeComponent();
    ViewModel = noteViewModel;
    _tagService = App.Instance.GetService<TagService>();

    this.Unloaded += EditNoteTagsDialog_Unloaded;
  }

  private void EditNoteTagsDialog_Unloaded(object sender, RoutedEventArgs e) => this.Bindings.StopTracking();

  public NoteViewModel ViewModel { get; }
  private readonly TagService _tagService;

  private bool _preventDefaultTagColorChange = false;
  private TagColor _defaultTagColor = TagColor.White;

  private void View_AddTagAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
  {
    string query = sender.Text.Trim();
    if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
    {
      if (string.IsNullOrEmpty(query))
      {
        sender.ItemsSource = null;
        RevertTagColor();
      }
      else
      {
        var suggestions = _tagService.Tags
          .Where(tag => tag.Text.Contains(query, StringComparison.CurrentCultureIgnoreCase))
          .Select(tag => tag.Text);
        sender.ItemsSource = suggestions;
      }
    }

    FixTagColor(query);
  }

  private void FixTagColor(string tagText)
  {
    var tag = _tagService.Tags.FirstOrDefault(tag => tag.Text == tagText);
    if (tag is null)
    {
      RevertTagColor();
      return;
    }

    _preventDefaultTagColorChange = true;
    View_TagColorSelector.TagColor = tag.Color;
    View_TagColorSelector.IsEnabled = false;
  }

  private void RevertTagColor()
  {
    _preventDefaultTagColorChange = false;
    View_TagColorSelector.TagColor = _defaultTagColor;
    View_TagColorSelector.IsEnabled = true;
  }

  private void View_TagColorSelector_TagColorChanged(TagColorSelector sender, TagColorChangedEventArgs args)
  {
    if (!_preventDefaultTagColorChange)
      _defaultTagColor = args.NewColor;
  }

  private void AppTagButton_DeleteButtonClick(object sender, RoutedEventArgs e)
  {
    ViewModel.DeleteNoteTagCommand?.Execute((Tag)((FrameworkElement)sender).DataContext);
  }
}
