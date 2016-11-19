using UnityEngine;
using System.Collections;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

public class Toolpane : MonoBehaviour {
    Form toolpane;
    Thread toolThread;
    ThreadStart toolStart;

    Form modepane;
    Thread modeThread;
    ThreadStart modeStart;

    void Start()
    {
        
        // Create forms
        toolpane = new Form();
        toolpane.Text = "Tools";
        //toolpane.FormBorderStyle = FormBorderStyle.FixedSingle;
        //toolpane.MaximizeBox = false;
        
        modepane = new Form();
        modepane.Text = "Mode";
        
        // Create threads
        toolStart = new ThreadStart(displayForm1);
        toolThread = new Thread(toolStart);

        // Create threads
        modeStart = new ThreadStart(displayForm2);
        modeThread = new Thread(modeStart);

        toolThread.Start();
        //modeThread.Start();
    }

    delegate void updateFormDelegate(bool focus);

    void updateFocus(bool focus)
    {
        toolpane.TopMost = focus;
        modepane.TopMost = focus;
    }

    void OnApplicationPause(bool hasFocus)
    {
        //if (toolpane.InvokeRequired)
        //{
        //toolpane.BeginInvoke(new updateFormDelegate(updateFocus), hasFocus);
        if (toolpane != null)
        {
            toolpane.TopMost = !hasFocus;
            toolpane.Refresh();
        }
        //}
        //modepane.Invoke(new updateFormDelegate(updateFocus), hasFocus);
    }

    void displayForm1()
    {
        System.Windows.Forms.Application.Run(toolpane);
    }

    void displayForm2()
    {
        System.Windows.Forms.Application.Run(modepane);
    }

    void OnApplicationQuit()
    {
        if (toolpane != null)
            toolpane.Close();
        if (modepane != null)
            modepane.Close();
    }
}
