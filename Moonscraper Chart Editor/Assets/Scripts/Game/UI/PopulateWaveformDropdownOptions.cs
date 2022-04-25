// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoonscraperEngine;
using MoonscraperChartEditor.Song;

[RequireComponent(typeof(Dropdown))]
public class PopulateWaveformDropdownOptions : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Dropdown waveformDropdown = GetComponent<Dropdown>();
        waveformDropdown.ClearOptions();
        List<Dropdown.OptionData> dropdownOptions = new List<Dropdown.OptionData>() { new Dropdown.OptionData("None") };

        foreach (Song.AudioInstrument audio in EnumX<Song.AudioInstrument>.Values)
        {
            dropdownOptions.Add(new Dropdown.OptionData(audio.ToString()));
        }

        waveformDropdown.AddOptions(dropdownOptions);
    }
}
