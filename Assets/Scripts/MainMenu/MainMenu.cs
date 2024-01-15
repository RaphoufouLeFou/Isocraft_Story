using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/*
public static class NetworkInfos
{
    public static bool IsMultiplayerGame { get; set; }
}
*/
public class MainMenu : MonoBehaviour
{

    [SerializeField] private string MainSceneName = "Main";
    [SerializeField] private GameObject MainParent;
    [SerializeField] private GameObject NewGameParent;
    [SerializeField] private GameObject LoadGameParent;
    [SerializeField] private GameObject BackButton;

    void Start()
    {
        BackButton.SetActive(false);
        MainParent.SetActive(true);
        NewGameParent.SetActive(false);
        LoadGameParent.SetActive(false);
    }
    void Update() { }

    public void NewGameButtonClick()
    {
        BackButton.SetActive(true);
        MainParent.SetActive(false);
        NewGameParent.SetActive(true);
    }

    public void BackButtonClick()
    {
        MainParent.SetActive(true);
        NewGameParent.SetActive(false);
        LoadGameParent.SetActive(false);
        BackButton.SetActive(false);
    }

    public void LoadGameButtonClick()
    {
        BackButton.SetActive(true);
        MainParent.SetActive(false);
        LoadGameParent.SetActive(true);
    }
    public void JoinGameButtonClick()
    {
        NetworkInfos.IsMultiplayerGame = true;
        NetworkInfos.IsHost = false;
        NetworkInfos.StartedFromMainMenu = true;
        SceneManager.LoadScene(MainSceneName);
    }
    public void MultiPlayerButtonClick()
    {
        NetworkInfos.IsMultiplayerGame = true;
        NetworkInfos.IsHost = true;
        NetworkInfos.StartedFromMainMenu = true;
        SceneManager.LoadScene(MainSceneName);
    }
    public void SinglePlayerButtonClick()
    {
        NetworkInfos.IsMultiplayerGame = false;
        NetworkInfos.IsHost = true;
        NetworkInfos.StartedFromMainMenu = true;
        SceneManager.LoadScene(MainSceneName);
    }
}
