using System;
using SQLite;

namespace DontForget.Persistence
{
    public interface ISQLiteDB
    {
        SQLiteAsyncConnection GetConnection();
    }
}
