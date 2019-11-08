using System;
using System.IO;
using DontForget.iOS;
using SQLite;
using Xamarin.Forms;
using DontForget.Persistence;

[assembly: Dependency(typeof(SQLiteDB))]

namespace DontForget.iOS
{
    public class SQLiteDB : ISQLiteDB
    {

        public SQLiteAsyncConnection GetConnection()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var libpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");
            var path = Path.Combine(libpath, "MySQLite.db3");

            return new SQLiteAsyncConnection(path);
        }
    }
}
