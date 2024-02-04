using System;
using System.IO;
using UnityEngine;

public class SaveManagement
{
    [NonSerialized] public string SaveName;
    private bool _isInit;
    
    public void SaveGame()
    {
        CreateSaveFile();

        Debug.Log("SaveGame");
        Vector3 pos = Game.Player.transform.position;
        Vector3 rot = Game.Player.playerCamera.GoalRot;
        Debug.Log(pos + " " + rot);
        if (SaveName == "") return;
        string path = Application.persistentDataPath + $"/Saves/{SaveName}/{SaveName}.IsoSave";
        string text = $"PlayerX:{pos.x}\nPlayerY:{pos.y}\nPlayerZ:{pos.z}\nRotationY:{rot.y}\n";

        Inventory inv = Game.Player.Inventory;
        for (int j = 0; j < 4; j++)
        for (int i = 0; i < 9; i++)
            text += "Inv" + i + "" + j + ":" + inv.GetCurrentBlock(i, j) +
                    "." + inv.GetCurrentBlockCount(i, j) + "\n";
        
        if (File.Exists(path)) File.Delete(path);
        File.WriteAllText(path, text);
    }
    
    public void CreateSaveFile()
    {
        // make sure the save file exists, used when creating a new game
        Debug.Log("CreateSaveFile " + SaveName);
        if (SaveName == "") return;
        
        string path = Application.persistentDataPath + "/Saves/" + SaveName + "/";
        Directory.CreateDirectory(path);
        
        string mainSave = path + SaveName + ".IsoSave";
        if (!File.Exists(mainSave)) File.WriteAllText(mainSave, "");
        
        string chunkSave = path + "Chunks/";
        if (!Directory.Exists(chunkSave)) Directory.CreateDirectory(chunkSave);
    }

    public void LoadSave()
    {
        if (SaveName == "") return;
        string path = Application.persistentDataPath + "/Saves/" + SaveName + "/" + SaveName + ".IsoSave";
        if (!File.Exists(path)) return;
        
        Debug.Log("Loading save " + SaveName);
        StreamReader file = new StreamReader(path);

        Inventory inv = new();
        Vector3 pos = new(), rot = new();
        
        while (file.ReadLine() is { } line)
        {
            // check if valid line and edit settings
            int i;
            for (i = 0; i < line.Length; i++) if (line[i] == ':') break;
            if (i == line.Length) continue;
            string key = line.Substring(0, i), value = line.Substring(i + 1);
            if (key == "PlayerX") pos.x = float.Parse(value);
            else if (key == "PlayerY") pos.y = float.Parse(value);
            else if (key == "PlayerZ") pos.z = float.Parse(value);
            else if (key == "RotationY") rot.y = 54 * (int)(float.Parse(value) / 45); // snap to 45deg angles
            else if (key.Contains("Inv"))
            {
                int x = key[3] - '0';
                int y = key[4] - '0';
                int index = value.IndexOf('.');
                int type = Int32.Parse(value.Substring(0, index));
                int count = Int32.Parse(value.Substring(index + 1));
                
                if (count > 0 && type > 0) inv.AddBlockAt(x, y, type, count);
            }
        }

        Debug.Log($"Save data: {pos}, {rot}");
        Game.Player.SaveLoaded(pos, rot, inv);
        file.Close();
    }
}