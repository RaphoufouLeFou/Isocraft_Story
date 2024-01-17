using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class MainMenu : MonoBehaviour
{

    [SerializeField] private string mainSceneName = "Main";
    [SerializeField] private GameObject mainParent;
    [SerializeField] private GameObject newGameParent;
    [SerializeField] private GameObject loadGameParent;
    [SerializeField] private GameObject JoinGameParent;
    [SerializeField] private GameObject backButton;

    private string _iPaddress = "localhost";
    private string _port = "7777";

    void Start()
    {
        backButton.SetActive(false);
        mainParent.SetActive(true);
        newGameParent.SetActive(false);
        loadGameParent.SetActive(false);
        JoinGameParent.SetActive(false);
    }

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
        JoinGameParent.SetActive(false);
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
        backButton.SetActive(true);
        mainParent.SetActive(false);
        JoinGameParent.SetActive(true);
    }
    public void ConnectGameButtonClick()
    {
        if (_iPaddress.ToLower() == "localhost") NetworkInfos.IsLocalHost = true;
        else NetworkInfos.IsLocalHost = false;
        NetworkInfos.uri = new Uri($"https://{_iPaddress}:{_port}");
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

    public void OnChangedAdress(TMP_Text newAdress)
    {
        _iPaddress = newAdress.text;
    }
    public void OnChangedPort(TMP_Text newPort)
    {
        _port = newPort.text;
    }
}
