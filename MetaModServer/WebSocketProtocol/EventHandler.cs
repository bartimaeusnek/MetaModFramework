namespace MetaModFramework.WebSocketProtocol
{
    public static class WsEventHandler
    {
        private static readonly object SyncLock = new ();
        private static          bool   _needsSyncBackingField;
        public static bool NeedsSync
        {
            get
            {
                lock (SyncLock)
                {
                    return _needsSyncBackingField;
                }
            }
            set
            {
                lock (SyncLock)
                {
                    _needsSyncBackingField = value;
                }
            }
        }

        public static void OnHandler()
        {
            NeedsSync = true;
        }
    }
}