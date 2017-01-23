using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LoadCustomResources : MonoBehaviour {
    List<WWW> filesLoading = new List<WWW>();
    string initialDirectory = "/Custom Resources/";
    List<string> filepaths = new List<string>();

    List<string> setFileNames = new List<string>(new string[] { "miss-1"});

    public AudioClip miss1 = null;

    // define audioClips
    // define textures

    // Use this for initialization
    void Start () {
        // Collect all the files
        filepaths = GetAllFiles(initialDirectory);

        // Sort that the files collected relate to an actual variable
        for (int i = 0; i < filepaths.Count; ++i)
        {
            bool loaded = false;
            string name = Path.GetFileNameWithoutExtension(filepaths[i]);

            if (setFileNames.Contains(name))
            {
                // Validate file extension and begin loading
                switch (name)
                {
                    case ("miss-1"):
                        if (Utility.validateExtension(filepaths[i], Globals.validAudioExtensions))
                        {
                            filesLoading.Add(new WWW("file://" + filepaths[i]));
                            loaded = true;
                        }
                        break;
                    default:
                        break;
                }

                // Remove so that duplicate files with different file extentions don't overwrite each other
                if (loaded)
                    setFileNames.Remove(name);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        float progress = 0;
        bool complete = true;

	    foreach (WWW fileLoad in filesLoading)
        {
            if (fileLoad != null)
            {
                progress += fileLoad.progress;
                if (!fileLoad.isDone)
                    complete = false;
            }
        }

        if (filesLoading.Count > 0)
            progress /= filesLoading.Count;
        else
            progress = 0;

        // if all files are loaded, place into the correct variables and load the next scene
        if (complete)
        {

        }
	}

    List<string> GetAllFiles(string dir)
    {
        List<string> files = new List<string>();
        try
        {
            foreach (string f in Directory.GetFiles(dir))
            {
                files.Add(f);
            }
            foreach (string d in Directory.GetDirectories(dir))
            {
                files.AddRange(GetAllFiles(d));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }

        return files;
    }

}
