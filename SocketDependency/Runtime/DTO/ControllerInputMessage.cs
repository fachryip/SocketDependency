using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NagihSkeleton.Console
{
    public class ControllerInputMessage
    {
        public const string ID = "id";
        public const string INPUT = "input";
        public const string CONDITION = "condition";
        public const string CONTENT = "content";

        public string id;
        public string input;
        public string condition;
        public Dictionary<string, object> content;

        private string _key;

        public ControllerInputMessage()
        {
            content = new Dictionary<string, object>();
            Clear();
        }

        public ControllerInputMessage(string id, string input, string condition)
        {
            content = new Dictionary<string, object>();
            Clear();

            this.id = id;
            this.input = input;
            this.condition = condition;
        }

        public void Clear()
        {
            id = string.Empty;
            input = string.Empty;
            condition = string.Empty;
            content.Clear();
            _key = string.Empty;
        }

        public ControllerInputMessage FromJson(JsonReader reader)
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
                        id = Convert.ToString(reader.Value);
                        break;
                    case INPUT:
                        reader.Read();
                        input = Convert.ToString(reader.Value);
                        break;
                    case CONDITION:
                        reader.Read();
                        condition = Convert.ToString(reader.Value);
                        break;
                    case CONTENT:
                        reader.Read();
                        if (reader.TokenType == JsonToken.Null)
                            break;
                        ConvertToDictionary(reader);
                        break;
                }
            }
            return this;
        }

        private void ConvertToDictionary(JsonReader reader)
        {
            content.Clear();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                if (reader.TokenType != JsonToken.PropertyName)
                    continue;

                _key = Convert.ToString(reader.Value);
                reader.Read();
                content[_key] = reader.Value;
            }
        }

        public override string ToString()
        {
            return $"Id:{id} Input:{input} Condition:{condition} Content:{content}";
        }
    }
}