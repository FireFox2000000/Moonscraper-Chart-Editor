using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    protected override void OnEnable()
    {
        base.OnEnable();

        Song song = editor.currentSong;
        songName.text = song.name;
        artist.text = song.artist;
        charter.text = song.charter;
        difficulty.text = song.difficulty.ToString();
        genre.text = song.genre;
        mediaType.text = song.mediatype;

        // Init audio names
        setAudioTextLabels();
    }

	void Apply()
    {
        editor.currentSong.name = songName.text;
        editor.currentSong.artist = artist.text;
    }

    public void setSongProperties()
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
            editor.Stop();

            editor.currentSong.LoadMusicStream(GetAudioFile());

            if (editor.currentSong.musicStream != null)
            {
                editor.musicSource.clip = editor.currentSong.musicStream;
            }

            setAudioTextLabels();
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }

    public void LoadGuitarStream()
    {
        try
        {
            editor.Stop();

            editor.currentSong.LoadGuitarStream(GetAudioFile());

            if (editor.currentSong.guitarStream != null)
            {
                //musicSource.clip = currentSong.guitarStream;
            }

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
            editor.Stop();

            editor.currentSong.LoadRhythmStream(GetAudioFile());

            if (editor.currentSong.rhythmStream != null)
            {
                //musicSource.clip = currentSong.rhythmStream;
            }

            setAudioTextLabels();
        }
        catch
        {
            Debug.LogError("Could not open audio");
        }
    }
}
