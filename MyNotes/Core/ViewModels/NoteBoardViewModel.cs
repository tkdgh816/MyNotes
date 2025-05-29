using CommunityToolkit.Mvvm.Messaging;

using MyNotes.Common.Collections;
using MyNotes.Common.Commands;
using MyNotes.Common.Messaging;
using MyNotes.Core.Models;
using MyNotes.Core.Services;

namespace MyNotes.Core.ViewModels;

public class NoteBoardViewModel : ViewModelBase
{
  public NoteBoardViewModel(DatabaseService databaseService, NavigationService navigationService)
  {
    DatabaseService = databaseService;
    NavigationService = navigationService;
    Navigation = (NavigationNotes)NavigationService.CurrentNavigation!;

    if (Navigation is NavigationBoard navigationBoard)
    {
      foreach (Note note in DatabaseService.GetNotes(navigationBoard))
        NoteViewModels.Add(InitializeNoteViewModel(note));
    }
    else if (Navigation is NavigationBookmarks navigationBookmarks)
    {
      foreach (Note note in DatabaseService.GetBookmarkedNotes())
        NoteViewModels.Add(InitializeNoteViewModel(note));
    }
    NotesCollectionViewSource.Source = NoteViewModels;

    RegisterEvents();
    SetCommands();
    RegisterMessengers();
  }

  public override void Dispose()
  {
    UnregisterEvents();
    GC.SuppressFinalize(this);
  }

  private readonly DatabaseService DatabaseService;
  private readonly NavigationService NavigationService;

  #region Handling Events
  private void RegisterEvents()
  {
    Navigation.PropertyChanged += OnNavigationPropertyChanged;
  }

  private void UnregisterEvents()
  {
    Navigation.PropertyChanged -= OnNavigationPropertyChanged;
  }
  #endregion

  #region Navigation
  public NavigationNotes Navigation { get; }
  private void OnNavigationPropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == "Icon")
      DatabaseService.UpdateBoard((NavigationBoard)Navigation, "Icon");
  }
  #endregion

  #region NoteViewModels: Notes Collection
  public SortedObervableCollection<NoteViewModel> NoteViewModels { get; } = new([new SortDescription<NoteViewModel>(func: vm => vm.Note.Modified, direction: SortDirection.Descending, keyPropertyName: "Modified")]);
  public CollectionViewSource NotesCollectionViewSource { get; } = new();

  static NoteViewModel InitializeNoteViewModel(Note note)
  {
    note.Initialize();
    NoteViewModel noteViewModel = App.Current.GetService<NoteViewModelFactory>().Create(note);
    return noteViewModel;
  }
  #endregion

  #region Sort
  public NoteSortKey SortKey { get; private set; }
  public SortDirection SortDirection { get; private set; }

  private void SortNoteViewModels()
  {
    SortDescription<NoteViewModel> sortDescription = SortKey switch
    {
      NoteSortKey.Created => new(func: vm => vm.Note.Created, direction: SortDirection, keyPropertyName: "Created"),
      NoteSortKey.Title => new(func: vm => vm.Note.Title, direction: SortDirection, keyPropertyName: "Title"),
      NoteSortKey.Modified or _ => new(func: vm => vm.Note.Modified, direction: SortDirection, keyPropertyName: "Modified"),
    };

    NoteViewModels.SortDescriptions.Clear();
    NoteViewModels.SortDescriptions.Add(sortDescription);
    NoteViewModels.Refresh();

    NotesCollectionViewSource.Source = NoteViewModels;
  }
  #endregion

  #region Commands
  public Command<string>? ChangeSortKeyCommand { get; private set; }
  public Command<string>? ChangeSortDirectionCommand { get; private set; }
  private void SetCommands()
  {
    ChangeSortKeyCommand = new((key) =>
    {
      if (Enum.TryParse<NoteSortKey>(key, out var sortKey))
      {
        SortKey = sortKey;
        SortNoteViewModels();
      }
    });
    ChangeSortDirectionCommand = new((direction) =>
    {
      if (Enum.TryParse<SortDirection>(direction, out var sortDirection))
      {
        SortDirection = sortDirection;
        SortNoteViewModels();
      }
    });
  }
  #endregion

  #region Messengers
  private void RegisterMessengers()
  {
    WeakReferenceMessenger.Default.Register<Message, string>(this, Tokens.RemoveUnbookmarkedNote, new((recipient, message) =>
    {
      var noteViewModel = (NoteViewModel)message.Sender!;
      if (Navigation is NavigationBookmarks)
        NoteViewModels.Remove(noteViewModel);
    }));
  }
  #endregion
}