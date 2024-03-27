using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class SettingsUI : MonoBehaviour
{
    private void Start()
    {
        _path = Application.persistentDataPath + "/options.txt";
        LoadSettings();
        // close all menus
        GoToMenu("None");
    }

    // menu GameObjects
    public GameObject settingsMenu;
    public GameObject settingsMenuButtons;
    public GameObject overlayMenu;
    public GameObject controlsMenu;
    public GameObject multiplayerMenu;
    public GameObject inventoryMenu;
    public GameObject pauseMenu;
    public GameObject pressKeyText;
    public GameObject scrollParent;
    public GameObject chatWindow;

    public NetworkManager manager;

    private string _path;

    private int _maxPlayerConnections = 20;

    // variables when assigning a key
    private bool _isReadingKey;
    private string _key;
    private TMP_Text _keyText;

    // unity button event functions
    public void ToggleFps() { Settings.Overlay.DisplayFps = !Settings.Overlay.DisplayFps; }
    public void ToggleMs() { Settings.Overlay.DisplayMs = !Settings.Overlay.DisplayMs; }
    public void ToggleCoordinates() { Settings.Overlay.DisplayCoords = !Settings.Overlay.DisplayCoords; }
    public void ToggleSaveName() { Settings.Overlay.DisplaySaveName = !Settings.Overlay.DisplaySaveName; }
    public void AssignKey(GameObject obj)
    {
        _isReadingKey = true;
        pressKeyText.SetActive(true); // show the "press a key" text
        _key = obj.name; // the function name is also the name of the UI input
        // get the pressed key Text to hide it later
        _keyText = obj.transform.GetChild(1).GetComponentInChildren<TMP_Text>();
    }

    public void GoToMenu(string menu)
    {
        if (menu == "None") // close menus
        {
            Settings.Playing = true;
            Settings.IsPaused = false;
            pauseMenu.SetActive(false);
            SaveSettings();
            return;
        }

        Settings.Playing = false;
        Settings.IsPaused = true;

        bool pause = menu == "Pause";
        pauseMenu.SetActive(pause);
        settingsMenu.SetActive(!pause);

        settingsMenuButtons.SetActive(menu == "Settings");
        overlayMenu.SetActive(menu == "Overlay");
        controlsMenu.SetActive(menu == "Controls");
        multiplayerMenu.SetActive(menu == "Multiplayer");
        if (menu == "Multiplayer")
        {
            string encP = NetworkManager.EncodeIP(NetworkManager.GetLocalIPv4());
            string encL = NetworkManager.EncodeIP(NetworkManager.GetLocalIPv4L());
            GameObject.Find("GameCodeMultiP").GetComponent<TMP_Text>().text = $"Public : {encP}";
            GameObject.Find("GameCodeMultiL").GetComponent<TMP_Text>().text = $"Lan : {encL}";
        }

        pressKeyText.SetActive(false);
    }

    private void SaveSettings()
    {
        string s = $"Ms:{Settings.Overlay.DisplayMs}\n";
        s += $"Fps:{Settings.Overlay.DisplayFps}\n";
        s += $"Coords:{Settings.Overlay.DisplayCoords}\n";
        s += $"SaveName:{Settings.Overlay.DisplaySaveName}\n";
        foreach (KeyValuePair<string, KeyCode> key in Settings.KeyMap) s += $"{key.Key}:{(int)key.Value}\n";
        if (File.Exists(_path)) File.Delete(_path);
        File.WriteAllText(_path, s);
    }

    private void UpdateSceneSettings()
    {
        // overlay
        Transform overlay = overlayMenu.transform;
        overlay.GetChild(0).GetChild(0).gameObject.GetComponent<Toggle>().isOn = Settings.Overlay.DisplayFps;
        overlay.GetChild(1).GetChild(0).gameObject.GetComponent<Toggle>().isOn = Settings.Overlay.DisplayMs;
        overlay.GetChild(2).GetChild(0).gameObject.GetComponent<Toggle>().isOn = Settings.Overlay.DisplayCoords;
        overlay.GetChild(3).GetChild(0).gameObject.GetComponent<Toggle>().isOn = Settings.Overlay.DisplaySaveName;

        // keys
        Transform t = scrollParent.transform;
        for (int i = 0; i < Settings.KeyMap.Count; i++)
        {
            Transform child = t.GetChild(i);
            child.GetChild(1).GetComponentInChildren<TMP_Text>().text =
                Settings.KeyMap[child.gameObject.name].ToString();
        }
    }

    private void LoadSettings()
    {
        // set all settings to default
        Settings.Overlay.DisplayMs = true;
        Settings.Overlay.DisplayFps = true;
        Settings.Overlay.DisplayCoords = true;
        Settings.Overlay.DisplaySaveName = true;

        Settings.Game.FastGraphics = false;
        Settings.Game.AutoSaveDelay = 15;
        Settings.Game.RenderDistance = 5;

        Settings.KeyMap = new Dictionary<string, KeyCode>
        {
            { "Forwards", KeyCode.W },
            { "Backwards", KeyCode.S },
            { "Left", KeyCode.A },
            { "Right", KeyCode.D },
            { "Jump", KeyCode.Space },
            { "Sprint", KeyCode.LeftControl },
            { "Inventory", KeyCode.E },
            { "Chat", KeyCode.T },
            { "Kill", KeyCode.K },
            { "Respawn", KeyCode.R }
        };

        string[] keys =
        {
            "Ms", "Fps", "Coords", "Forwards", "Backwards", "Left", "Right", "Jump", "Sprint", "Inventory", "Chat",
            "Kill", "Respawn"
        }; // accepted keys in save file

        // replace by existing settings in file
        if (File.Exists(_path))
        {
            StreamReader file = new StreamReader(_path);
            while (file.ReadLine() is { } line)
            {
                // check if valid line and edit settings
                int i;
                for (i = 0; i < line.Length; i++) if (line[i] == ':') break;
                if (i == line.Length) continue;
                string key = line.Substring(0, i), value = line.Substring(i + 1);
                if (!keys.Contains(key)) continue;
                if (key == "Ms") Settings.Overlay.DisplayMs = value == "True";
                else if (key == "Fps") Settings.Overlay.DisplayFps = value == "True";
                else if (key == "Coords") Settings.Overlay.DisplayCoords = value == "True";
                else if (key == "SaveName") Settings.Overlay.DisplaySaveName = value == "True";
                else if (int.TryParse(value, out i)) Settings.KeyMap[key] = (KeyCode)i;
            }

            file.Close();
        }

        // update fields in scene
        UpdateSceneSettings();
        // save fixed settings to file
        SaveSettings();
    }

    public void MultiplayerMenuListener(GameObject self)
    {
        if (self.name == "IsMulti")
        {
            SuperGlobals.IsMultiplayerGame = self.GetComponent<Toggle>().isOn;
            manager.ChangeMaxConnection(SuperGlobals.IsMultiplayerGame ? _maxPlayerConnections : 1);
        }

        else if (self.name == "Port")
            manager.ChangePort((ushort)Int32.Parse(self.GetComponent<TMP_InputField>().text));
        else if (self.name == "Players")
        {
            _maxPlayerConnections = Int32.Parse(self.GetComponent<TMP_InputField>().text);
            manager.ChangeMaxConnection(_maxPlayerConnections);
        }
    }

    private void Update()
    {
        // handle escape key for all popups, including chat and inventory
        if (Input.GetKeyDown(KeyCode.Escape) && !_isReadingKey) // avoid checking when assigning a key
        {
            if (Settings.Playing) GoToMenu("Pause"); // show the pause menu
            else // exit any popup, or go back to parent popup
            {
                if (
                    pauseMenu.activeSelf
                    || inventoryMenu.activeSelf
                    || chatWindow.activeSelf
                    // popups
                    )
                {
                    pauseMenu.SetActive(false);
                    GameObject scripts = GameObject.Find("Scripts");
                    scripts.GetComponent<InventoryUI>().HideInventory();
                    chatWindow.SetActive(false);
                    Settings.Playing = true;
                    Settings.IsPaused = false;
                }

                else if (settingsMenuButtons.activeSelf) GoToMenu("Pause");
                else if (
                    overlayMenu.activeSelf
                    || controlsMenu.activeSelf
                    || multiplayerMenu.activeSelf
                    // submenus
                ) GoToMenu("Settings");
            }
        }

        if (!_isReadingKey) return; // if is not in key assign mode, the update is done

        if (Input.anyKey) // if a key is detected
            foreach(KeyCode key in Enum.GetValues(typeof(KeyCode))) // enum in all possible keys
                if (Input.GetKey(key))
                {
                    _isReadingKey = false;
                    pressKeyText.SetActive(_isReadingKey); //hide the text

                    if(key == KeyCode.Escape) return; // cancel if escape key is pressed

                    Settings.KeyMap[_key] = key; // map the key in the dictionary
                    _keyText.text = key.ToString();  // update the button text of the parameter
                }
    }
}
