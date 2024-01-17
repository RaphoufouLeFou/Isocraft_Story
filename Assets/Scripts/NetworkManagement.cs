using System;
using kcp2k;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class NetworkManagement : MonoBehaviour
{
    private NetworkManager _manager;
    private bool _isHost;

    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        _manager = GetComponent<NetworkManager>();

        if (!NetworkInfos.StartedFromMainMenu)
        {
            NetworkInfos.IsHost = true;
            NetworkInfos.IsMultiplayerGame = true;
            NetworkInfos.uri = new Uri($"kcp://127.0.0.1:1234");
        }
        bool isOnline = NetworkInfos.IsMultiplayerGame;
        _isHost = NetworkInfos.IsHost;
        _manager.maxConnections = isOnline ? 10 : 1;
        if (_isHost)
        {
            if(isOnline) _manager.GetComponent<KcpTransport>().Port = (ushort)NetworkInfos.uri.Port;
            _manager.StartHost();
        }
        else
        {
            if(NetworkInfos.IsLocalHost) _manager.StartClient();
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
        if (_isHost) _manager.StopHost();
        else _manager.StopClient();

        SceneManager.LoadScene(mainMenuSceneName);
    }
}