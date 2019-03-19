using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem {

    public static void SaveTournament(TournamentController info)
    {

        BinaryFormatter bin = new BinaryFormatter();

        string path = Application.persistentDataPath + "/tour.bin";
        FileStream stream = new FileStream(path, FileMode.Create);

        TourInfo tourData = new TourInfo(info);

        bin.Serialize(stream, tourData);
        stream.Close();

    }

    public static void SavePlayerData(PlayerDataController data)
    {
        BinaryFormatter bin = new BinaryFormatter();

        string path = Application.persistentDataPath + "/player.bin";
        FileStream stream = new FileStream(path, FileMode.Create);

        PlayerData playerData = new PlayerData(data);

        bin.Serialize(stream, playerData);
        stream.Close();
    }

    public static TourInfo LoadTournament()
    {
        string path = Application.persistentDataPath + "/tour.bin";
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
            return null;
        }
    }

    public static PlayerData LoadPlayerData()
    {
        string path = Application.persistentDataPath + "/player.bin";
        if (File.Exists(path))
        {
            BinaryFormatter bin = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            PlayerData pData = bin.Deserialize(stream) as PlayerData;

            stream.Close();
            return pData;
        }
        else
        {
            return null;
        }
    }
}
