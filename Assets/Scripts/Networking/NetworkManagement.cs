using System;
using System.Net;
using kcp2k;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Linq;

public class NetworkManagement : MonoBehaviour
{
    private NetworkManager _manager;

    public string mainMenuSceneName;

    public GameObject game;
    private Game _game;

    void Start()
    {
        _game = game.GetComponent<Game>();
        _game.InitGameUtils();

        _manager = GetComponent<NetworkManager>();
        _manager.enabled = true;

        if (SuperGlobals.EditorMode)
        {
            SuperGlobals.IsHost = true;
            SuperGlobals.IsMultiplayerGame = true;
            SuperGlobals.Uri = new Uri("kcp://127.0.0.1:7777");
        }

        bool isOnline = SuperGlobals.IsMultiplayerGame;
        _manager.maxConnections = isOnline ? 20 : 1;

        if (SuperGlobals.IsHost)
        {
            if (isOnline) _manager.GetComponent<KcpTransport>().Port = (ushort)SuperGlobals.Uri.Port;
            _manager.StartHost();
        }

        else
        {
            _manager.GetComponent<KcpTransport>().Port = (ushort)SuperGlobals.Uri.Port;
            _manager.networkAddress = SuperGlobals.Uri.Host;
            _manager.StartClient(SuperGlobals.Uri);
        }

        // server start is asynchronous, so don't StartGame here
    }

    public static string EncodeIP(string ip)
    {
        Debug.Log(ip);
        string res = "";
        string[] parts = ip.Split('.');
        foreach (string s in parts)
        {
            byte b = byte.Parse(s);
            byte high = (byte)(b >> 4);
            byte low = (byte)(b & 0xF);
            char h = (char)(high + 'A');
            char l = (char)(low + 'A');
            res += h;
            res += l;
        }

        return res;
    }

    public static (string ip, int err) DecodeIP(string encoded)
    {
        if (encoded.Length != 8) return ("", 1);
        string res = "";
        for (int i = 0; i < 8; i += 2)
        {
            char h = encoded[i];
            char l = encoded[i + 1];
            byte high = (byte)(h - 'A');
            byte low = (byte)(l - 'A');
            if (high >= 16 || low >= 16) return ("", 2);
            ushort n = (ushort)((high << 4) | low);
            res += i == 6 ? n : $"{n}.";
        }

        return (res, 0);
    }

    public static string GetLocalIPv4()
    {
        return new WebClient().DownloadString("https://icanhazip.com");
    }

    public static string GetLocalIPv4L()
    {
        return Dns.GetHostEntry(Dns.GetHostName())
        .AddressList.First(
        f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        .ToString();
    }

    public void LeaveGame()
    {
        Game.SaveManager.SaveGame();
        Game.Player = null; // unload Player to load it again next time

        if (SuperGlobals.IsHost) _manager.StopHost();
        else _manager.StopClient();

        SuperGlobals.IsMultiplayerGame = false;
        SuperGlobals.EditorMode = true;
        SuperGlobals.IsHost = false;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ChangeMaxConnection(int max)
    {
        _manager.maxConnections = max;
    }

    public void ChangePort(ushort port)
    {
        _manager.GetComponent<KcpTransport>().Port = port;
    }

    private void OnApplicationQuit()
    {
        if (Game.SaveManager is not null) Game.SaveManager.SaveGame();
    }

    private void Update()
    {
        if (_manager is null) throw new NullExceptionCrash("Game probably failed to load");
        if (_manager.isNetworkActive == false) SceneManager.LoadScene(mainMenuSceneName);
    }
}
