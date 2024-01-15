
using System;
using UnityEngine;
using UnityEngine.Serialization;

public static class Parameters
{
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
    public GameObject pauseMenu;
    void Start()
    {
        mainParamMenu.SetActive(false);
        mainParamMenuButtons.SetActive(false);
        overlayMenu.SetActive(false);
        Parameters.InitParam();
    }

    public static void ToggleFps()
    {
        Parameters.OverlayParam.DisplayFps = !Parameters.OverlayParam.DisplayFps;
    }
    public static void ToggleMspf()
    {
        Parameters.OverlayParam.DisplayMspf = !Parameters.OverlayParam.DisplayMspf;
    }
    public static void ToggleCoordonates()
    {
        Parameters.OverlayParam.DisplayCoordonates = !Parameters.OverlayParam.DisplayCoordonates;
    }

    public void EnterOverlayParam()
    {
        overlayMenu.SetActive(true);
        mainParamMenuButtons.SetActive(false);
    }

    public void ReturnToMainParam()
    {
        mainParamMenu.SetActive(true);
        overlayMenu.SetActive(false);
        pauseMenu.SetActive(false);
        mainParamMenuButtons.SetActive(true);
    }

    public void ReturnToPauseMenu()
    {
        mainParamMenu.SetActive(false);
        overlayMenu.SetActive(false);
        pauseMenu.SetActive(true);
        mainParamMenuButtons.SetActive(false);
    }

}