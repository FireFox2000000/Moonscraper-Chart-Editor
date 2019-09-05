//#undef UNITY_EDITOR

public class WindowHandleManager {
    public NativeWindow nativeWindow = new NativeWindow();

    string productName; // Cross checking the window handle

    string originalWindowName;
    string extraApplicationStateInfo;

    bool isDirty = false;

    public WindowHandleManager(string originalWindowName, string productName)
    {
        this.originalWindowName = originalWindowName;
        this.productName = productName;

        SetApplicationWindowPointer();
    }

    void SetApplicationWindowPointer()
    {
        if (nativeWindow.SetApplicationWindowPointerByName(productName))
        {
            RepaintWindowText(false);
        }
#if !UNITY_EDITOR
        else
        {
            UnityEngine.Debug.LogError("Couldn't find window handle");
        }
#endif
    }

    public void UpdateDirtyNotification(bool isDirty)
    {
        if (this.isDirty != isDirty)
        {
            RepaintWindowText(isDirty);
        }

        this.isDirty = isDirty;
    }

    public void SetExtraApplicationStateInfoStr(string info)
    {
        extraApplicationStateInfo = info;
        RepaintWindowText(this.isDirty);
    }

    public void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !nativeWindow.IsValid())
            SetApplicationWindowPointer();
    }

    void RepaintWindowText(bool isDirty)
    {
        string dirtySymbol = isDirty ? "*" : "";
        string windowName = originalWindowName + " ~ " + extraApplicationStateInfo + dirtySymbol;

        UnityEngine.Debug.Log("RepaintWindowText - " + windowName);

        nativeWindow.SetWindowTitle(windowName);
    }
}
