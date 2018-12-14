using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem {

    public static void SaveTournament(TourInfo info)
    {

        BinaryFormatter bin = new BinaryFormatter();

        string path = Application.persistentDataPath + "/tourData.dat";
        FileStream stream = new FileStream(path, FileMode.Create);

        TourInfo tourData = new TourInfo(info);

        bin.Serialize(stream, tourData);
        stream.Close();

    }

    public static TourInfo LoadTournament()
    {
        string path = Application.persistentDataPath + "/tourData.dat";
        if (File.Exists(path))
        {
            BinaryFormatter bin = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            TourInfo tInfo = bin.Deserialize(stream) as TourInfo;

            stream.Close();
            return tInfo;
        }
        else
        {
            Debug.LogError("File not found in " + path);
            return null;
        }
    }
}
