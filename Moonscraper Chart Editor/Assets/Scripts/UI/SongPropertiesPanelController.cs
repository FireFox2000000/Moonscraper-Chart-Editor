//#undef UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;

public class SongPropertiesPanelController : DisplayMenu {

    public InputField songName;
    public InputField artist;
    public InputField charter;
    public InputField difficulty;
    public InputField genre;
    public InputField mediaType;

    public Text musicStream;
    public Text guitarStream;
    public Text rhythmStream;

    bool init = false;
    protected override void OnEnable()
    {
        base.OnEnable();
        init = true;
        Song song = editor.currentSong;   
           
        songName.text = song.name;
        artist.text = song.artist;
        charter.text = song.charter;
        difficulty.text = song.difficulty.ToString();
        genre.text = song.genre;
        mediaType.text = song.mediatype;

        // Init audio names
        setAudioTextLabels();
        init = false;
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

            ChartEditor.editOccurred = true;
        }
    }

    void setAudioTextLabels()
    {
        Song song = editor.currentSong;
        if (song.musicStream)
        {
            musicStream.color = Color.white;
            musicStream.text = song.musicStream.name;
        }
        else
        {
            musicStream.color = Color.red;
            musicStream.text = "No audio";
        }

        if (song.guitarStream)
        {
            guitarStream.color = Color.white;
            guitarStream.text = song.guitarStream.name;
        }
        else
        {
            guitarStream.color = Color.red;
            guitarStream.text = "No audio";
        }

        if (song.rhythmStream)
        {
            rhythmStream.color = Color.white;
            rhythmStream.text = song.rhythmStream.name;
        }
        else
        {
            rhythmStream.color = Color.red;
            rhythmStream.text = "No audio";
        }

        ChartEditor.editOccurred = true;
    }

    string GetAudioFile()
    {
        string audioFilepath = string.Empty;

#if UNITY_EDITOR
        audioFilepath = UnityEditor.EditorUtility.OpenFilePanel("Select Audio", "", "*.mp3;*.ogg;*.wav");
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
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }

        editor.SetAudioSources();

        setAudioTextLabels();       
    }

    public void LoadGuitarStream()
    {
        try
        {
            editor.currentSong.LoadGuitarStream(GetAudioFile());

            editor.SetAudioSources();

            setAudioTextLabels();
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void LoadRhythmStream()
    {
        try
        {
            editor.currentSong.LoadRhythmStream(GetAudioFile());

            editor.SetAudioSources();

            setAudioTextLabels();
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }
}
