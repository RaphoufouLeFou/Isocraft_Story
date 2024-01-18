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
    [SerializeField] private GameObject SaveTextPrefab;
    
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
    public void MultiPlayerButtonClick(GameObject saveName)
    {
        string nm = saveName.GetComponent<TMP_InputField>().text;
        if(nm == "" || DoesSaveExist(nm)) return;
        SaveInfos.SaveName = nm;
        NetworkInfos.uri = new Uri($"kcp://{_IPAddress}:{_port}");
        NetworkInfos.IsMultiplayerGame = true;
        NetworkInfos.IsHost = true;
        NetworkInfos.StartedFromMainMenu = true;
        SceneManager.LoadScene(mainSceneName);
    }
    
    public void MultiPlayer(string saveName)
    {
        if(saveName == "") return;
        SaveInfos.SaveName = saveName;
        NetworkInfos.uri = new Uri($"kcp://{_IPAddress}:{_port}");
        NetworkInfos.IsMultiplayerGame = true;
        NetworkInfos.IsHost = true;
        NetworkInfos.StartedFromMainMenu = true;
        SceneManager.LoadScene(mainSceneName);
    }

    private bool DoesSaveExist(string saveName)
    {
        string path = Application.persistentDataPath + "/Saves/";
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

    public void LoadGame(string saveName)
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
        foreach (string file in Directory.EnumerateFiles(path))
        {
            string saveName = Path.GetFileName(file);
            if (!saveName.Contains(".IsoSave")) continue;
            saveName = saveName.Replace(".IsoSave", "");
            GameObject go = Instantiate(SaveTextPrefab, Vector3.zero, Quaternion.identity, content.transform);
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
        string path = Application.persistentDataPath + "/Saves/" + saveName + ".IsoSave";
        if(File.Exists(path)) File.Delete(path);
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
