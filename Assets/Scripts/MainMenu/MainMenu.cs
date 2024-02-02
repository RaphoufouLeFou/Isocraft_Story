using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private GameObject saveTextPrefab;
    
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private TMP_InputField portInput;
    [SerializeField] private TMP_InputField portNewGameInput;

    private string _ipAddress = "localhost";
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
        NetworkInfos.IsLocalHost = _ipAddress.ToLower() == "localhost";
        Debug.Log($"Address = {_ipAddress}");
        NetworkInfos.uri = new Uri($"kcp://{_ipAddress}:{_port}");
        NetworkInfos.IsMultiplayerGame = true;
        NetworkInfos.IsHost = false;
        NetworkInfos.StartedFromMainMenu = true;
        SceneManager.LoadScene(mainSceneName);
    }
    public void MultiPlayerButtonClick(GameObject saveName)
    {
        string nm = saveName.GetComponent<TMP_InputField>().text;
        if(nm == "" || DoesSaveExist(nm)) return;
        SaveInfos.SaveName = nm;
        NetworkInfos.uri = new Uri($"kcp://{_ipAddress}:{_port}");
        NetworkInfos.IsMultiplayerGame = true;
        NetworkInfos.IsHost = true;
        NetworkInfos.StartedFromMainMenu = true;
        SceneManager.LoadScene(mainSceneName);
    }
    
    private void MultiPlayer(string saveName)
    {
        if(saveName == "") return;
        SaveInfos.SaveName = saveName;
        NetworkInfos.uri = new Uri($"kcp://{_ipAddress}:{_port}");
        NetworkInfos.IsMultiplayerGame = true;
        NetworkInfos.IsHost = true;
        NetworkInfos.StartedFromMainMenu = true;
        SceneManager.LoadScene(mainSceneName);
    }

    private bool DoesSaveExist(string saveName)
    {
        string path = Application.persistentDataPath + "/Saves/" + saveName + "/";
        return File.Exists(path + saveName + ".IsoSave");
    }
    
    public void SinglePlayerButtonClick(GameObject saveName)
    {
        string nm = saveName.GetComponent<TMP_InputField>().text;
        if(nm == "" || DoesSaveExist(nm)) return;
        SaveInfos.SaveName = nm;
        NetworkInfos.IsMultiplayerGame = false;
        NetworkInfos.IsHost = true;
        NetworkInfos.StartedFromMainMenu = true;
        SceneManager.LoadScene(mainSceneName);
    }

    private void LoadGame(string saveName)
    {
        MultiPlayer(saveName);
    }

    public void RefreshSaveList(GameObject content)
    {
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        string path = Application.persistentDataPath + "/Saves/";
        if(!Directory.Exists(path)) return;
        foreach (string dir in Directory.EnumerateDirectories(path))
        {
            string saveName = new DirectoryInfo(dir).Name;
            GameObject go = Instantiate(saveTextPrefab, Vector3.zero, Quaternion.identity, content.transform);
            go.GetComponentInChildren<TMP_Text>().text = saveName;
            go.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                LoadGame(saveName);
            });
            go.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                DeleteSave(saveName);
                RefreshSaveList(content);
            });
        }
    }

    private void DeleteSave(string saveName)
    {
        string path = Application.persistentDataPath + "/Saves/" + saveName + "/";
        foreach (string file in Directory.EnumerateFiles(path))
        {
            File.Delete(file);
        }
        if(Directory.Exists(path)) Directory.Delete(path);
    }

    public void OnChangedAddress()
    {
        _ipAddress = addressInput.text;
        Debug.Log($"Entered address = {_ipAddress}");
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
