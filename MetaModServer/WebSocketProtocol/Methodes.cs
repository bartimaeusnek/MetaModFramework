using System;
using System.Reflection;
using MetaModFramework.Controllers;
using static MetaModFramework.WebSocketProtocol.Methodes;

namespace MetaModFramework.WebSocketProtocol;

public enum Methodes : long
{
    RequestItems             = 0x0001,
    OverwriteData            = 0x0002,
    UpsertItems              = 0x0003,
    RequestAndDecrementItems = 0x0004,
}

public static class MethodesImpl
{
    public static MethodInfo GetMethodInfo(this Methodes methodes)
    {
        return methodes switch
               {
                   UpsertItems              => UpsertItemsMi.Value,
                   RequestAndDecrementItems => RequestAndDecrementItemsMi.Value,
                   RequestItems             => RequestItemsMi.Value,
                   OverwriteData            => throw new NotImplementedException(),
                   _                        => throw new ArgumentOutOfRangeException(nameof(methodes), methodes, null)
               };
    }

    private static Type             itemTransferController     = typeof(ItemTransferController);
    private static Lazy<MethodInfo> UpsertItemsMi              = new(() => GetMethodInfo(nameof(ItemTransferController.PostUpsertItems)));
    private static Lazy<MethodInfo> RequestAndDecrementItemsMi = new(() => GetMethodInfo(nameof(ItemTransferController.PutRequestItems)));
    private static Lazy<MethodInfo> RequestItemsMi             = new(() => GetMethodInfo(nameof(ItemTransferController.GetAsyncInternal)));
    private static Lazy<MethodInfo> OverwriteDataMi            = new(() => throw new NotImplementedException());
    private static MethodInfo       GetMethodInfo(string name) => itemTransferController.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
}