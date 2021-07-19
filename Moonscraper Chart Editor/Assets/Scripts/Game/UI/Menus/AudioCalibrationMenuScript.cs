// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AudioCalibrationMenuScript : DisplayMenu {
    public InputField audioInput;
    public InputField clapInput;

    protected override void OnEnable()
    {
        base.OnEnable();
        audioInput.text = Globals.gameSettings.audioCalibrationMS.ToString();
        clapInput.text = Globals.gameSettings.clapCalibrationMS.ToString();
    }
	
    public void audioValChanged(string val)
    {
        valChanged(val, ref Globals.gameSettings.audioCalibrationMS);
    }

    public void audioValEndEdit(string val)
    {
        valEndEdit(val, ref Globals.gameSettings.audioCalibrationMS);
    }

    public void sfxValChanged(string val)
    {
        valChanged(val, ref Globals.gameSettings.clapCalibrationMS);
    }

    public void sfxValEndEdit(string val)
    {
        valEndEdit(val, ref Globals.gameSettings.clapCalibrationMS);
    }

    void valChanged(string val, ref GameSettings.IntSaveSetting calibration)
    {
        if (val != string.Empty && val != "-")
            calibration.value = int.Parse(val);
    }

    void valEndEdit(string val, ref GameSettings.IntSaveSetting calibration)
    {
        if (val == string.Empty && val == "-")
            calibration.value = 0;
        else
            valChanged(val, ref calibration);
    }
}
