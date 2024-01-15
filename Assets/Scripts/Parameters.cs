
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
