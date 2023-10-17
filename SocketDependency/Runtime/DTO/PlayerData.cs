using Newtonsoft.Json;
using System;

namespace NagihSkeleton.Console
{
    [System.Serializable]
    public class PlayerData
    {
        private const string ID = "id";
        private const string NAME = "name";

        public string Id;
        public string Name;
        public bool IsMaster;
        public int Index;
        public string LayoutName;
        public int CharacterIndex;

        public bool IsJoinGame => Index >= 0 && Index < 4;

        public PlayerData(string id, string name)
        {
            Id = id;
            Name = Uri.UnescapeDataString(name);
            IsMaster = false;
            Index = -1;
            CharacterIndex = 0;
        }

        public void Clear()
        {
            Id = string.Empty;
            Name = string.Empty;
            IsMaster = false;
            Index = 0;
            LayoutName = string.Empty;
            CharacterIndex = 0;
        }

        public PlayerData FromJson(JsonReader reader)
        {
            Clear();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader.TokenType != JsonToken.PropertyName)
                    continue;

                switch (Convert.ToString(reader.Value))
                {
                    case ID:
                        reader.Read();
                        Id = Convert.ToString(reader.Value);
                        break;
                    case NAME:
                        reader.Read();
                        Name = Uri.UnescapeDataString(Convert.ToString(reader.Value));
                        break;
                }
            }
            return this;
        }

        public void SyncNameId(PlayerData data)
        {
            Id = data.Id;
            Name = data.Name;
        }

        public void SyncGameData(PlayerData data)
        {
            CharacterIndex = data.CharacterIndex;
        }

        public override string ToString()
        {
            return $"Id:{Id} Name:{Name} IsMaster:{IsMaster} Index:{Index} Character:{CharacterIndex}";
        }
    }

}
