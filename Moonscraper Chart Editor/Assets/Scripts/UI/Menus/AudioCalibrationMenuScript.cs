using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AudioCalibrationMenuScript : DisplayMenu {
    public InputField audioInput;

    protected override void OnEnable()
    {
        base.OnEnable();
        audioInput.text = Globals.audioCalibrationMS.ToString();
    }
	
    public void audioValChanged(string val)
    {
        if (val != string.Empty && val != "-")
            Globals.audioCalibrationMS = int.Parse(val);
    }

    public void audioValEndEdit(string val)
    {
        if (val == string.Empty && val == "-")
            Globals.audioCalibrationMS = 0;
        else
            audioValChanged(val);
    }
}
