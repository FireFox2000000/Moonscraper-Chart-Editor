#define HACKY_PLUGIN_FIX

using UnityEditor;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Text;

public class BuildManager  {
    const string applicationName = "Moonscraper Chart Editor";

    // 7-Zip.exe location
    static readonly string CompressionProgramPath = System.Environment.GetEnvironmentVariable("7-Zip");

    // Security Patch application location
    static readonly string UnityApplicationPatcherProgramPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../UnityApplicationPatcher-1.0.6-Win/UnityApplicationPatcherCLI.exe"));
    
    // Inno Setup 6 ISCC.exe location
    static readonly string InstallerProgramPath = System.Environment.GetEnvironmentVariable("ISCC");

    [System.Flags]
    public enum BuildFlags
    {
        None = 0,
        CreateDistributable = 1 << 0,
        SpecifyVersionNumber = 1 << 1,
        BuildInstaller = 1 << 2,
    }

    [MenuItem("Build Processes/Windows x64 Distributable")]
    public static void BuildWindows64()
    {
        BuildSpecificTargetDistributable(BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Build Processes/Windows x86 Distributable")]
    public static void BuildWindows32()
    {
        BuildSpecificTargetDistributable(BuildTarget.StandaloneWindows);
    }

    [MenuItem("Build Processes/Linux Universal Distributable")]
    public static void BuildLinux()
    {
        BuildSpecificTargetDistributable(BuildTarget.StandaloneLinuxUniversal);
    }

    [MenuItem("Build Processes/Build Full Distributables")]
    public static void BuildAll_Distributable()
    {
        BuildAll(BuildFlags.CreateDistributable | BuildFlags.SpecifyVersionNumber);
    }

    [MenuItem("Build Processes/Windows x64 Installer")]
    public static void BuildWindows64Installer()
    {
        // Parent directory must match path defined in installer script
        _BuildSpecificTarget(BuildTarget.StandaloneWindows64, Path.GetFullPath(Path.Combine(Application.dataPath, "../../Installer/Builds/")), BuildFlags.BuildInstaller);
    }

    [MenuItem("Build Processes/Windows x86 Installer")]
    public static void BuildWindows32Installer()
    {
        // Parent directory must match path defined in installer script
        _BuildSpecificTarget(BuildTarget.StandaloneWindows, Path.GetFullPath(Path.Combine(Application.dataPath, "../../Installer/Builds/")), BuildFlags.BuildInstaller);
    }

    [MenuItem("Build Processes/Build Final")]
    public static void BuildAll_Standard()
    {
        BuildWindows64Installer();
        BuildWindows32Installer();
        //BuildLinux();
    }

    public static void BuildAll(BuildFlags buildFlags)
    {
        string parentDirectory = GetSavePath();

        if (string.IsNullOrEmpty(parentDirectory)) {
            Debug.Log("Build canceled");
            return;
        }

        string folderName = Application.productName;
        if ((buildFlags & BuildFlags.SpecifyVersionNumber) != 0)
        {
            folderName += string.Format(" v{0}", Application.productName);
        }

        if (!string.IsNullOrEmpty(Globals.applicationBranchName))
        {
            folderName += string.Format(" {0}", Globals.applicationBranchName);
        }

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
            _BuildSpecificTarget(target, path, buildFlags);
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

    enum UnityApplicationPatcher_ExitCode
    {
        Success = 0,
        PatchFailed = 1,
        PatchNotFound = 2,
        ExceptionCaught = 3,
        InvalidCommandLineArg = 64,
        PatchAlreadyApplied = 183,
    }

    static void BuildSpecificTargetDistributable(BuildTarget buildTarget) {
        string path = GetSavePath();

        if (string.IsNullOrEmpty(path)) {
            Debug.Log("Build canceled");
            return;
        }

        _BuildSpecificTarget(buildTarget, path, BuildFlags.CreateDistributable | BuildFlags.SpecifyVersionNumber);
    }

    static void _BuildSpecificTarget(BuildTarget buildTarget, string parentDirectory, BuildFlags buildFlags)
    {
        if (!Directory.Exists(parentDirectory))
        {
            Debug.Log(string.Format("Output target directory does not exist. Creating directory \"{0}\"", parentDirectory));
            Directory.CreateDirectory(parentDirectory);
        }

        string architecture;
        string executableName;
        string compressionExtension = string.Empty;
        string installerCompileScriptPath = string.Empty;
        string installerPlatform = string.Empty;

        switch (buildTarget) {
        case BuildTarget.StandaloneWindows:
            architecture = "Windows x86 (32 bit)";
            executableName = applicationName + ".exe";
            compressionExtension = ".zip";
            installerCompileScriptPath = "MSCE Windows.iss";
            installerPlatform = "x86";
            break;
        case BuildTarget.StandaloneWindows64:
            architecture = "Windows x86_64 (64 bit)";
            executableName = applicationName + ".exe";
            compressionExtension = ".zip";
            installerCompileScriptPath = "MSCE Windows.iss";
            installerPlatform = "x64";
            break;
        case BuildTarget.StandaloneLinuxUniversal:
            architecture = "Linux (Universal)";
            executableName = applicationName;
            compressionExtension = ".tar.gz";

            Debug.Assert((buildFlags & BuildFlags.BuildInstaller) == 0, "Installer not supported for Linux builds");
            break;
        default:
            architecture = buildTarget.ToString();
            executableName = applicationName;
            break;
        }

        string folderName = Application.productName;

        if ((buildFlags & BuildFlags.SpecifyVersionNumber) != 0)
        {
            folderName += string.Format(" v{0}", Application.version);
        }

        if (!string.IsNullOrEmpty(Globals.applicationBranchName))
        {
            folderName += string.Format(" {0}", Globals.applicationBranchName);
        }

        folderName += string.Format(" {0}", architecture);

        string path = Path.Combine(parentDirectory, folderName);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
        Directory.CreateDirectory(path);
        Debug.Log($"Building game at path: '{path}'");

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

        switch (buildTarget)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                {
                    Debug.Log("Applying security patch");

                    UnityApplicationPatcher_ExitCode resultCode = RunUnityApplicationPatcher(path, string.Format("-windows -applicationPath \"{0}\"", path));
                    switch (resultCode)
                    {
                        case UnityApplicationPatcher_ExitCode.Success:
                        case UnityApplicationPatcher_ExitCode.PatchAlreadyApplied:
                            {
                                break;
                            }
                        default:
                            {
                                Debug.LogErrorFormat("Application patch failed due to code {0}. Aborting build", resultCode);
                                return;
                            }
                    }
                    break;
                }

            default:
                {
                    break;
                }
        }  

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

                Debug.Log(filepath);
                Debug.Log(dirPath);
                Debug.Log(destPath);

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

        if ((buildFlags & BuildFlags.BuildInstaller) != 0)
        {
            string ScriptFolderPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Installer/Scripts/"));
            string installerCompilePath = Path.Combine(ScriptFolderPath, installerCompileScriptPath);

            bool installerValid = !string.IsNullOrEmpty(InstallerProgramPath) && File.Exists(InstallerProgramPath);
            bool scriptValid = !string.IsNullOrEmpty(installerCompileScriptPath) && File.Exists(installerCompilePath);

            Debug.Assert(installerValid, "Path to Inno Installer not set in environment variables, cannot proceed with build.");
            Debug.Assert(scriptValid);

            if (installerValid && scriptValid)
            {
                Debug.Assert(!string.IsNullOrEmpty(installerPlatform));     // Unhandled platform 

                using (var process = new System.Diagnostics.Process())
                {
                    List<string> args = new List<string>();
                    args.Add(string.Format("/dMyAppVersion={0}", Application.version));
                    args.Add(string.Format("/dPlatform={0}", installerPlatform));
                    args.Add(string.Format("\"{0}\"", installerCompileScriptPath));

                    StringBuilder argsSb = new StringBuilder();
                    foreach (string arg in args)
                    {
                        argsSb.AppendFormat("{0} ", arg);
                    }

                    Debug.LogFormat("Installer command args: {0}", argsSb.ToString());

                    process.StartInfo.FileName = InstallerProgramPath;
                    process.StartInfo.WorkingDirectory = ScriptFolderPath;
                    process.StartInfo.Arguments = argsSb.ToString().Trim();
                    process.Start();

                    process.WaitForExit();
                }
            }
        }

        if ((buildFlags & BuildFlags.CreateDistributable) != 0)
        {
            // Compress to shareable file
            if (CompressionProgramPath == null || !File.Exists(CompressionProgramPath))
            {
                Debug.Assert(false, "Path to 7-Zip not set in environment variables, cannot proceed with build compression.");
                return;
            }

            if (!string.IsNullOrEmpty(compressionExtension) && File.Exists(CompressionProgramPath))
            {
                Debug.Log("Performing compression step.");

                using (var process = new System.Diagnostics.Process())
                {
                    switch (compressionExtension)
                    {
                        case ".zip":
                            {
                                string compressedFile = string.Format("{0}.zip", folderName);
                                if (File.Exists(compressedFile))
                                {
                                    File.Delete(compressedFile);
                                }

                                process.StartInfo.FileName = CompressionProgramPath;
                                process.StartInfo.WorkingDirectory = parentDirectory;
                                process.StartInfo.Arguments = string.Format("a \"{0}\" \"{1}\"", compressedFile, path);
                                process.Start();

                                process.WaitForExit();

                                break;
                            }

                        case ".tar.gz":
                            {
                                {
                                    string compressedFile = string.Format("{0}.tar", folderName);
                                    if (File.Exists(compressedFile))
                                    {
                                        File.Delete(compressedFile);
                                    }

                                    compressedFile = string.Format("{0}.tar.gz", folderName);
                                    if (File.Exists(compressedFile))
                                    {
                                        File.Delete(compressedFile);
                                    }
                                }

                                process.StartInfo.FileName = CompressionProgramPath;
                                process.StartInfo.WorkingDirectory = parentDirectory;
                                process.StartInfo.Arguments = string.Format("a \"{0}.tar\" \"{1}\"", folderName, path);
                                process.Start();

                                process.WaitForExit();

                                process.StartInfo.FileName = CompressionProgramPath;
                                process.StartInfo.WorkingDirectory = parentDirectory;
                                process.StartInfo.Arguments = string.Format("a \"{0}.tar.gz\" \"{1}.tar\"", folderName, folderName);
                                process.Start();

                                process.WaitForExit();

                                break;
                            }

                        default:
                            break;
                    }
                }
            }
        }

        Debug.Log("Build target complete!");
    }

    /// <returns>Exit code</returns>
    static UnityApplicationPatcher_ExitCode RunUnityApplicationPatcher(string workingDirectoryPath, string args)
    {
        using (var process = new System.Diagnostics.Process())
        {
            process.StartInfo.FileName = UnityApplicationPatcherProgramPath;
            process.StartInfo.WorkingDirectory = workingDirectoryPath;
            process.StartInfo.Arguments = args;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.OutputDataReceived += (sender, e) => {
                if (e.Data != null)
                    Debug.Log(e.Data);
            };
            process.ErrorDataReceived += (sender, e) =>{
                if (e.Data != null)
                    Debug.LogError(e.Data);
            };

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            return (UnityApplicationPatcher_ExitCode)process.ExitCode;
        }
    }
}
