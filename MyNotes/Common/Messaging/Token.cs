using System.Diagnostics.CodeAnalysis;

namespace MyNotes.Common.Messaging;
internal readonly struct Token : IEquatable<Token>
{
  public override bool Equals([NotNullWhen(true)] object? obj) => base.Equals(obj);
  public override int GetHashCode() => base.GetHashCode();
  public bool Equals(Token other) => this.Equals(other);
  public static bool operator ==(Token left, Token right) => left.Equals(right);
  public static bool operator !=(Token left, Token right) => !left.Equals(right);
}
