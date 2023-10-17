using Newtonsoft.Json;
using System;

namespace NagihSkeleton.Console
{
    public class PlayerDataMessage
    {
        private const string ID = "id";
        private const string NAME = "name";

        public string id;
        public string name;

        public PlayerDataMessage() { }

        public PlayerDataMessage(string id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public PlayerDataMessage FromJson(JsonReader reader)
        {
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
                        id = Convert.ToString(reader.Value);
                        break;
                    case NAME:
                        reader.Read();
                        name = Convert.ToString(reader.Value);
                        break;
                }
            }
            return this;
        }
    }
}