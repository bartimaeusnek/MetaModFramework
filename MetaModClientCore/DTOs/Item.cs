using System;

namespace MetaModClientCore.DTOs
{
    public class ClientItemDefinition
    {
        public string UniqueIdentifier { get; set; }
        public string Game             { get; set; }
    }

    public class ServerItemDefinition
    {
        public string UniqueIdentifier { get; set; }
    }

    public class ServerItem
    {
        public ServerItemDefinition ItemDefinition { get; set; }
        public ulong                Amount         { get; set; }
    }
    public class ClientItem
    {
        public ClientItemDefinition ItemDefinition { get; set; }
        public ulong                Amount         { get; set; }
    }
    
}