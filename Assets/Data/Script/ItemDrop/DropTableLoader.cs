using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class DropTableLoader
{
    public static List<DropTableData> LoadDropTables(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("Drop table Json not found at: " + path);
            return null;
        }

        string json = File.ReadAllText(path);
        DropTableCollection wrapper = JsonUtility.FromJson<DropTableCollection>("{\"tables\":" + json + "}");
        return wrapper.tables;
    }
}
