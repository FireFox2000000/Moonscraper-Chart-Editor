// Copyright (c) 2016-2017 Alexander Ong
// See LICENSE in project root for license information.

//#undef UNITY_EDITOR
#define BASS_AUDIO

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
           
        songName.text = song.name;
        artist.text = song.artist;
        charter.text = song.charter;
        album.text = song.album;
        year.text = song.year;
        offset.text = song.offset.ToString();
        difficulty.text = song.difficulty.ToString();
        genre.text = song.genre;
        mediaType.text = song.mediatype;

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
        editor.currentSong.artist = artist.text;
    }

    public void setSongProperties()
    {
        if (!init)
        {
            Song song = editor.currentSong;
            song.name = songName.text;
            song.artist = artist.text;
            song.charter = charter.text;
            song.album = album.text;
            song.year = year.text;

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
                song.difficulty = int.Parse(difficulty.text);
            }
            catch
            {
                song.difficulty = 0;
            }

            song.genre = genre.text;
            song.mediatype = mediaType.text;

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
        if (song.songAudioLoaded)
        {
            musicStream.color = Color.white;
            musicStream.text = song.musicSongName;
            ClipText(musicStream);
        }
        else
        {
            musicStream.color = Color.red;
            musicStream.text = "No audio";
        }

        if (song.guitarAudioLoaded)
        {
            guitarStream.color = Color.white;
            guitarStream.text = song.guitarSongName;
            ClipText(guitarStream);
        }
        else
        {
            guitarStream.color = Color.red;
            guitarStream.text = "No audio";
        }

        if (song.bassAudioLoaded)
        {
            bassStream.color = Color.white;
            bassStream.text = song.bassSongName;
            ClipText(bassStream);
        }
        else
        {
            bassStream.color = Color.red;
            bassStream.text = "No audio";
        }

        if (song.rhythmAudioLoaded)
        {
            rhythmStream.color = Color.white;
            rhythmStream.text = song.rhythmSongName;
            ClipText(rhythmStream);
        }
        else
        {
            rhythmStream.color = Color.red;
            rhythmStream.text = "No audio";
        }

        if (song.drumAudioLoaded)
        {
            drumStream.color = Color.white;
            drumStream.text = song.drumSongName;
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

#if UNITY_EDITOR
        audioFilepath = UnityEditor.EditorUtility.OpenFilePanel("Select Audio", "", "mp3,ogg,wav");
#else
            OpenFileName openAudioDialog = new OpenFileName();
            openAudioDialog = new OpenFileName();

            openAudioDialog.structSize = Marshal.SizeOf(openAudioDialog);

            openAudioDialog.file = new String(new char[256]);
            openAudioDialog.maxFile = openAudioDialog.file.Length;

            openAudioDialog.fileTitle = new String(new char[64]);
            openAudioDialog.maxFileTitle = openAudioDialog.fileTitle.Length;

            openAudioDialog.initialDir = "";
            openAudioDialog.title = "Open file";
            openAudioDialog.defExt = "txt";

            openAudioDialog.filter = "Audio files (*.ogg,*.mp3,*.wav)\0*.mp3;*.ogg;*.wav";

            if (LibWrap.GetOpenFileName(openAudioDialog))
            {
                audioFilepath = openAudioDialog.file;
            
            }
            else
                throw new System.Exception("Could not open file");
#endif

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
        clearAudioStream(Song.MUSIC_STREAM_ARRAY_POS);
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
        clearAudioStream(Song.GUITAR_STREAM_ARRAY_POS);
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
        clearAudioStream(Song.BASS_STREAM_ARRAY_POS);
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
        clearAudioStream(Song.RHYTHM_STREAM_ARRAY_POS);
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
        clearAudioStream(Song.DRUM_STREAM_ARRAY_POS);
    }

    void clearAudioStream(int songAudioIndex)
    {
        switch (songAudioIndex)
        {
            case (Song.MUSIC_STREAM_ARRAY_POS):
                editor.currentSong.musicSample.Free();
                editor.currentSong.bassMusicStream = 0;
                break;

            case (Song.GUITAR_STREAM_ARRAY_POS):
                editor.currentSong.guitarSample.Free();
                editor.currentSong.bassGuitarStream = 0;
                break;

            case (Song.BASS_STREAM_ARRAY_POS):
                editor.currentSong.bassSample.Free();
                editor.currentSong.bassBassStream = 0;
                break;

            case (Song.RHYTHM_STREAM_ARRAY_POS):
                editor.currentSong.rhythmSample.Free();
                editor.currentSong.bassRhythmStream = 0;
                break;

            case (Song.DRUM_STREAM_ARRAY_POS):
                editor.currentSong.drumSample.Free();
                editor.currentSong.bassDrumStream = 0;
                break;

            default:
                break;
        }
        
        editor.SetAudioSources();
        setAudioTextLabels();
    }

    IEnumerator SetAudio()
    {
        Globals.applicationMode = Globals.ApplicationMode.Loading;

        loadingScreen.loadingInformation.text = "Loading audio";
        loadingScreen.FadeIn();

        while (editor.currentSong.IsAudioLoading)
            yield return null;

        editor.SetAudioSources();
        setAudioTextLabels();
        loadingScreen.FadeOut();
        Globals.applicationMode = Globals.ApplicationMode.Menu;
    }
}
