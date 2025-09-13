namespace MyNotes.Core.Shared;

internal static class DatabaseSettings
{
  public static class Repository
  {
    public static readonly string DirectoryName = "data";
    public static readonly string FileName = "data.sqlite";
  }

  public static class Table
  {
    public static readonly string Boards = "Boards";
    public static readonly string Notes = "Notes";
    public static readonly string Tags = "Tags";
    public static readonly string NotesTags = "NotesTags";

  }

  public static class Column
  {
    public static readonly string Id = "id";
    public static readonly string Grouped = "grouped";
    public static readonly string Parent = "parent";
    public static readonly string Previous = "previous";
    public static readonly string Name = "name";
    public static readonly string IconType = "icon_type";
    public static readonly string IconValue = "icon_value";
    public static readonly string Created = "created";
    public static readonly string Modified = "modified";
    public static readonly string Title = "title";
    public static readonly string Body = "body";
    public static readonly string Preview = "preview";
    public static readonly string Background = "background";
    public static readonly string Backdrop = "backdrop";
    public static readonly string Width = "width";
    public static readonly string Height = "height";
    public static readonly string PositionX = "position_x";
    public static readonly string PositionY = "position_y";
    public static readonly string Bookmarked = "bookmarked";
    public static readonly string Trashed = "trashed";
    public static readonly string Text = "text";
    public static readonly string Color = "color";
    public static readonly string NoteId = "note_id";
    public static readonly string TagId = "tag_id";
  }

  public static class Parameter
  {
    public static readonly string Id = "@id";
    public static readonly string Grouped = "@grouped";
    public static readonly string Parent = "@parent";
    public static readonly string Previous = "@previous";
    public static readonly string Name = "@name";
    public static readonly string IconType = "@icon_type";
    public static readonly string IconValue = "@icon_value";
    public static readonly string Created = "@created";
    public static readonly string Modified = "@modified";
    public static readonly string Title = "@title";
    public static readonly string Body = "@body";
    public static readonly string Preview = "@preview";
    public static readonly string Background = "@background";
    public static readonly string Backdrop = "@backdrop";
    public static readonly string Width = "@width";
    public static readonly string Height = "@height";
    public static readonly string PositionX = "@position_x";
    public static readonly string PositionY = "@position_y";
    public static readonly string Bookmarked = "@bookmarked";
    public static readonly string Trashed = "@trashed";
    public static readonly string Text = "@text";
    public static readonly string Color = "@color";
    public static readonly string NoteId = "@note_id";
    public static readonly string TagId = "@tag_id";
  }
}
