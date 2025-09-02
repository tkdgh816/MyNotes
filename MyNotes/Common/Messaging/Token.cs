using System.Diagnostics.CodeAnalysis;

namespace MyNotes.Common.Messaging;
public readonly struct Token<T>(T value) : IEquatable<Token<T>> where T : notnull
{
  public T Value { get; } = value;
  public override bool Equals([NotNullWhen(true)] object? obj) => obj is Token<T> other && Equals(other);
  public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(Value);
  public bool Equals(Token<T> other) => EqualityComparer<T>.Default.Equals(Value, other.Value);
  public static bool operator ==(Token<T> left, Token<T> right) => left.Equals(right);
  public static bool operator !=(Token<T> left, Token<T> right) => !left.Equals(right);
}
