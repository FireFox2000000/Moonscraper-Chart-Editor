// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

//#undef UNITY_EDITOR

public class WindowHandleManager {
    public NativeWindow nativeWindow = new NativeWindow();

    string productName; // Cross checking the window handle

    string originalWindowName;
    string projectName;
    string projectState;

    bool isDirty = false;
    System.Text.StringBuilder sb = new System.Text.StringBuilder();

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

    public void SetProjectNameStr(string info)
    {
        projectName = info;
        RepaintWindowText(this.isDirty);
    }

    public void SetProjectStateStr(string info)
    {
        projectState = info;
        RepaintWindowText(this.isDirty);
    }

    public void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !nativeWindow.IsConnectedToWindow())
            SetApplicationWindowPointer();
    }

    void RepaintWindowText(bool isDirty)
    {
        string dirtySymbol = isDirty ? " *" : "";
        sb.Clear();

        sb.AppendFormat("{0}{1} - {2} - {3}", projectName, dirtySymbol, projectState, originalWindowName);

        string windowName = sb.ToString();
        nativeWindow.SetWindowTitle(windowName);
        UnityEngine.Debug.Log("RepaintWindowText - " + windowName);
    }
}
