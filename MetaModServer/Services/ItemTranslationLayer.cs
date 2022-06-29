using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using LiteDB.Async;
using MetaModFramework.DTOs;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MetaModFramework.Services
{
    public class ItemTranslationLayer : IAsyncDisposable
    {
        private class ItemTranslationEntry : IEquatable<ItemTranslationEntry>
        {
            [BsonCtor]
            public ItemTranslationEntry(string serverName, string clientName, string gameName)
            {
                ServerName = serverName;
                ClientName = clientName;
                GameName   = gameName;
            }
            public string ServerName { get; }
            public string ClientName { get; }
            public string GameName   { get; }
            
            [BsonId]
            // ReSharper disable once UnusedMember.Local
            public int Id
            {
                get { return GetHashCode(); }
            }

            public bool Equals(ItemTranslationEntry other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(ServerName, other.ServerName, StringComparison.InvariantCultureIgnoreCase) && string.Equals(ClientName, other.ClientName, StringComparison.InvariantCultureIgnoreCase) && string.Equals(GameName, other.GameName, StringComparison.InvariantCultureIgnoreCase);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((ItemTranslationEntry)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = (int) 2166136261;
                    hash = ( hash * 16777619 ) ^ ServerName.GetStableHashCode();
                    hash = ( hash * 16777619 ) ^ ClientName.GetStableHashCode();
                    hash = ( hash * 16777619 ) ^ GameName.GetStableHashCode();
                    return hash;
                }
            }

            public static bool operator ==(ItemTranslationEntry left, ItemTranslationEntry right) => Equals(left, right);

            public static bool operator !=(ItemTranslationEntry left, ItemTranslationEntry right) => !Equals(left, right);
        }
        
        private readonly Dictionary<ClientItemDefinition, string>       _CtS_dictionary = new();
        private readonly Dictionary<string, List<ItemTranslationEntry>> _StC_dictionary = new ();
        private readonly LiteDatabaseAsync                              _db;
        private readonly Task                                           _readInTask;
        
        public ItemTranslationLayer(LiteDatabaseAsync db)
        {
            _db         = db;
            _readInTask = ReadAllAsync();
        }
        
        public Task<List<string>> GetAllDefinitions(string mod)
        {
           return Task.Run(() =>
                           {
                               return _StC_dictionary.Values
                                          .SelectMany(x => x, (x, ite) => new { x, ite })
                                          .Where(t => t.ite.GameName.Equals(mod))
                                          .Select(t => t.ite.ClientName)
                                          .ToList();
                           });
        }
        
        private async Task<ClientItemDefinition> GetClientNameAsync(string mod, ServerItemDefinition serverItemDefinition)
        {
            await _readInTask;
        
            _StC_dictionary.TryGetValue(mod, out var entries);
            return entries?.Where(x => x.ServerName == serverItemDefinition.UniqueIdentifier)
                           .Select(x => new ClientItemDefinition
                                        {
                                            Game             = mod,
                                            UniqueIdentifier = x.ClientName
                                        }).FirstOrDefault();
        }

        public async Task<IEnumerable<ClientItem>> GetClientNamesAsync(string mod, params ServerItem[] serverItem)
        {
            var ret = new List<ClientItem>();
            foreach (var si in serverItem)
            {
                var def = await GetClientNameAsync(mod, si.ItemDefinition);
                if (def != null)
                    ret.Add(new ClientItem
                        {
                            Amount         = si.Amount,
                            ItemDefinition = def
                        }
                       );
            }

            return ret;
        }

        public async Task<List<ServerItem>> GetServerNamesAsync(params ClientItem[] serverItem)
        {
            var ret = new List<ServerItem>();
            foreach (var si in serverItem)
            {
                ret.Add(new ServerItem
                        {
                            Amount = si.Amount,
                            ItemDefinition = await GetServerNameAsync(si.ItemDefinition)
                        });
            }
            
            return ret;
        }

        private async Task<ServerItemDefinition> GetServerNameAsync(ClientItemDefinition clientItemDefinition)
        {
            await _readInTask;
            return _CtS_dictionary.TryGetValue(clientItemDefinition, out var clientName) ? new ServerItemDefinition{UniqueIdentifier = clientName} : null;
        }

        private async Task ReadAllAsync()
        {
            await ReadFromDbAsync();
            await ReadFromFilesAsync();
        }

        private Task ReadFromFilesAsync()
        {
            return Task.Run(async () =>
                            {
                                var dir = Path.Combine(Directory.GetCurrentDirectory(), "GameTranslationRules");
                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);
                                    return;
                                }

                                foreach (var filePath in Directory.GetFiles(dir))
                                {
                                    if (Path.GetExtension(filePath) != ".json")
                                    {
                                        continue;
                                    }
                                    var success = true;
                                    try
                                    {
                                       
                                        var iteA = JsonSerializer.Deserialize<ItemTranslationEntry[]>(await File.ReadAllTextAsync(filePath));
                                        if (iteA == null || iteA.Length == 0)
                                            continue;

                                        foreach (var ite in iteA)
                                        {
                                            if (ite == null || string.IsNullOrWhiteSpace(ite.ServerName) ||
                                                string.IsNullOrWhiteSpace(ite.ClientName))
                                            {
                                                continue;
                                            }

                                            _CtS_dictionary[new ClientItemDefinition
                                                                 {
                                                                     Game             = ite.GameName,
                                                                     UniqueIdentifier = ite.ClientName
                                                                 }] = ite.ServerName;
                                            {
                                                if (_StC_dictionary.TryGetValue(ite.ServerName, out var names))
                                                {
                                                    names.Add(ite);
                                                }
                                                else
                                                {
                                                    if (!_StC_dictionary.TryAdd(ite.ServerName,
                                                            new List<ItemTranslationEntry>
                                                            { ite }))
                                                    {
                                                        success = false;
                                                    }
                                                }
                                            }
                                            // else
                                            // {
                                            //     Console.WriteLine($"Item: {ite.ServerName}, ClientName: {ite.ClientName}, ModName: {ite.GameName} already exists in Database, Overwriting!");
                                            // }
                                        }
                                    }
                                    catch
                                    {
                                        success = false;
                                    }

                                    if (success)
                                    {
                                        await SaveToDbAsync();
                                        File.Move(filePath, filePath + ".added");
                                    }
                                    else
                                    {
                                        File.Move(filePath, filePath + ".broken");
                                    }
                                }
                            });
        }

        private async Task ReadFromDbAsync()
        {
            var col  = _db.GetCollection<ItemTranslationEntry>("ItemTranslationLayer");
            var list = await col.Query().ToListAsync();
            foreach (var entry in list)
            {
                _CtS_dictionary[
                                     new ClientItemDefinition
                                         {
                                             UniqueIdentifier = entry.ClientName,
                                             Game = entry.GameName
                                         }
                                    ]  = entry.ServerName;
                if (_StC_dictionary.TryGetValue(entry.GameName, out var entries))
                {
                    entries.Add(entry);
                }
                else
                {
                    _StC_dictionary.Add(entry.GameName, new List<ItemTranslationEntry>{entry});
                }
            }
        }
        private async Task SaveToDbAsync()
        {
            var col   = _db.GetCollection<ItemTranslationEntry>("ItemTranslationLayer");
            var tasks = _StC_dictionary.Values
                            .Select(itemTranslationEntry => col.UpsertAsync(itemTranslationEntry));
            await Task.WhenAll(tasks);
        }
        
        public async ValueTask DisposeAsync()
        {
            await _readInTask;
            await SaveToDbAsync();
        }
    }
}