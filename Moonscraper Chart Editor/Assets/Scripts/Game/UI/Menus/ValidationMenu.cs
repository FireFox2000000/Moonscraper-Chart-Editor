// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValidationMenu : DisplayMenu
{
    public string errorMessage = "Press the \"Validate\" button to begin validation";
    public Text errorText;
    public Toggle validateGH3;
    public Toggle validateCH;
    public Toggle autoValidateSongOnSave;

    SongValidate.ValidationOptions _currentOptions = ~SongValidate.ValidationOptions.None;
    SongValidate.ValidationOptions currentOptions
    {
        get { return _currentOptions; }
        set
        {
            _currentOptions = value;
            Globals.gameSettings.songValidatorModes = value;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        errorText.text = errorMessage;

        currentOptions = Globals.gameSettings.songValidatorModes;
        validateGH3.isOn = (currentOptions & SongValidate.ValidationOptions.GuitarHero3) != 0;
        validateCH.isOn = (currentOptions & SongValidate.ValidationOptions.CloneHero) != 0;
        autoValidateSongOnSave.isOn = Globals.gameSettings.autoValidateSongOnSave;

        ValidateSong();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    public void ValidateSong()
    {
        bool hasErrors;
        SongValidate.ValidationParameters validateParams = new SongValidate.ValidationParameters() {
            songLength = editor.currentSongLength,
            checkMidiIssues = true,
        };
        errorText.text = SongValidate.GenerateReport(currentOptions, editor.currentSong, validateParams, out hasErrors);
    }

    void SetValidateOptions(bool value, SongValidate.ValidationOptions setting)
    {
        if (value)
            currentOptions |= setting;
        else
            currentOptions &= ~setting;
    }

    public void SetValidateGH3Toggle(bool value)
    {
        SetValidateOptions(value, SongValidate.ValidationOptions.GuitarHero3);
        ValidateSong();
    }

    public void SetValidateCHToggle(bool value)
    {
        SetValidateOptions(value, SongValidate.ValidationOptions.CloneHero);
        ValidateSong();
    }

    public void SetAutoValidateSongOnSave(bool value)
    {
        Globals.gameSettings.autoValidateSongOnSave = value;
    }
}
