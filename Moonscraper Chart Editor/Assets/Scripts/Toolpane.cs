using UnityEngine;
using System.Collections;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

public class Toolpane : MonoBehaviour {
    Form toolpane;

    Thread toolThread;
    ThreadStart toolStart;

    void Start()
    {
        // Create forms
        toolpane = new Form();
        toolpane.Text = "Tools";
        toolpane.FormBorderStyle = FormBorderStyle.FixedSingle;
        toolpane.MaximizeBox = false;
        
        // Create threads
        toolStart = new ThreadStart(displayForm);
        toolThread = new Thread(toolStart);

        toolThread.Start();
    }

    void displayForm()
    {
#if UNITY_EDITOR
        //System.IntPtr hwnd = FindWindow(null, "Moonscraper");
        System.IntPtr hwnd = FindWindow(null, "Unity Personal (64bit) - test.unity - Moonscraper Chart Editor - PC, Mac & Linux Standalone <DX11>");
#else
        System.IntPtr hwnd = FindWindow(null, "Moonscraper Chart Editor");
#endif
        if (hwnd != System.IntPtr.Zero)
        {
            Debug.Log(new ArbitraryWindow(hwnd));

            toolpane.ShowDialog(new ArbitraryWindow(hwnd));
            //toolpane.ShowDialog(Control.FromHandle(hwnd));  
        } 
    }

    void OnApplicationQuit()
    {
        toolpane.Close();
    }
   
    [DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern System.IntPtr FindWindow(string ClassName, string WindowText);

    class ArbitraryWindow : IWin32Window
    {
        public ArbitraryWindow(System.IntPtr handle) { Handle = handle; }
        public System.IntPtr Handle { get; private set; }
    }
}
