using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class Parameters
{       

    public static Dictionary<string, KeyCode> KeyMap = new Dictionary<string, KeyCode>(); 
    public struct Overlay
    {
        public bool DisplayFps;
        public bool DisplayMspf;
        public bool DisplayCoordonates;
    }

    public static Overlay OverlayParam;

    public static void InitParam()
    {
        OverlayParam.DisplayFps = true;
        OverlayParam.DisplayMspf = true;
        OverlayParam.DisplayCoordonates = true;
    }
}

public class ParamUI : MonoBehaviour
{
    public GameObject mainParamMenu;
    public GameObject mainParamMenuButtons;
    public GameObject overlayMenu;
    public GameObject controlsMenu;
    public GameObject pauseMenu;
    public GameObject pressKeyText;

    private bool _isReadingKey;
    private string _function;
    private TMP_Text _keyText;
    void Start()
    {
        mainParamMenu.SetActive(false);
        controlsMenu.SetActive(false);
        mainParamMenuButtons.SetActive(false);
        pressKeyText.SetActive(false);
        overlayMenu.SetActive(false);
        Parameters.InitParam();
        
        //defaults keys in qwerty:
        
        Parameters.KeyMap.Add("Forward", KeyCode.W);
        Parameters.KeyMap.Add("Backward", KeyCode.S);
        Parameters.KeyMap.Add("Left", KeyCode.A);
        Parameters.KeyMap.Add("Right", KeyCode.D);
        Parameters.KeyMap.Add("CamLeft", KeyCode.Q);
        Parameters.KeyMap.Add("CamRight", KeyCode.E);
        Parameters.KeyMap.Add("Kill", KeyCode.K);
        Parameters.KeyMap.Add("TopView", KeyCode.T);
        Parameters.KeyMap.Add("Spawn", KeyCode.R);
    }

    public void ToggleFps()
    {
        Parameters.OverlayParam.DisplayFps = !Parameters.OverlayParam.DisplayFps;
    }
    public void ToggleMspf()
    {
        Parameters.OverlayParam.DisplayMspf = !Parameters.OverlayParam.DisplayMspf;
    }
    public void ToggleCoordonates()
    {
        Parameters.OverlayParam.DisplayCoordonates = !Parameters.OverlayParam.DisplayCoordonates;
    }
    public void EnterOverlayParam()
    {
        overlayMenu.SetActive(true);
        mainParamMenuButtons.SetActive(false);
    }
    
    public void EnterControlsParam()
    {
        controlsMenu.SetActive(true);
        mainParamMenuButtons.SetActive(false);
    }


    public void ReturnToMainParam()
    {
        mainParamMenu.SetActive(true);
        overlayMenu.SetActive(false);
        controlsMenu.SetActive(false);
        pauseMenu.SetActive(false);
        mainParamMenuButtons.SetActive(true);
    }

    public void ReturnToPauseMenu()
    {
        mainParamMenu.SetActive(false);
        pauseMenu.SetActive(true);
        mainParamMenuButtons.SetActive(false);
    }

    public void AssignKey(GameObject function)
    {
        _isReadingKey = true;
        pressKeyText.SetActive(_isReadingKey);
        _function = function.name;      //For optimization, the function name is the name of the object in the UI
        _keyText = function.transform.GetChild(1).GetComponentInChildren<TMP_Text>(); //get the text of the button
    }

    private void Update()
    {
        if (!_isReadingKey) return;

        if (Input.anyKey)
        {
            foreach(KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(kcode))
                {
                    _isReadingKey = false;
                    pressKeyText.SetActive(_isReadingKey);
                    Parameters.KeyMap[_function] = kcode;
                    _keyText.text = kcode.ToString();
                    Debug.Log(_function + " = " +  kcode);
                    Debug.Log(Parameters.KeyMap[_function]);
                }
            }
        }
    }
}