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
    [SerializeField] private GameObject joinGameParent;
    [SerializeField] private GameObject backButton;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private TMP_InputField portInput;
    [SerializeField] private TMP_InputField portNewGameInput;

    private string _IPAddress = "localhost";
    private string _port = "7777";

    void Start()
    {
        backButton.SetActive(false);
        mainParent.SetActive(true);
        newGameParent.SetActive(false);
        loadGameParent.SetActive(false);
        joinGameParent.SetActive(false);
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
        joinGameParent.SetActive(false);
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
        joinGameParent.SetActive(true);
    }
    public void ConnectGameButtonClick()
    {
        NetworkInfos.IsLocalHost = _IPAddress.ToLower() == "localhost";
        Debug.Log($"Address = {_IPAddress}");
        NetworkInfos.uri = new Uri($"kcp://{_IPAddress}:{_port}");
        NetworkInfos.IsMultiplayerGame = true;
        NetworkInfos.IsHost = false;
        NetworkInfos.StartedFromMainMenu = true;
        SceneManager.LoadScene(mainSceneName);
    }
    public void MultiPlayerButtonClick()
    {
        NetworkInfos.uri = new Uri($"kcp://{_IPAddress}:{_port}");
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

    public void OnChangedAddress()
    {
        _IPAddress = addressInput.text;
        Debug.Log($"Entered address = {_IPAddress}");
    }
    public void OnChangedPort()
    {
        _port = portInput.text;
    }
    
    public void OnChangedPortNewGame()
    {
        _port = portNewGameInput.text;
    }
}
