// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

//#undef UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class SongPropertiesPanelController : DisplayMenu {
    public Scrollbar verticalScroll;

    public InputField songName;
    public InputField artist;
    public InputField charter;
    public InputField album;
    public InputField year;
    public InputField offset;
    public InputField difficulty;
    public InputField genre;
    public InputField mediaType;

    public Text musicStream;
    public Text guitarStream;
    public Text bassStream;
    public Text rhythmStream;
    public Text drumStream;

    public LoadingScreenFader loadingScreen;

    bool init = false;

    TimeSpan customTime = new TimeSpan();

    protected override void OnEnable()
    {
        bool edit = ChartEditor.isDirty;

        base.OnEnable();
        
        init = true;
        Song song = editor.currentSong;
        Metadata metaData = song.metaData; 
           
        songName.text = song.name;
        artist.text = metaData.artist;
        charter.text = metaData.charter;
        album.text = metaData.album;
        year.text = metaData.year;
        offset.text = song.offset.ToString();
        difficulty.text = metaData.difficulty.ToString();
        genre.text = metaData.genre;
        mediaType.text = metaData.mediatype;

        // Init audio names
        setAudioTextLabels();
        init = false;

        customTime = TimeSpan.FromSeconds(editor.currentSong.length);

        ChartEditor.isDirty = edit;
        StartCoroutine(ScrollSetDelay());
    }

    IEnumerator ScrollSetDelay()
    {
        yield return null;
        verticalScroll.value = 1;
    }

	void Apply()
    {
        editor.currentSong.name = songName.text;
        editor.currentSong.metaData.artist = artist.text;
    }

    public void setSongProperties()
    {
        if (!init)
        {
            Song song = editor.currentSong;
            Metadata metaData = song.metaData;

            if (!song.name.Equals(songName.text))
            {
                song.name = songName.text;
                ChartEditor.Instance.uiServices.editorPanels.displayProperties.UpdateSongNameText();
            }

            metaData.artist = artist.text;
            metaData.charter = charter.text;
            metaData.album = album.text;
            metaData.year = year.text;

            try
            {
                song.offset = float.Parse(offset.text);
            }
            catch
            {
                song.offset = 0;
            }

            try
            {
                metaData.difficulty = int.Parse(difficulty.text);
            }
            catch
            {
                metaData.difficulty = 0;
            }

            metaData.genre = genre.text;
            metaData.mediatype = mediaType.text;

            if (editor.currentSong.manualLength)
            {
                editor.currentSong.length = (float)customTime.TotalSeconds;
            }

            ChartEditor.isDirty = true;
        }
    }

    void ClipText(Text text)
    {
        float maxWidth = text.rectTransform.rect.width;
        if (text.preferredWidth > maxWidth)
        {
            int removePos = text.text.Length - 1;
            text.text += "...";

            while (removePos > 0 && text.preferredWidth > maxWidth)
            {
                text.text = text.text.Remove(removePos--, 1);
            }
        }
    }

    void setAudioTextLabels()
    {
        Song song = editor.currentSong;
        if (song.GetAudioIsLoaded(Song.AudioInstrument.Song))
        {
            musicStream.color = Color.white;
            musicStream.text = song.GetAudioName(Song.AudioInstrument.Song);
            ClipText(musicStream);
        }
        else
        {
            musicStream.color = Color.red;
            musicStream.text = "No audio";
        }

        if (song.GetAudioIsLoaded(Song.AudioInstrument.Guitar))
        {
            guitarStream.color = Color.white;
            guitarStream.text = song.GetAudioName(Song.AudioInstrument.Guitar);
            ClipText(guitarStream);
        }
        else
        {
            guitarStream.color = Color.red;
            guitarStream.text = "No audio";
        }

        if (song.GetAudioIsLoaded(Song.AudioInstrument.Bass))
        {
            bassStream.color = Color.white;
            bassStream.text = song.GetAudioName(Song.AudioInstrument.Bass);
            ClipText(bassStream);
        }
        else
        {
            bassStream.color = Color.red;
            bassStream.text = "No audio";
        }

        if (song.GetAudioIsLoaded(Song.AudioInstrument.Rhythm))
        {
            rhythmStream.color = Color.white;
            rhythmStream.text = song.GetAudioName(Song.AudioInstrument.Rhythm);
            ClipText(rhythmStream);
        }
        else
        {
            rhythmStream.color = Color.red;
            rhythmStream.text = "No audio";
        }

        if (song.GetAudioIsLoaded(Song.AudioInstrument.Drum))
        {
            drumStream.color = Color.white;
            drumStream.text = song.GetAudioName(Song.AudioInstrument.Drum);
            ClipText(drumStream);
        }
        else
        {
            drumStream.color = Color.red;
            drumStream.text = "No audio";
        }

        ChartEditor.isDirty = true;
    }

    string GetAudioFile()
    {
        string audioFilepath = string.Empty;
        audioFilepath = FileExplorer.OpenFilePanel("Audio files (*.ogg,*.mp3,*.wav)\0*.mp3;*.ogg;*.wav", "mp3,ogg,wav");
        return audioFilepath;
    }

    public void LoadMusicStream()
    {
        try
        {
            editor.currentSong.LoadMusicStream(GetAudioFile());

            StartCoroutine(SetAudio());
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }        
    }

    public void ClearMusicStream()
    {
        ClearAudioStream(Song.AudioInstrument.Song);
    }

    public void LoadGuitarStream()
    {
        try
        {
            editor.currentSong.LoadGuitarStream(GetAudioFile());

            StartCoroutine(SetAudio());
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearGuitarStream()
    {
        ClearAudioStream(Song.AudioInstrument.Guitar);
    }

    public void LoadBassStream()
    {
        try
        {
            editor.currentSong.LoadBassStream(GetAudioFile());

            StartCoroutine(SetAudio());
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearBassStream()
    {
        ClearAudioStream(Song.AudioInstrument.Bass);
    }

    public void LoadRhythmStream()
    {
        try
        {
            editor.currentSong.LoadRhythmStream(GetAudioFile());

            StartCoroutine(SetAudio());
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearRhythmStream()
    {
        ClearAudioStream(Song.AudioInstrument.Rhythm);
    }

    public void LoadDrumStream()
    {
        try
        {
            editor.currentSong.LoadDrumStream(GetAudioFile());

            StartCoroutine(SetAudio());
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearDrumStream()
    {
        ClearAudioStream(Song.AudioInstrument.Drum);
    }

    void ClearAudioStream(Song.AudioInstrument audio)
    {
        editor.currentSong.GetSampleData(audio).Dispose();
        editor.currentSong.SetBassAudioStream(audio, null);

        setAudioTextLabels();
    }

    IEnumerator SetAudio()
    {
        Globals.applicationMode = Globals.ApplicationMode.Loading;

        loadingScreen.loadingInformation.text = "Loading audio";
        loadingScreen.FadeIn();

        while (editor.currentSong.isAudioLoading)
            yield return null;

        setAudioTextLabels();
        loadingScreen.FadeOut();
        Globals.applicationMode = Globals.ApplicationMode.Menu;
    }
}
