using System;
using System.Collections.Generic;
using System.Threading;

namespace MetaModFramework.Services
{
    public class ServiceTransactions
    {
        private long _lock;
        public bool Lock
        {
            get
            {
                lock (SyncLock)
                {
                    return Interlocked.Read(ref this._lock) == 1;
                }
            }
            set
            {
                lock (SyncLock)
                {
                    Interlocked.Exchange(ref this._lock, Convert.ToInt64(value));
                }
            }
        }

        private static readonly object SyncLock = new ();

        private static readonly Dictionary<string, ServiceTransactions> Directory = new();
        
        public static bool TransactionNotInUse(string user)
        {
            lock (SyncLock)
            {
                return !Directory[user].Lock;
            }
        }
        
        public static bool CanNotRequestTransaction(string user)
        {
            lock (SyncLock)
            {
                var @lock = Directory[user].Lock;
                if (@lock)
                    return true;
                Directory[user].Lock = true;
                return false;
            }
        }
        
        public static void EndTransaction(string user)
        {
            lock (SyncLock)
            {
                Directory[user].Lock = false;
            }
        }

        public static void AddUser(string user)
        {
            lock (SyncLock)
            {
                Directory.TryAdd(user, new ServiceTransactions());
            }
        }
    }
}