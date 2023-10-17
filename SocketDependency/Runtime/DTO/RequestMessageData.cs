using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace NagihSkeleton.Console
{
    public class RequestMessageData
    {
        public string Type;
        public Dictionary<string, object> Data;
        public Dictionary<string, object> Content;
        public string[] Ids;
        public int Idx;

        public RequestMessageData(string type)
        {
            Type = type;
            Data = new Dictionary<string, object>();
            Content = new Dictionary<string, object>();
            Ids = new string[8];
            Idx = 0;
        }

        public void Clear()
        {
            Data.Clear();
            Content.Clear();
            ClearId();
        }

        public void ClearId()
        {
            for (var i = 0; i < Ids.Length; i++)
            {
                Ids[i] = string.Empty;
            }
            Idx = 0;
        }

        public void AddDataValue(string key, object value)
        {
            Data[key] = value;
        }

        public void AddContentValue(string key, object value)
        {
            Content[key] = value;
        }

        public void AddContentDictionary(Dictionary<string, object> content)
        {
            Content = content;
        }

        public void AddId(string id)
        {
            if (Idx >= 0 && Idx < Ids.Length)
            {
                Ids[Idx++] = id;
            }
        }

        public string ToJson()
        {
            using var sw = new StringWriter();
            using var writer = new JsonTextWriter(sw);

            writer.WriteStartObject();

            writer.WritePropertyName(ConstConsole.PROPERTY_TYPE);
            writer.WriteValue(Type);

            if (Data.Count > 0)
            {
                writer.WritePropertyName(ConstConsole.PROPERTY_DATA);
                writer.WriteStartObject();

                foreach (var keypair in Data)
                {
                    writer.WritePropertyName(keypair.Key);
                    writer.WriteValue(keypair.Value);
                }

                if (Content.Count > 0)
                {
                    writer.WritePropertyName(ConstConsole.PROPERTY_CONTENT);
                    writer.WriteStartObject();

                    foreach (var keypair in Content)
                    {
                        writer.WritePropertyName(keypair.Key);
                        writer.WriteValue(keypair.Value);
                    }

                    writer.WriteEndObject();
                }

                if (Idx > 0)
                {
                    writer.WritePropertyName(ConstConsole.PROPERTY_CONTROLLER_ID);
                    writer.WriteStartArray();

                    for (int i = 0; i < Idx; i++)
                    {
                        writer.WriteValue(Ids[i]);
                    }

                    writer.WriteEndArray();
                }

                writer.WriteEndObject();
            }

            writer.WriteEndObject();
            return sw.ToString();
        }
    }
}