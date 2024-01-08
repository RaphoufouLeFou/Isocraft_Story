using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class NetworkManagement : MonoBehaviour
{
    private NetworkManager manager;
    private bool IsHost;

    public string MainMenuScenename = "MainMenu";

    private bool IsPaused = false;
    public GameObject PauseMenu;


    void Start()
    {
        PauseMenu.SetActive(false);
        manager = GetComponent<NetworkManager>();
        bool IsOnline = NetworkInfos.IsMultiplayerGame;
        IsHost = NetworkInfos.IsHost;

        if (!NetworkInfos.StartedFromMainMenu)
        {
            IsHost = true;
            IsOnline = true;
        }

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
}
