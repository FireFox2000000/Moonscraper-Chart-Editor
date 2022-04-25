#if UNITY_EDITOR

public class NativeWindow_Editor : INativeWindow
{
    public bool IsConnectedToWindow()
    {
        return false;
    }

    public bool SetApplicationWindowPointerByName(string desiredWindowName)
    {
        // Not supported
        return false;
    }

    public void SetWindowTitle(string title)
    {
        // Not supported
    }
}

#endif
