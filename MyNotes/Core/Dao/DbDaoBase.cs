using Microsoft.Data.Sqlite;

namespace MyNotes.Core.Dao;

internal abstract class DbDaoBase()
{
  protected static T? GetReaderValue<T>(SqliteDataReader reader, string fieldName, T? nullValue = default) where T : notnull
  {
    int ordinal = reader.GetOrdinal(fieldName);
    return reader.IsDBNull(ordinal)
      ? nullValue
      : typeof(T) switch
      {
        Type t when t == typeof(bool) => (T)(object)reader.GetBoolean(ordinal),
        Type t when t == typeof(byte) => (T)(object)reader.GetByte(ordinal),
        Type t when t == typeof(short) => (T)(object)reader.GetInt16(ordinal),
        Type t when t == typeof(int) => (T)(object)reader.GetInt32(ordinal),
        Type t when t == typeof(long) => (T)(object)reader.GetInt64(ordinal),
        Type t when t == typeof(DateTime) => (T)(object)reader.GetDateTime(ordinal),
        Type t when t == typeof(DateTimeOffset) => (T)(object)reader.GetDateTimeOffset(ordinal),
        Type t when t == typeof(Guid) => (T)(object)new Guid(reader.GetString(ordinal)),
        Type t when t == typeof(string) => (T)(object)reader.GetString(ordinal),
        Type t when t == typeof(double) => (T)(object)reader.GetDouble(ordinal),
        _ => (T)reader[ordinal]
      };
  }
}