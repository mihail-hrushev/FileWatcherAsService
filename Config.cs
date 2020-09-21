using Newton = Newtonsoft.Json;
using System.IO;

namespace FileWatcherPentahoRun
{
    class Config
    {
        public int timeout { get; set; }

        public static Config LoadConfig(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var items = Newton.JsonConvert.DeserializeObject<Config>(json);
            return items;
        }

        public static void SaveConfig(string filePath, Config _itemList)
        {
            File.WriteAllText(filePath, Newton.JsonConvert.SerializeObject(_itemList));
        }

    }
}
