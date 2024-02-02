using System;
using kcp2k;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class NetworkManagement : MonoBehaviour
{
    public GameObject game;
    private Game _game;

    private NetworkManager _manager;
    private bool _isHost;

    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        _game = game.GetComponent<Game>();

        _manager = GetComponent<NetworkManager>();
        _manager.enabled = true;
        if (!NetworkInfos.StartedFromMainMenu)
        {
            NetworkInfos.IsHost = true;
            NetworkInfos.IsMultiplayerGame = true;
            NetworkInfos.uri = new Uri("kcp://127.0.0.1:7777");
            _game.SaveManager.SaveName = "";
        }
        
        bool isOnline = NetworkInfos.IsMultiplayerGame;
        _isHost = NetworkInfos.IsHost;
        _manager.maxConnections = isOnline ? 20 : 1;
        if (_isHost)
        {
            if (isOnline) _manager.GetComponent<KcpTransport>().Port = (ushort)NetworkInfos.uri.Port;
            
            _manager.StartHost();
        }
        else
        {
            if (NetworkInfos.IsLocalHost) _manager.StartClient();
            else
            {
                _manager.GetComponent<KcpTransport>().Port = (ushort)NetworkInfos.uri.Port;
                _manager.networkAddress = NetworkInfos.uri.Host;
                _manager.StartClient(NetworkInfos.uri);
            }
        }
    }
    
    public void LeaveGameButtonClick()
    {
        _game.SaveManager.SaveGame();
        if (_isHost) _manager.StopHost();
        else _manager.StopClient();
        NetworkInfos.IsLocalHost = false;
        NetworkInfos.IsMultiplayerGame = false;
        NetworkInfos.StartedFromMainMenu = false;
        NetworkInfos.IsHost = false;
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
}