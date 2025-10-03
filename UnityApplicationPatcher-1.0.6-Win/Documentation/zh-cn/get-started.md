# Unity 应用程序修补程序入门指南

Unity 应用程序修补程序 __UNITY_VERSION_REPLACED_BY_BUILD__ 是一款提供 Android、Windows 和 macOS 应用程序修补版本的工具。

> **注意**：您可以在 Windows 或 Mac 上运行 Unity 应用程序修补程序，并且能够修补 Android、Windows 或 macOS 应用程序。在以下几节中，**修补 Android/Windows/macOS 应用程序**是指您正在修补的应用程序所属的平台（目标），而不是您运行该修补程序的操作系统（主机）。

请参阅以下页面了解如何修补 Android 和 Windows 应用程序：

| **主题** | **描述** |
| :-------- | :-------------- |
| [修补 Android 应用程序](patch-android-applications.md)| 了解如何使用 Unity 应用程序修补程序来修补 Android 应用程序。|
| [修补 Windows 应用程序](patch-windows-applications.md)| 了解如何使用 Unity 应用程序修补程序来修补 Windows 应用程序。|
| [修补 macOS 应用程序](patch-macos-applications.md)| 了解如何使用 Unity 应用程序修补程序来修补 macOS 应用程序。|

## 本文档使用说明

您可以使用任何能够预览 Markdown 文件的工具（例如 Visual Studio Code、Typora 或类似工具）中查看 Unity 应用程序修补程序的文档。

要查看 Unity 应用程序修补程序的文档，请在 Markdown 查看工具中打开 `Documentation` 文件夹。

> **提示**：为确保能够预览文档中的图像，请在 Markdown 查看工具中打开整个 `Documentation` 文件夹。使用某些工具时，如果只打开特定文件而不打开整个文件夹，文档中可能不会显示图像。

### 文档语言

文档已翻译成以下语言，您可以从相应的文件夹访问这些语言的版本：

* 日语 (`ja-jp`)
* 韩语 (`ko-kr`)
* 简体中文 (`zh-cn`)

## 已知问题

- 如果 `UnityApplicationPatcher.app` 的路径（不包括 `UnityApplicationPatcher.app`）长度超过 174 个字符，则在 macOS 上运行 Unity 应用程序修补程序可能会失败。这将导致该工具的 UI 版本呈现空白屏幕。该工具的命令行版本将记录 `System.IO.DirectoryNotFoundException`，但应该能够成功修补您的应用。
- 如果 `UnityApplicationPatcher.exe` 或 `UnityApplicationPatcherCLI.exe` 的路径（不包括 `UnityApplicationPatcher.exe` 或 `UnityApplicationPatcherCLI.exe`）长度超过 193 个字符，则在 Windows 上运行 Unity 应用程序修补程序可能会失败。这将导致修补 Android 应用程序失败。如果 `UnityApplicationPatcher` 可执行文件的路径长度超过 201 个字符，这将导致该工具的 UI 版本和命令行版本都出现致命错误，并会阻止该工具启动。
- 在 Windows 上通过命令行工具使用 Unity 应用程序修补程序时，非英文字符可能无法在命令提示符窗口中正确显示。更改命令提示符字体（例如更改为 `SimSun-ExtB`）可以解决此问题。要更改字体，请右键点击命令提示符窗口的顶栏，选择**属性**，点击**字体**选项卡，然后选择新字体。点击**确定**应用所做的更改。
