using System;
using System.IO;
using AspNetCore.Identity.LiteDB.Data;
using LiteDB;
using LiteDB.Async;

namespace MetaModFramework.Services
{
    public class LiteDbInstance : ILiteDbContext, IDisposable
    {
        public LiteDatabaseAsync Database { get; } = new($"Filename={Path.Combine(Directory.GetCurrentDirectory(), "LDB.mms")};Connection=shared;");

        public void Dispose()
        {
            Database?.Dispose();
        }

        public LiteDatabase LiteDatabase
        {
            get
            {
                return (LiteDatabase) Database.UnderlyingDatabase;
            }
        }
    }
}