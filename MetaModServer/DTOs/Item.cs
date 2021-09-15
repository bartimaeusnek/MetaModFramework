using System;
using System.Text.Json.Serialization;
using LiteDB;

namespace MetaModFramework.DTOs
{
    public class ClientItemDefinition : IEquatable<ClientItemDefinition>
    {
        public string UniqueIdentifier { get; set; }
        public string Game             { get; set; }

        [JsonIgnore]
        [BsonId]
        public int Id
        {
            get => this.GetHashCode();
        }
        
        public bool Equals(ClientItemDefinition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.UniqueIdentifier == other.UniqueIdentifier && this.Game == other.Game;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ClientItemDefinition)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = (int) 2166136261;
                hash = ( hash * 16777619 ) ^ this.UniqueIdentifier.GetStableHashCode();
                hash = ( hash * 16777619 ) ^ this.Game.GetStableHashCode();
                return hash;
            }
        }

        public static bool operator ==(ClientItemDefinition left, ClientItemDefinition right) => Equals(left, right);

        public static bool operator !=(ClientItemDefinition left, ClientItemDefinition right) => !Equals(left, right);
    }

    public class ServerItemDefinition : IEquatable<ServerItemDefinition>
    {
        [BsonId(false)]
        public string UniqueIdentifier { get; set; }

        public bool Equals(ServerItemDefinition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.UniqueIdentifier == other.UniqueIdentifier;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ServerItemDefinition)obj);
        }

        public override int GetHashCode() => (this.UniqueIdentifier != null ? this.UniqueIdentifier.GetStableHashCode() : 0);

        public static bool operator ==(ServerItemDefinition left, ServerItemDefinition right) => Equals(left, right);

        public static bool operator !=(ServerItemDefinition left, ServerItemDefinition right) => !Equals(left, right);
    }

    public class ServerItem : IEquatable<ServerItem>
    {
        [JsonIgnore]
        [BsonId]
        public int Id
        {
            get => this.GetHashCode();
        }
        public ServerItemDefinition ItemDefinition { get; set; }
        public ulong                Amount         { get; set; }

        public bool Equals(ServerItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(this.ItemDefinition, other.ItemDefinition);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ServerItem)obj);
        }

        public override int GetHashCode() => (this.ItemDefinition != null ? this.ItemDefinition.GetHashCode() : 0);

        public static bool operator ==(ServerItem left, ServerItem right) => Equals(left, right);

        public static bool operator !=(ServerItem left, ServerItem right) => !Equals(left, right);
    }
    public class ClientItem : IEquatable<ClientItem>
    {   
        [JsonIgnore]
        [BsonId]
        public int Id
        {
            get => this.GetHashCode();
        }
        
        public ClientItemDefinition ItemDefinition { get; set; }
        public ulong                Amount         { get; set; }

        public bool Equals(ClientItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(this.ItemDefinition, other.ItemDefinition);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ClientItem)obj);
        }

        public override int GetHashCode() => (this.ItemDefinition != null ? this.ItemDefinition.GetHashCode() : 0);

        public static bool operator ==(ClientItem left, ClientItem right) => Equals(left, right);

        public static bool operator !=(ClientItem left, ClientItem right) => !Equals(left, right);
    }
    
}