using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INativeWindow
{
    bool IsValid();
    bool SetApplicationWindowPointerByName(string desiredWindowName);
    void SetWindowTitle(string title);
}
