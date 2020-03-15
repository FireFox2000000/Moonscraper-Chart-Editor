// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

//#undef UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
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
	public Text keysStream;
	public Text vocalStream;
    public Text drum1Stream;
	public Text drum2Stream;
	public Text drum3Stream;
	public Text drum4Stream;

    bool init = false;

    TimeSpan customTime = new TimeSpan();

    readonly ExtensionFilter audioExFilter = new ExtensionFilter("Audio files", "ogg", "mp3", "wav");

    private void Start()
    {
        offset.onValidateInput = LocalesManager.ValidateDecimalInput;
    }

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
                editor.uiServices.editorPanels.displayProperties.UpdateSongNameText();
                editor.RepaintWindowText();
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
		
		if (song.GetAudioIsLoaded(Song.AudioInstrument.Keys))
        {
			keysStream.color = Color.white;
            keysStream.text = song.GetAudioName(Song.AudioInstrument.Keys);
            ClipText(keysStream);
        }
        else
        {
            keysStream.color = Color.red;
			keysStream.text = "No audio";
        }

		 if (song.GetAudioIsLoaded(Song.AudioInstrument.Vocals))
        {
            vocalStream.color = Color.white;
            vocalStream.text = song.GetAudioName(Song.AudioInstrument.Vocals);
            ClipText(vocalStream);
        }
        else
        {
            vocalStream.color = Color.red;
            vocalStream.text = "No audio";
        }
		
        if (song.GetAudioIsLoaded(Song.AudioInstrument.Drum))
        {
            drum1Stream.color = Color.white;
            drum1Stream.text = song.GetAudioName(Song.AudioInstrument.Drum);
            ClipText(drum1Stream);
        }
        else
        {
            drum1Stream.color = Color.red;
            drum1Stream.text = "No audio";
        }
		
		if (song.GetAudioIsLoaded(Song.AudioInstrument.Drums_2))
        {
            drum2Stream.color = Color.white;
            drum2Stream.text = song.GetAudioName(Song.AudioInstrument.Drums_2);
            ClipText(drum2Stream);
        }
        else
        {
            drum2Stream.color = Color.red;
            drum2Stream.text = "No audio";
        }
if (song.GetAudioIsLoaded(Song.AudioInstrument.Drums_3))
        {
            drum3Stream.color = Color.white;
            drum3Stream.text = song.GetAudioName(Song.AudioInstrument.Drums_3);
            ClipText(drum3Stream);
        }
        else
        {
            drum3Stream.color = Color.red;
            drum3Stream.text = "No audio";
        }
		
		if (song.GetAudioIsLoaded(Song.AudioInstrument.Drums_4))
        {
            drum4Stream.color = Color.white;
            drum4Stream.text = song.GetAudioName(Song.AudioInstrument.Drums_4);
            ClipText(drum4Stream);
        }
        else
        {
            drum4Stream.color = Color.red;
            drum4Stream.text = "No audio";
        }
		
        ChartEditor.isDirty = true;
    }

    string GetAudioFile()
    {
        string audioFilepath = string.Empty;
        FileExplorer.OpenFilePanel(audioExFilter, "mp3,ogg,wav", out audioFilepath);
        return audioFilepath;
    }

    public void LoadMusicStream()
    {
        string path = GetAudioFile();
        if (!string.IsNullOrEmpty(path))
        {
            editor.currentSong.LoadMusicStream(path);

            StartCoroutine(SetAudio());
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

   public void LoadVocalStream()
    {
        try
        {
            editor.currentSong.LoadVocalStream(GetAudioFile());

            StartCoroutine(SetAudio());
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearVocalStream()
    {
        ClearAudioStream(Song.AudioInstrument.Vocals);
    }
	
	public void LoadKeysStream()
    {
        try
        {
            editor.currentSong.LoadKeysStream(GetAudioFile());

            StartCoroutine(SetAudio());
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearKeysStream()
    {
        ClearAudioStream(Song.AudioInstrument.Keys);
    }
	

    public void LoadDrum1Stream()
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

    public void ClearDrum1Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drums);
    }
	
	public void LoadDrum2Stream()
    {
        try
        {
            editor.currentSong.LoadDrum2Stream(GetAudioFile());

            StartCoroutine(SetAudio());
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearDrum2Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drums_2);
    }
	
	public void LoadDrum3Stream()
    {
        try
        {
            editor.currentSong.LoadDrum3Stream(GetAudioFile());

            StartCoroutine(SetAudio());
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearDrum3Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drums_3);
    }
	
	public void LoadDrum4Stream()
    {
        try
        {
            editor.currentSong.LoadDrum4Stream(GetAudioFile());

            StartCoroutine(SetAudio());
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearDrum4Stream()
    {
        ClearAudioStream(Song.AudioInstrument.Drums_4);
    }
	
	public void LoadCrowdStream()
    {
        try
        {
            editor.currentSong.LoadCrowdStream(GetAudioFile());

            StartCoroutine(SetAudio());
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void ClearCrowdStream()
    {
        ClearAudioStream(Song.AudioInstrument.Crowd);
    }

    void ClearAudioStream(Song.AudioInstrument audio)
    {
        editor.currentSong.GetSampleData(audio).Dispose();
        editor.currentSong.SetBassAudioStream(audio, null);

        setAudioTextLabels();
    }

    IEnumerator SetAudio()
    {
        LoadingTasksManager tasksManager = editor.services.loadingTasksManager;

        List<LoadingTask> tasks = new List<LoadingTask>()
        {
            new LoadingTask("Loading audio", () =>
            {
                while (editor.currentSong.isAudioLoading) ;
            })
        };

        tasksManager.KickTasks(tasks);

        while (tasksManager.isRunningTask)
            yield return null;

        setAudioTextLabels();
    }
}
