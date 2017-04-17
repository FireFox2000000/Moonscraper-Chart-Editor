using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AudioCalibrationMenuScript : DisplayMenu {
    public InputField audioInput;
    public InputField clapInput;

    protected override void OnEnable()
    {
        base.OnEnable();
        audioInput.text = Globals.audioCalibrationMS.ToString();
        clapInput.text = Globals.clapCalibrationMS.ToString();
    }
	
    public void audioValChanged(string val)
    {
        valChanged(val, ref Globals.audioCalibrationMS);
    }

    public void audioValEndEdit(string val)
    {
        valEndEdit(val, ref Globals.audioCalibrationMS);
    }

    public void sfxValChanged(string val)
    {
        valChanged(val, ref Globals.clapCalibrationMS);
    }

    public void sfxValEndEdit(string val)
    {
        valEndEdit(val, ref Globals.clapCalibrationMS);
    }

    void valChanged(string val, ref int calibration)
    {
        if (val != string.Empty && val != "-")
            calibration = int.Parse(val);
    }

    void valEndEdit(string val, ref int calibration)
    {
        if (val == string.Empty && val == "-")
            calibration = 0;
        else
            valChanged(val, ref calibration);
    }
}
