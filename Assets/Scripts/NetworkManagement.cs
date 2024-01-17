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
        }

        bool isOnline = NetworkInfos.IsMultiplayerGame;
        _isHost = NetworkInfos.IsHost;
        _manager.maxConnections = isOnline ? 2 : 1;
        if (_isHost)
        {
            _manager.StartHost();
            _manager.GetComponent<KcpTransport>().Port = (ushort)NetworkInfos.uri.Port;
        }
        else
        {
            if(NetworkInfos.IsLocalHost) _manager.StartClient();
            else
            {
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