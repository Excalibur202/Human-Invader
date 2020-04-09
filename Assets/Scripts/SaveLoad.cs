using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SaveLoad
{
    public static class BinarySaveLoad
    {

        private static string fileExtension = ".dat";

        public static void SaveBinary<T>(this T data, string folderPath, string fileName)
        {

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string dataPath = Path.Combine(folderPath, fileName + fileExtension);

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (FileStream fileStream = File.Open(dataPath, FileMode.OpenOrCreate))
            {
                binaryFormatter.Serialize(fileStream, data);
            }
            //Debug.Log(fileName + " -- Saved");
        }

        public static T LoadBinary<T>(this T loadedData, string folderPath, string fileName)
        {

            string[] filePaths = GetFilePaths(folderPath, fileName);
            //Debug.Log(filePaths.Length);
            if (filePaths.Length == 1)
            {

                BinaryFormatter binaryFormatter = new BinaryFormatter();
                foreach (string filePath in filePaths)
                {
                    using (FileStream fileStream = File.Open(filePath, FileMode.Open))
                    {
                        //Debug.Log(fileName + " -- Loaded");
                        return (T)binaryFormatter.Deserialize(fileStream);
                    }
                }

            }
            //Debug.("Error loading " + fileName);
            return loadedData;
        }

        static string[] GetFilePaths(string folderPath, string fileName)
        {
            return Directory.GetFiles(folderPath, fileName + fileExtension);
        }
    }
}
