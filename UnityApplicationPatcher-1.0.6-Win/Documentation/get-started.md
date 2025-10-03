# Unity Application Patcher get started

The Unity Application Patcher 1.0.6 is a tool that provides patched versions of Android, Windows, and macOS applications.

> **Note**: You can run the Unity Application Patcher on Windows or Mac, and can patch Android, Windows, or macOS applications. In the following sections, **patch an Android/Windows/macOS application** refers to the platform of the application you are patching (the target), not the operating system you are running the patcher on (the host).

Refer to the following pages to understand how to patch Android and Windows applications:

| **Topic** | **Description** |
| :-------- | :-------------- |
| [Patch Android applications](patch-android-applications.md) | Understand how to patch Android applications with the Unity Application Patcher. |
| [Patch Windows applications](patch-windows-applications.md) | Understand how to patch Windows applications with the Unity Application Patcher. |
| [Patch macOS applications](patch-macos-applications.md) | Understand how to patch macOS applications with the Unity Application Patcher. |

## How to use this documentation

You can view the documentation for the Unity Application Patcher in any tool that previews Markdown, such as Visual Studio Code, Typora, or similar.

To view the Unity Application Patcher documentation, open the `Documentation` folder in your Markdown viewing tool.

> **Tip**: To ensure you can preview images in the documentation, open the entire `Documentation` folder in your Markdown viewing tool. With some tools, images might not appear in documentation if you open specific files, rather than the entire folder.

### Documentation languages

Documentation has been translated into the following languages, which you can access from the corresponding folder:

* Japanese (`ja-jp`)
* Korean (`ko-kr`)
* Simplified Chinese (`zh-cn`)

## Known issues

- Running the Unity Application Patcher on macOS might fail if the path to `UnityApplicationPatcher.app` (not including `UnityApplicationPatcher.app`) is longer than 174 characters. This will cause the UI version of the tool to render a blank screen. The command line version of the tool will log a `System.IO.DirectoryNotFoundException` but should otherwise succeed at patching your app.
- Running the Unity Application Patcher Windows might fail if the path to `UnityApplicationPatcher.exe` or `UnityApplicationPatcherCLI.exe` (not including `UnityApplicationPatcher.exe` or `UnityApplicationPatcherCLI.exe`) is longer than 193 characters. This will cause failure when patching Android applications. If the path to the `UnityApplicationPatcher` executables is longer than 201 characters, this will cause a fatal error on both the UI version of the tool and the command line version, and prevent the tool from launching.
- Non-english characters might not render properly in the Command Prompt when using the Unity Application Patcher via the command line tool on Windows. Changing the Command Prompt font (to `SimSun-ExtB` for example) can address this issue. To change the font, right click on the top bar of the Command Prompt window, select **Properties**, click the **Font** tab, and select the new font. Click **OK** to apply the changes.
