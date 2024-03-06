using System;
using kcp2k;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class NetworkManagement : MonoBehaviour
{
    private NetworkManager _manager;
    private bool _isHost;

    public string mainMenuSceneName;

    public GameObject game;
    private Game _game;

    void Start()
    {
        _game = game.GetComponent<Game>();
        _game.InitGameUtils();

        _manager = GetComponent<NetworkManager>();
        _manager.enabled = true;
        if (!SuperGlobals.StartedFromMainMenu)
        {
            SuperGlobals.IsHost = true;
            SuperGlobals.IsMultiplayerGame = true;
            SuperGlobals.Uri = new Uri("kcp://127.0.0.1:7777");
        }
        
        bool isOnline = SuperGlobals.IsMultiplayerGame;
        _isHost = SuperGlobals.IsHost;
        _manager.maxConnections = isOnline ? 20 : 1;
        if (_isHost)
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
        
        // starting server is asynchronous, so don't StartGame here
    }
    
    public void LeaveGame()
    {
        Game.SaveManager.SaveGame();
        Game.Player = null; // unload Player to load it again next time

        if (_isHost) _manager.StopHost();
        else _manager.StopClient();
        
        SuperGlobals.IsMultiplayerGame = false;
        SuperGlobals.StartedFromMainMenu = false;
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
        Game.SaveManager.SaveGame();
    }

    private void Update()
    {
        if (_manager.isNetworkActive == false) SceneManager.LoadScene(mainMenuSceneName);
    }
}