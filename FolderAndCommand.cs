using System;
using Newton = Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace FileWatcherPentahoRun
{
    public class FolderAndCommand: IEquatable<FolderAndCommand>
    {
        public string folderName;
        public string commandPath; 

        public FolderAndCommand( string folder, string command)
        {
            folderName = folder;
            commandPath = command; 
        }

        public bool Equals(FolderAndCommand other)
        {
            if ((this.folderName == other.folderName) && (this.commandPath == other.commandPath))
            {
                return true;
            } else {
                return false;
            }
        }

        public static List<FolderAndCommand> LoadItems(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var items = Newton.JsonConvert.DeserializeObject<List<FolderAndCommand>>(json);
            return items;
        }

        public static void SaveItems(string filePath, List<FolderAndCommand> _itemList)
        {
            File.WriteAllText(filePath, Newton.JsonConvert.SerializeObject(_itemList));
        }
    }
}
