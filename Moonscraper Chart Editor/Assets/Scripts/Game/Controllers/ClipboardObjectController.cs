// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using MoonscraperChartEditor.Song;

public class ClipboardObjectController : Snapable {
    public static string CLIPBOARD_FILE_LOCATION { get; private set; }

    public GroupSelect groupSelectTool;
    public Transform strikeline;
    public static Clipboard clipboard = new Clipboard();
    Renderer ren;

    uint pastePos = 0;

    protected override void Awake()
    {
        base.Awake();
        ren = GetComponent<Renderer>();
        editor.events.editorStateChangedEvent.Register(OnApplicationModeChanged);
        CLIPBOARD_FILE_LOCATION = UnityEngine.Application.persistentDataPath + "/MoonscraperClipboard.bin";
    }

    new void LateUpdate()
    {
        if (editor.services.mouseMonitorSystem.world2DPosition != null && Input.mousePosition.y < Camera.main.WorldToScreenPoint(editor.mouseYMaxLimit.position).y)
        {
            pastePos = objectSnappedChartPos;
        }
        else
        {
            pastePos = editor.currentSong.WorldPositionToSnappedTick(strikeline.position.y, Globals.gameSettings.step);
        }

        transform.position = new Vector3(transform.position.x, editor.currentSong.TickToWorldYPosition(pastePos), transform.position.z);

        if (!Services.IsTyping && MSChartEditorInput.GetInputDown(MSChartEditorInputActions.ClipboardPaste))
        {
            Paste(pastePos);
            groupSelectTool.reset();
        }
    }

    void OnApplicationModeChanged(in ChartEditor.State editorState)
    {
        // Can only paste in editor mode
        gameObject.SetActive(editorState == ChartEditor.State.Editor);
    }

    public static void SetData(SongObject[] data, Clipboard.SelectionArea area, Song song)
    {
        clipboard = new Clipboard();
        clipboard.data = data;
        clipboard.resolution = song.resolution;
        clipboard.instrument = MenuBar.currentInstrument;
        clipboard.SetCollisionArea(area, song);
        //System.Windows.Forms.Clipboard.SetDataObject("", false);   // Clear the clipboard to mimic the real clipboard. For some reason putting custom objects on the clipboard with this dll doesn't work.

        try
        {
            FileStream fs = null;
            
            try
            {
                fs = new FileStream(CLIPBOARD_FILE_LOCATION, FileMode.Create, FileAccess.ReadWrite);
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, clipboard);
            }
            catch (SerializationException e)
            {
                Logger.LogException(e, "Failed to serialize");
            }
            catch (System.Exception e)
            {
                Logger.LogException(e, "Failed to serialize in general");
            }
            finally
            {
                if (fs != null)
                    fs.Close();
                else
                    Debug.LogError("Filestream when writing clipboard data failed to initialise");
            }
        }
        catch (System.Exception e)
        {
            Logger.LogException(e, "Failed to copy data");
        }
    }

    // Paste the clipboard data into the chart, overwriting anything else in the process
    public void Paste(uint chartLocationToPaste)
    {
        //if (System.Windows.Forms.Clipboard.GetDataObject().GetFormats().Length > 0 && 
        //    !(
        //        System.Windows.Forms.Clipboard.ContainsText(TextDataFormat.UnicodeText) && 
        //        System.Windows.Forms.Clipboard.ContainsText(TextDataFormat.Text) && 
        //        System.Windows.Forms.Clipboard.GetText() == "")
        //    )     // Something else is pasted on the clipboard instead of Moonscraper stuff.
        //    return;

        FileStream fs = null;
        clipboard = null;
        try
        {
            // Read clipboard data from a file instead of the actual clipboard because the actual clipboard doesn't work for whatever reason
            fs = new FileStream(CLIPBOARD_FILE_LOCATION, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();

            clipboard = (Clipboard)formatter.Deserialize(fs);
        }
        catch (System.Exception e)
        {
            Logger.LogException(e, "Failed to read from clipboard file");
            clipboard = null;
        }
        finally
        {
            if (fs != null)
                fs.Close();
            else
                Debug.LogError("Filestream when reading clipboard data failed to initialise");
        }

        if (editor.currentState == ChartEditor.State.Editor && clipboard != null && clipboard.data.Length > 0)
        {
            List<SongEditCommand> commands = new List<SongEditCommand>();

            Rect collisionRect = clipboard.GetCollisionRect(chartLocationToPaste, editor.currentSong);
            if (clipboard.areaChartPosMin > clipboard.areaChartPosMax)
            {
                Debug.LogError("Clipboard minimum (" + clipboard.areaChartPosMin + ") is greater than clipboard the max (" + clipboard.areaChartPosMax + ")");
            }
            uint colliderChartDistance = TickFunctions.TickScaling(clipboard.areaChartPosMax - clipboard.areaChartPosMin, clipboard.resolution, editor.currentSong.resolution);

            editor.globals.ToggleSongViewMode(!clipboard.data[0].GetType().IsSubclassOf(typeof(ChartObject)));

            {
                List<SongObject> newObjectsToDelete = new List<SongObject>();

                // Overwrite any objects in the clipboard space
                if (clipboard.data[0].GetType().IsSubclassOf(typeof(ChartObject)))
                {
                    foreach (ChartObject chartObject in editor.currentChart.chartObjects)
                    {
                        if (chartObject.tick >= chartLocationToPaste && chartObject.tick <= (chartLocationToPaste + colliderChartDistance) && PrefabGlobals.HorizontalCollisionCheck(PrefabGlobals.GetCollisionRect(chartObject), collisionRect))
                        {
                            newObjectsToDelete.Add(chartObject);
                        }
                    }
                }
                else
                {
                    // Overwrite synctrack, leave sections alone
                    foreach (SyncTrack syncObject in editor.currentSong.syncTrack)
                    {
                        if (syncObject.tick >= chartLocationToPaste && syncObject.tick <= (chartLocationToPaste + colliderChartDistance) && PrefabGlobals.HorizontalCollisionCheck(PrefabGlobals.GetCollisionRect(syncObject), collisionRect))
                        {
                            newObjectsToDelete.Add(syncObject);
                        }
                    }
                }

                if (newObjectsToDelete.Count > 0)
                {
                    commands.Add(new SongEditDelete(newObjectsToDelete));
                }
            }

            {
                uint maxLength = editor.currentSong.TimeToTick(editor.currentSongLength, editor.currentSong.resolution);

                List<SongObject> newObjectsToAddIn = new List<SongObject>();

                // Paste the new objects in
                foreach (SongObject clipboardSongObject in clipboard.data)
                {
                    SongObject objectToAdd = clipboardSongObject.Clone();

                    objectToAdd.tick = chartLocationToPaste +
                        TickFunctions.TickScaling(clipboardSongObject.tick, clipboard.resolution, editor.currentSong.resolution) -
                        TickFunctions.TickScaling(clipboard.areaChartPosMin, clipboard.resolution, editor.currentSong.resolution);

                    if (objectToAdd.tick >= maxLength)
                        break;

                    if (objectToAdd.GetType() == typeof(Note))
                    {
                        Note note = (Note)objectToAdd;

                        if (clipboard.instrument == Song.Instrument.GHLiveGuitar || clipboard.instrument == Song.Instrument.GHLiveBass)
                        {
                            // Pasting from a ghl track
                            if (!Globals.ghLiveMode)
                            {
                                if (note.ghliveGuitarFret == Note.GHLiveGuitarFret.Open)
                                    note.guitarFret = Note.GuitarFret.Open;
                                else if (note.ghliveGuitarFret == Note.GHLiveGuitarFret.White3)
                                    continue;
                            }
                        }
                        else if (Globals.ghLiveMode)
                        {
                            // Pasting onto a ghl track
                            if (note.guitarFret == Note.GuitarFret.Open)
                                note.ghliveGuitarFret = Note.GHLiveGuitarFret.Open;
                        }

                        note.length = TickFunctions.TickScaling(note.length, clipboard.resolution, editor.currentSong.resolution);
                    }
                    else if (objectToAdd.GetType() == typeof(Starpower))
                    {
                        Starpower sp = (Starpower)objectToAdd;
                        if (editor.currentInstrument != Song.Instrument.Drums)
                        {
                            sp.flags &= ~Starpower.Flags.ProDrums_Activation;
                        }
                        sp.length = TickFunctions.TickScaling(sp.length, clipboard.resolution, editor.currentSong.resolution);
                    }

                    newObjectsToAddIn.Add(objectToAdd);
                }

                if (newObjectsToAddIn.Count > 0)
                {
                    commands.Add(new SongEditAdd(newObjectsToAddIn));
                }
            }

            if (commands.Count > 0)
            {
                BatchedSongEditCommand batchedCommands = new BatchedSongEditCommand(commands);
                editor.commandStack.Push(batchedCommands);
            }
        }
        // 0 objects in clipboard, don't bother pasting
    }
}
