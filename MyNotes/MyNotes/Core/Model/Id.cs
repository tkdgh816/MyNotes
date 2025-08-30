namespace MyNotes.Core.Model;
internal interface IId<T> : IEquatable<T> where T : struct, IId<T>
{
  Guid Value { get; init; }
}

internal readonly struct NoteId(Guid value) : IId<NoteId>
{
  public Guid Value { get; init; } = value;

  public bool Equals(NoteId other) => this.Value == other.Value;
  public override int GetHashCode() => Value.GetHashCode();

  public override bool Equals(object? obj) => obj is NoteId other && Equals(other);
  public static bool operator ==(NoteId left, NoteId right) => left.Equals(right);
  public static bool operator !=(NoteId left, NoteId right) => !left.Equals(right);

  public override string ToString() => Value.ToString();
}

internal readonly struct BoardId(Guid value) : IId<BoardId>
{
  public Guid Value { get; init; } = value;

  public static BoardId Empty { get; } = new();

  public bool Equals(BoardId other) => this.Value == other.Value;
  public override int GetHashCode() => Value.GetHashCode();

  public override bool Equals(object? obj) => obj is BoardId other && Equals(other);
  public static bool operator ==(BoardId left, BoardId right) => left.Equals(right);
  public static bool operator !=(BoardId left, BoardId right) => !left.Equals(right);

  public override string ToString() => Value.ToString();
}

internal readonly struct TagId(Guid value) : IId<TagId>
{
  public Guid Value { get; init; } = value;

  public bool Equals(TagId other) => this.Value == other.Value;
  public override int GetHashCode() => Value.GetHashCode();

  public override bool Equals(object? obj) => obj is TagId other && Equals(other);
  public static bool operator ==(TagId left, TagId right) => left.Equals(right);
  public static bool operator !=(TagId left, TagId right) => !left.Equals(right);

  public override string ToString() => Value.ToString();
}

internal static class IdExtensions
{
  public static bool IsEmpty<T>(IId<T> id) where T : struct, IId<T>
    => id.Value == Guid.Empty;
}