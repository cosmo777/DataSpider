using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace DataSpider
{
    public static class SerializationTool
    {
        public static string SerializeJson(object objectToSerialize)
        {
            var stringBuilder = new StringBuilder();
            var jsonSerializer = new JsonSerializer();
            jsonSerializer.Formatting = Formatting.Indented;
            jsonSerializer.Serialize(new JsonTextWriter(new StringWriter(stringBuilder)), objectToSerialize);
            return stringBuilder.ToString();
        }
        public static void SerializeJsonFile(string path, object objectToSerialize)
        {
            var json = SerializeJson(objectToSerialize);
            File.WriteAllText(path, json);
        }

        public static T DeserializeJson<T>(string jsonString) where T : class
        {
            var jsonDeserializer = new JsonSerializer();
            var config = jsonDeserializer.Deserialize<T>(new JsonTextReader(new StringReader(jsonString)));
            return config;
        }

        public static T DeserializeJsonFile<T>(string path) where T : class
        {
            if (File.Exists(path))
            {
                var jsonString = File.ReadAllText(path);
                return DeserializeJson<T>(jsonString) as T;
            }
            return default(T);
        }
    }


}
