using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MainMenu : MonoBehaviour
{

    [SerializeField] private string mainSceneName = "Main";
    [SerializeField] private GameObject mainParent;
    [SerializeField] private GameObject newGameParent;
    [SerializeField] private GameObject loadGameParent;
    [SerializeField] private GameObject backButton;

    void Start()
    {
        backButton.SetActive(false);
        mainParent.SetActive(true);
        newGameParent.SetActive(false);
        loadGameParent.SetActive(false);
    }
    void Update() { }

    public void NewGameButtonClick()
    {
        backButton.SetActive(true);
        mainParent.SetActive(false);
        newGameParent.SetActive(true);
    }

    public void BackButtonClick()
    {
        mainParent.SetActive(true);
        newGameParent.SetActive(false);
        loadGameParent.SetActive(false);
        backButton.SetActive(false);
    }

    public void LoadGameButtonClick()
    {
        backButton.SetActive(true);
        mainParent.SetActive(false);
        loadGameParent.SetActive(true);
    }
    public void JoinGameButtonClick()
    {
        NetworkInfos.IsMultiplayerGame = true;
        NetworkInfos.IsHost = false;
        NetworkInfos.StartedFromMainMenu = true;
        SceneManager.LoadScene(mainSceneName);
    }
    public void MultiPlayerButtonClick()
    {
        NetworkInfos.IsMultiplayerGame = true;
        NetworkInfos.IsHost = true;
        NetworkInfos.StartedFromMainMenu = true;
        SceneManager.LoadScene(mainSceneName);
    }
    public void SinglePlayerButtonClick()
    {
        NetworkInfos.IsMultiplayerGame = false;
        NetworkInfos.IsHost = true;
        NetworkInfos.StartedFromMainMenu = true;
        SceneManager.LoadScene(mainSceneName);
    }
}
