using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class LoadManager
{
    public static List<string> ReturnSubdirectories(string path)
    {
        List<string> result = new List<string>();

        if (Directory.Exists(path)){
            DirectoryInfo baseDir = new DirectoryInfo(path);

            // Get a reference to each directory in that directory.
            
            foreach(DirectoryInfo dir in baseDir.EnumerateDirectories())
            {
                result.Add(dir.Name);
            }
        }

        return result;
    }

    public static void CreateFolder(string path, string directoryName)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        string directoryPath = Path.Combine(path, directoryName);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    public static void SaveFile<T>(string path, T objectToSave)
    {
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();

        if (File.Exists(path))
        {
            file = File.OpenWrite(path);
        }
        else
        {
            file = File.Create(path);
        }

        bf.Serialize(file, objectToSave);
        file.Close();
    }

    public static T ReadFile<T>(string path) where T : new()
    {
        T result = new T();

        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.OpenRead(path);
            result = (T)bf.Deserialize(file);
            file.Close();

            return result;
        }

        return result;
    }

    public static void DeleteDirectory(string path)
    {
        Directory.Delete(path, true);
    }
}
