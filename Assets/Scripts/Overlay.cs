using UnityEngine;
using TMPro;
using System;
using UnityEngine.Serialization;
using Mirror;

public class Overlay : MonoBehaviour
{
    [FormerlySerializedAs("TextData")] public TMP_Text textData;
    [NonSerialized] public float FPS;
    [NonSerialized] public float MS;
    [FormerlySerializedAs("UpdatePerSecond")] public int updatePerSecond = 2;
    private int _iterations = 1;

    void Update()
    {
        if(_iterations >= FPS / updatePerSecond)
        {
            FPS = 1.0f / Time.deltaTime;
            MS = Time.deltaTime * 1000.0f;
            _iterations = 0;
        }
        else _iterations++;
        string text = "";
        text = AddFps(text);
        text = AddMsps(text);
        text = AddCoordonates(text);
        textData.text = text;
    }

    string AddFps(string text)
    {
        return text += $"Fps : {FPS}\n";
    }

    string AddMsps(string text)
    {
        return text += $"Last frame time : {MS}\n";
    }

    string AddCoordonates(string text)
    {
        return text += $"position : x = {Math.Round(NetworkInfos.PlayerPos.x, 3)} y = {Math.Round(NetworkInfos.PlayerPos.y, 3)}, z = {Math.Round(NetworkInfos.PlayerPos.z, 3)}\n";
    }
}
