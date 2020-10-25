#define HACKY_PLUGIN_FIX

using UnityEditor;
using System.IO;
using System.Linq;

public class BuildDocumentation  {
    const string applicationName = "Moonscraper Chart Editor";

    [MenuItem("Build Processes/Windows x64 Build With Postprocess")]
    public static void BuildWindows64()
    {
        _BuildSpecificTarget(BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Build Processes/Windows x86 Build With Postprocess")]
    public static void BuildWindows32()
    {
        _BuildSpecificTarget(BuildTarget.StandaloneWindows);
    }

    [MenuItem("Build Processes/Linux Universal Build With Postprocess")]
    public static void BuildLinux()
    {
        _BuildSpecificTarget(BuildTarget.StandaloneLinuxUniversal);
    }

    [MenuItem("Build Processes/Build Full Releases")]
    public static void BuildAll()
    {
        string parentDirectory = GetSavePath();

        if (string.IsNullOrEmpty(parentDirectory)) {
            UnityEngine.Debug.Log("Build canceled");
            return;
        }

        string folderName = string.IsNullOrEmpty(Globals.applicationBranchName) ?
            string.Format("{0} v{1}", UnityEngine.Application.productName, UnityEngine.Application.version) :
            string.Format("{0} v{1} {2}", UnityEngine.Application.productName, UnityEngine.Application.version, Globals.applicationBranchName);

        string path = Path.Combine(parentDirectory, folderName);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
        Directory.CreateDirectory(path);

        BuildTarget[] targets = {
            BuildTarget.StandaloneWindows64,
            BuildTarget.StandaloneWindows,
            BuildTarget.StandaloneLinuxUniversal
        };

        foreach (var target in targets) {
            _BuildSpecificTarget(target, path);
        }
    }

    static string GetSavePath()
    {
        string commandLinePath = System.Environment.GetCommandLineArgs()
            .SkipWhile((arg) => arg != "-moonscraperBuildPath")
            .Skip(1)
            .FirstOrDefault();

        if (commandLinePath != null) {
            return commandLinePath;
        }

        string chosenPath = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");

        return chosenPath;
    }

    static void _BuildSpecificTarget(BuildTarget buildTarget) {
        string path = GetSavePath();

        if (string.IsNullOrEmpty(path)) {
            UnityEngine.Debug.Log("Build canceled");
            return;
        }

        _BuildSpecificTarget(buildTarget, path);
    }

    static void _BuildSpecificTarget(BuildTarget buildTarget, string parentDirectory)
    {
        string architecture;
        string executableName;
        switch (buildTarget) {
        case BuildTarget.StandaloneWindows:
            architecture = "Windows x86 (32 bit)";
            executableName = applicationName + ".exe";
            break;
        case BuildTarget.StandaloneWindows64:
            architecture = "Windows x86_64 (64 bit)";
            executableName = applicationName + ".exe";
            break;
        case BuildTarget.StandaloneLinuxUniversal:
            architecture = "Linux (Universal)";
            executableName = applicationName;
            break;
        default:
            architecture = buildTarget.ToString();
            executableName = applicationName;
            break;
        }

        string folderName = string.IsNullOrEmpty(Globals.applicationBranchName) ?
           string.Format("{0} v{1} {2}", UnityEngine.Application.productName, UnityEngine.Application.version, architecture)
           : string.Format("{0} v{1} {2} {3}", UnityEngine.Application.productName, UnityEngine.Application.version, Globals.applicationBranchName, architecture);

        string path = Path.Combine(parentDirectory, folderName);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
        Directory.CreateDirectory(path);
        UnityEngine.Debug.Log($"Building game at path: '{path}'");

        // Build player.
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.options = BuildOptions.None;
        options.target = buildTarget;
        options.scenes = EditorBuildSettings.scenes
            .Where((scene) => scene.enabled)
            .Select((scene) => scene.path)
            .ToArray();
        options.locationPathName = Path.Combine(path, executableName);
        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            return;

        if (Directory.Exists("Assets/Custom Resources"))
        {
            // Copy a file from the project folder to the build folder, alongside the built game.
            FileUtil.CopyFileOrDirectory("Assets/Custom Resources", Path.Combine(path, "Custom Resources"));
        }

        if (Directory.Exists("Assets/Documentation"))
        {
            // Copy a file from the project folder to the build folder, alongside the built game.
            FileUtil.CopyFileOrDirectory("Assets/Documentation", Path.Combine(path, "Documentation"));
        }

        string extraFilesDir = "Assets/ExtraBuildFiles";
        if (Directory.Exists(extraFilesDir))
        {
            // Copy a file from the project folder to the build folder, alongside the built game.
            foreach (string filepath in Directory.GetFiles(extraFilesDir))
            {
                FileUtil.CopyFileOrDirectory(filepath, Path.Combine(path, Path.GetFileName(filepath)));
            }

            foreach (string filepath in Directory.GetDirectories(extraFilesDir))
            {    
                string dirPath = filepath.Remove(0, extraFilesDir.Count() + 1);
                string destPath = Path.Combine(path, dirPath);

                UnityEngine.Debug.Log(filepath);
                UnityEngine.Debug.Log(dirPath);
                UnityEngine.Debug.Log(destPath);

                FileUtil.CopyFileOrDirectory(filepath, destPath);
            }
        }

        foreach (string file in Directory.GetFiles(path, "*.meta", SearchOption.AllDirectories))
        {
            File.Delete(file);
        }

#if HACKY_PLUGIN_FIX
        if (buildTarget == BuildTarget.StandaloneWindows || buildTarget == BuildTarget.StandaloneWindows64) {
            string[] copyToMonoFiles = {
                "bass_fx.dll"
            };

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
        }
#endif
    }
}
