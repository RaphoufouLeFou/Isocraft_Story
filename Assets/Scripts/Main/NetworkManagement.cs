using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class NetworkManagement : MonoBehaviour
{
    private NetworkManager manager;
    private bool IsHost;

    public string MainMenuScenename = "MainMenu";

    [NonSerialized] public bool IsPaused = false;

    public GameObject PauseMenu;

    [NonSerialized] public bool IsChunksReady = false;
    private Dictionary<string, Chunk> Chunks;


    void Start()
    {
        PauseMenu.SetActive(false);
        manager = GetComponent<NetworkManager>();

        if (!NetworkInfos.StartedFromMainMenu)
        {
            NetworkInfos.IsHost = true;
            NetworkInfos.IsMultiplayerGame = true;
        }

        bool IsOnline = NetworkInfos.IsMultiplayerGame;
        IsHost = NetworkInfos.IsHost;

        manager.maxConnections = IsOnline ? 2 : 1;

        if (IsHost)
            manager.StartHost();
        else
            manager.StartClient();
    }



    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            IsPaused = !IsPaused;
            PauseMenu.SetActive(IsPaused);
        }
    }

    public void ButtonResumeClick()
    {
        IsPaused = !IsPaused;
        PauseMenu.SetActive(IsPaused);
    }

    public void LeaveGameButtonClick()
    {
        if (IsHost)
            manager.StopHost();
        else
            manager.StopClient();

        SceneManager.LoadScene(MainMenuScenename);
    }

    public void ReadyToSendChunks(Dictionary<string, Chunk> chunks)
    {
        IsChunksReady = true;
        Chunks = chunks;
    }

}
