using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public string mainSceneName;
    public GameObject superGlobals;
    public GameObject mainParent;
    public GameObject newGameParent;
    public GameObject loadGameParent;
    public GameObject joinGameParent;
    public GameObject backButton;
    public GameObject saveTextPrefab;

    public TMP_InputField addressInput;
    public TMP_InputField codeInput;
    public TMP_InputField portInput;
    public TMP_InputField portNewGameInput;

    private string _ipAddress = "localhost";
    private string _code = "";
    private string _port = "7777";

    void Start()
    {
        backButton.SetActive(false);
        mainParent.SetActive(true);
        newGameParent.SetActive(false);
        loadGameParent.SetActive(false);
        joinGameParent.SetActive(false);

        // send SuperGlobals to main scene
        DontDestroyOnLoad(superGlobals);
        SuperGlobals.BackToMenu();
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
        if (_code != "")
        {
            (string ip, int err) = NetworkManagement.DecodeIP(_code.ToUpper().Replace("\n", "").Replace(" ",""));

            if (err != 0)
            {
                Debug.LogError($"Error : {err}");
                return;
            }
            _ipAddress = ip;
        }
        Debug.Log($"Address = {_ipAddress}");
        SuperGlobals.Uri = new Uri($"kcp://{_ipAddress}:{_port}");
        SuperGlobals.IsMultiplayerGame = true;
        SuperGlobals.IsHost = false;
        SceneManager.LoadScene(mainSceneName);
    }

    public void MultiPlayerButtonClick(GameObject save)
    {
        string saveName = save.GetComponent<TMP_InputField>().text;
        StartGame(saveName, true, true);
    }

    public void SinglePlayerButtonClick(GameObject save)
    {
        string saveName = save.GetComponent<TMP_InputField>().text;
        StartGame(saveName, false, true);
    }

    private bool IsValidNewSaveName(string saveName)
    {
        return Uri.IsWellFormedUriString(saveName, UriKind.Relative)
               && !saveName.StartsWith("CLIENT__")
               && !Directory.Exists($"/Saves/{saveName}/")
               && !saveName.Contains("/") && !saveName.Contains("\\");
    }

    private void StartGame(string saveName, bool multi, bool newSave)
    {
        saveName = saveName.Replace(' ', '_');
        string saveFile = Application.persistentDataPath + $"/Saves/{saveName}/{saveName}.IsoSave";
        if (!IsValidNewSaveName(saveName)) throw new ArgumentException("Save name is not valid");
        if (!File.Exists(saveFile) && !newSave) throw new ArgumentException("Save file not found in folder");

        SuperGlobals.IsNewSave = newSave;
        SuperGlobals.SaveName = saveName;
        SuperGlobals.Uri = new Uri($"kcp://{_ipAddress}:{_port}");
        SuperGlobals.IsMultiplayerGame = multi;
        SuperGlobals.IsHost = true;
        SceneManager.LoadScene(mainSceneName);
    }

    private void LoadGameButton(string saveName)
    {
        StartGame(saveName, true, false);
    }

    public void RefreshSaveList(GameObject content)
    {
        foreach (Transform child in content.transform) Destroy(child.gameObject);

        string path = $"{Application.persistentDataPath}/Saves/";
        if (!Directory.Exists(path)) return;

        foreach (string dir in Directory.EnumerateDirectories(path))
        {

            string saveName = new DirectoryInfo(dir).Name;
            if(saveName.StartsWith("CLIENT__")) continue;
            saveName = saveName.Replace('_' ,' ');
            GameObject go = Instantiate(saveTextPrefab, Vector3.zero, Quaternion.identity, content.transform);
            go.GetComponentInChildren<TMP_Text>().text = saveName;

            go.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => LoadGameButton(saveName));
            go.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
            {
                DeleteSave(saveName);
                RefreshSaveList(content);
            });
        }
    }

    private void DeleteSave(string saveName)
    {
        saveName = saveName.Replace(' ' ,'_');
        string path = Application.persistentDataPath + $"/Saves/{saveName}/";
        if (Directory.Exists(path)) Directory.Delete(path, true);
    }

    public void OnChangedAddress()
    {
        _ipAddress = addressInput.text;
    }

    public void OnChangedGCode()
    {
        _code = codeInput.text;
        _code = _code.ToUpper();
        codeInput.text = _code;
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
