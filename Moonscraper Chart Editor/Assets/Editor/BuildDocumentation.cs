#define HACKY_PLUGIN_FIX

using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

public class BuildDocumentation  {
    const string applicationName = "Moonscraper Chart Editor";
    static readonly string[] copyToMonoFiles = new string[]
    {
        "bass_fx.dll"
    };

    [MenuItem("Build Processes/Windows Build With Postprocess")]
    public static void BuildGame()
    {
        string path = GetSavePath();
        _BuildGame(path);
    }

    [MenuItem("Build Processes/Windows 64 Build With Postprocess")]
    public static void BuildGame64()
    {
        string path = GetSavePath();
        _BuildGame64(path);
    }

    [MenuItem("Build Processes/Build Full Windows Releases")]
    public static void BuildGameWindowsAll()
    {
        string path = GetSavePath();
        string folderName = string.Format("{0} v{1}", UnityEngine.Application.productName, UnityEngine.Application.version);
        string folderPath = path + "/" + folderName;

        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath);
        }
        Directory.CreateDirectory(folderPath);
        path = folderPath;

        _BuildGame(path);
        _BuildGame64(path);
    }

    static void _BuildGame(string path)
    {
        string folderName = string.IsNullOrEmpty(Globals.applicationBranchName) ?
           string.Format("{0} v{1} x86 (32 bit)", UnityEngine.Application.productName, UnityEngine.Application.version)
           : string.Format("{0} v{1} {2} x86 (32 bit)", UnityEngine.Application.productName, UnityEngine.Application.version, Globals.applicationBranchName);
        buildSpecificPlayer(BuildTarget.StandaloneWindows, path, folderName);
    }

    static void _BuildGame64(string path)
    {
        string folderName = string.IsNullOrEmpty(Globals.applicationBranchName) ?
            string.Format("{0} v{1} x86_64 (64 bit)", UnityEngine.Application.productName, UnityEngine.Application.version)
            : string.Format("{0} v{1} {2} x86_64 (64 bit)", UnityEngine.Application.productName, UnityEngine.Application.version, Globals.applicationBranchName);
        buildSpecificPlayer(BuildTarget.StandaloneWindows64, path, folderName);
    }

    static string GetSavePath()
    {
        string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
        return path;
    }

    static void buildSpecificPlayer(BuildTarget buildTarget, string path, string folderName)
    {
        if (path != string.Empty)
        {
            string folderPath = path + "/" + folderName;
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath);
            }
            Directory.CreateDirectory(folderPath);
            path = folderPath;
            UnityEngine.Debug.Log("Building game at path: " + path);

            List<string> levels = new List<string>(EditorBuildSettings.scenes.Length);

            for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
            {
                EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
                if (scene.enabled)
                {
                    levels.Add(scene.path);
                    UnityEngine.Debug.Log(scene);
                }
            }

            // Build player.
            var report = BuildPipeline.BuildPlayer(levels.ToArray(), path + "/" + applicationName + ".exe", buildTarget, BuildOptions.None);

            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                return;

            if (Directory.Exists("Assets/Custom Resources"))
            {
                // Copy a file from the project folder to the build folder, alongside the built game.
                clearAndDeleteDirectory(path + "/Custom Resources");
                FileUtil.CopyFileOrDirectory("Assets/Custom Resources", path + "/Custom Resources");
            }

            if (Directory.Exists("Assets/Documentation"))
            {
                // Copy a file from the project folder to the build folder, alongside the built game.
                clearAndDeleteDirectory(path + "/Documentation");
                FileUtil.CopyFileOrDirectory("Assets/Documentation", path + "/Documentation");
            }

            if (Directory.Exists("Assets\\ExtraBuildFiles"))
            {
                // Copy a file from the project folder to the build folder, alongside the built game.
                foreach (string filepath in Directory.GetFiles("Assets\\ExtraBuildFiles"))
                {
                    FileUtil.CopyFileOrDirectory(filepath, path + "/" + Path.GetFileName(filepath));
                }

            }

            foreach (string file in Directory.GetFiles(path, "*.meta", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }

#if HACKY_PLUGIN_FIX
            string dataPath = path + "/" + applicationName + "_Data/";
            string destFolder = dataPath + "Mono/";

            if (copyToMonoFiles.Length > 0)
                Directory.CreateDirectory(destFolder);  // Make sure this exists

            foreach (string file in copyToMonoFiles)
            {
                string pluginPath = dataPath + "Plugins/" + file;
                if (File.Exists(pluginPath))
                {
                    string dest = destFolder + file;                
                    File.Copy(pluginPath, dest);
                }
            }
#endif
        }
        else
        {
            UnityEngine.Debug.Log("Build canceled");
        }
    }

    static void clearAndDeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            Directory.Delete(path);
        }
    }
}
