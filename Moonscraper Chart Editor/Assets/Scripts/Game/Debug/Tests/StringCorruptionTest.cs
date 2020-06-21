// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using UnityEngine;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using MoonscraperChartEditor.Song;

// Purposefully generating as much garbage as possible, purely to test potential memory issues.
public class StringCorruptionTest : MonoBehaviour {
    ChartEditor editor;
    float startTime;
    Clipboard clipboard = new Clipboard();

    // Use this for initialization
    void Start () {
        editor = ChartEditor.Instance; 
    }

    void StartTest()
    {
        Debug.Log("Starting null internal string test");
        enabled = true;
        startTime = Time.realtimeSinceStartup;
        Random.InitState(0);
    }

    void EndTest()
    {
        Debug.Log("Ending null internal string test. Total time: " + (Time.realtimeSinceStartup - startTime));
        enabled = false;
    }

    public void StartStopTest()
    {
        if (enabled)
            EndTest();
        else
            StartTest();
    }

    private void OnEnable()
    {
        StartTest();
    }

    private void OnDisable()
    {
        EndTest();
    }

    // Update is called once per frame
    void Update () {
        Song song = editor.currentSong;
        float songLength = editor.currentSongLength;

        ReadClipboardFromFile();

        if (ValidateSectionsAndGetStopTest(song, 0))
        {
            EndTest();
            return;
        }

        ClearAllSections(song);
        AddRandomSections(song, songLength);

        if (ValidateSectionsAndGetStopTest(song, 1))
        {
            EndTest();
            return;
        }

        FillClipboard(song);
        WriteClipboardToFile(clipboard);
    }

    void ClearAllSections(Song song)
    {
        for(int i = song.sections.Count - 1; i >= 0; --i)
        {
            song.Remove(song.sections[i]);
        }
    }

    bool ValidateSectionsAndGetStopTest(Song song, int id)
    {
        bool stopTest = false;

        foreach (Section section in song.sections)
        {
            bool nullReferenceExceptionFound = false;
            string exceptionMessage;
            if (!ValidateString(section.title, ref nullReferenceExceptionFound, out exceptionMessage))
            {
                if (nullReferenceExceptionFound)
                {
                    Debug.LogError("Validation " + id + ". Null reference string failed validation. \nString was " + section.title + ". \n Message was " + exceptionMessage);
                    stopTest = true;
                    break;
                }
                else
                {
                    Debug.LogWarning("Validation " + id + ". String failed validation. String was " + section.title + ". \n Message was " + exceptionMessage);
                }
            }
        }

        return stopTest;
    }

    void AddRandomSections(Song song, float songLength)
    {
        uint maxTickPosition = song.TimeToTick(songLength, song.resolution);
        const int maxSections = 100;
        int numberOfSections = Random.Range(0, maxSections);

        for (int i = 0; i < numberOfSections; ++i)
        {
            string sectionName = GenerateRandomSectionName();
            uint tickPosition = (uint)Random.Range(0, (int)maxTickPosition);
            Section section = new Section(sectionName, tickPosition);
            song.Add(section);
        }
    }

    string GenerateRandomSectionName()
    {
        const int maxLength = 150;
        int stringLength = Random.Range(0, maxLength);
        string str = string.Empty;

        for (int i = 0; i < stringLength; ++i)
        {
            int charInt;
            do
            {
                charInt = Random.Range(char.MinValue, char.MaxValue);
            }
            while (charInt >= 0xD800 && charInt <= 0xDFFF);

            char character = (char)charInt;
            str += character;
        }

        return str;
    }

    bool ValidateString(string str, ref bool isNullReferenceException, out string exceptionMessage)
    {
        isNullReferenceException = false;
        exceptionMessage = string.Empty;

        try
        {
            UTF8Encoding.UTF8.GetByteCount(str);
        }
        catch (System.NullReferenceException e)
        {
            isNullReferenceException = true;
            exceptionMessage = e.Message;
            return false;
        }
        catch (System.Exception e)
        {
            exceptionMessage = e.Message;
            return false;
        }

        return true;
    }

    void FillClipboard(Song song)
    {
        Clipboard.SelectionArea area = new Clipboard.SelectionArea();
        clipboard = new Clipboard();
        
        clipboard.data = song.sections.ToArray();
        clipboard.resolution = song.resolution;
        clipboard.instrument = MenuBar.currentInstrument;
        clipboard.SetCollisionArea(area, song);
    }

    void WriteClipboardToFile(Clipboard clipboard)
    {
        try
        {
            FileStream fs = null;

            try
            {
                fs = new FileStream(ClipboardObjectController.CLIPBOARD_FILE_LOCATION, FileMode.Create, FileAccess.ReadWrite);
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

    void ReadClipboardFromFile()
    {
        FileStream fs = null;
        clipboard = null;

        try
        {
            // Read clipboard data from a file instead of the actual clipboard because the actual clipboard doesn't work for whatever reason
            fs = new FileStream(ClipboardObjectController.CLIPBOARD_FILE_LOCATION, FileMode.Open);
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

        if (clipboard != null)
        {
            foreach (SongObject clipboardSongObject in clipboard.data)
            {
                PlaceSongObject.AddObjectToCurrentEditor(clipboardSongObject.Clone(), editor, false);
            }
        }
    }
}
