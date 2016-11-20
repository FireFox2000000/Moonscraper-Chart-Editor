using UnityEngine;
using System.Collections;
//using System.Windows.Forms;
//using System.Threading;
using System.Runtime.InteropServices;

public class Toolpane : MonoBehaviour {
    public static Tools currentTool = Tools.Cursor;

    void Start()
    {
    }

    public enum Tools
    {
        Cursor, Eraser, Note
    }
}
