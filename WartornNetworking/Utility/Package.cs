using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace WartornNetworking
{
    namespace Utility
    {
        public class Package
        {
            public readonly Messages messages;
            public readonly Commands commands;
            public string data;

            public Package(Messages msgs, Commands cmds, string data)
            {
                messages = msgs;
                commands = cmds;
                this.data = data;
            }

            public Package(string package)
            {
                Package msg = JsonConvert.DeserializeObject<Package>(package);
                messages = msg.messages;
                commands = msg.commands;
                data = msg.data;
            }

            public override string ToString()
            {
                return string.Format("{0}|{1}|{2}", messages.ToString(), commands.ToString(), data);
            }
        }

        public class MessageJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Package);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Package temp = (Package)value;

                writer.WriteStartObject();
                writer.WritePropertyName("Messages");
                serializer.Serialize(writer, temp.messages.ToString());
                writer.WritePropertyName("Commands");
                serializer.Serialize(writer, temp.commands.ToString());
                writer.WritePropertyName("data");
                serializer.Serialize(writer, temp.data);
                writer.WriteEndObject();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                Messages msgs = Messages.Accept;
                Commands cmds = Commands.Message;
                string data = string.Empty;

                bool isGotMessages = false,
                     isGotCommands = false,
                     isGotData = false;

                while (reader.Read())
                {
                    if (reader.TokenType != JsonToken.PropertyName)
                    {
                        break;
                    }

                    string propertyName = (string)reader.Value;
                    if (!reader.Read())
                    {
                        continue;
                    }

                    switch (propertyName)
                    {
                        case "Messages":
                            msgs = (serializer.Deserialize<string>(reader)).ToEnum<Messages>();
                            isGotCommands = true;
                            break;
                        case "Commands":
                            cmds = (serializer.Deserialize<string>(reader)).ToEnum<Commands>();
                            isGotMessages = true;
                            break;
                        case "data":
                            data = serializer.Deserialize<string>(reader);
                            isGotData = true;
                            break;
                    }
                }

                if (isGotCommands && isGotMessages && isGotData)
                {
                    return new Package(msgs, cmds, data);
                }
                else
                {
                    throw new InvalidDataException("Not enought data");
                }
            }
        }
    }
}