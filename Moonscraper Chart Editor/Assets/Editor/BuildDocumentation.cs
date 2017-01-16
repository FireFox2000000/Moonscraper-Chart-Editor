using UnityEditor;
using System.Diagnostics;
using System.IO;

public class BuildDocumentation  {
    [MenuItem("MyTools/Windows Build With Postprocess")]
    public static void BuildGame()
    {
        buildSpecificPlayer(BuildTarget.StandaloneWindows);
    }

    [MenuItem("MyTools/Windows 64 Build With Postprocess")]
    public static void BuildGame64()
    {
        buildSpecificPlayer(BuildTarget.StandaloneWindows64);
    }

    static void buildSpecificPlayer(BuildTarget buildTarget)
    {
        // Get filename.
        string path = EditorUtility.SaveFolderPanel("Choose Location of Built Game", "", "");
        
        if (path != string.Empty)
        {      
            string[] levels = new string[] { "Assets/Scenes/test.unity" };

            // Build player.
            BuildPipeline.BuildPlayer(levels, path + "/Moonscraper Chart Editor.exe", buildTarget, BuildOptions.None);

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

            foreach (string file in Directory.GetFiles(path, "*.meta", SearchOption.AllDirectories))
            {
                File.Delete(file);
            }
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
