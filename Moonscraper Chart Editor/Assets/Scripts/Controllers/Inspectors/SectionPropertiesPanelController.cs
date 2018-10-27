// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SectionPropertiesPanelController : PropertiesPanelController {
    public Section currentSection { get { return (Section)currentSongObject; }
        set {
            currentSongObject = value;
        }
    }
    public InputField sectionName;

    protected override void Update()
    {
        base.Update();
        if (currentSection != null)
        {
            positionText.text = "Position: " + currentSection.tick.ToString();
            sectionName.text = currentSection.title;
        }
        else
        {
            Debug.LogError("Null section");
        }
    }

    void OnEnable()
    {       
        bool edit = ChartEditor.isDirty;

        if (currentSection != null)
            sectionName.text = currentSection.title;

        ChartEditor.isDirty = edit;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        currentSection = null;
    }

    public void UpdateSectionName (string name)
    {
        bool tentativeRecord, lockedRecord;
        ShouldRecordInputField(name, currentSection.title, out tentativeRecord, out lockedRecord);

        if (!lockedRecord)
        {
            editor.commandStack.Pop();
        }

        if (tentativeRecord || lockedRecord)
        {
            Section newSection = new Section(name, currentSection.tick);
            editor.commandStack.Push(new SongEditModify<Section>(currentSection, newSection));
            int insertionIndex = SongObjectHelper.FindObjectPosition(newSection, editor.currentSong.sections);
            Debug.Assert(insertionIndex != SongObjectHelper.NOTFOUND, "Song event failed to be inserted?");
            editor.currentSelectedObject = editor.currentSong.sections[insertionIndex];
        }
    }
}
