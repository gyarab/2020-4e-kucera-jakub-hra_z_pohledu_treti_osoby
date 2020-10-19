using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

// TODO prob change methods to static adn remove the Instance
public class LoadManager
{
    private static LoadManager instance;

    public static LoadManager Instance
    {
        get
        {
            if (instance == null)
            {
                // Need to create a new GameObject to attach the singleton to.
                instance = new LoadManager();
            }

            return instance;
        }
    }

    public List<string> ReturnSubdirectories(string _path)
    {
        List<string> result = new List<string>();

        if (Directory.Exists(_path)){
            DirectoryInfo baseDir = new DirectoryInfo(_path);

            // Get a reference to each directory in that directory.
            
            foreach(DirectoryInfo dir in baseDir.EnumerateDirectories())
            {
                result.Add(dir.Name);
            }
        }

        return result;
    }

    public void CreateSave(string _path)
    {
        if (!Directory.Exists(Path.GetDirectoryName(_path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
        }

        string saveData = "test";
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(_path, FileMode.Create);
        bf.Serialize(stream, saveData);
        stream.Close();
    }

    public void DeleteDirectory(string _path)
    {
        Directory.Delete(_path, true);
    }
}
