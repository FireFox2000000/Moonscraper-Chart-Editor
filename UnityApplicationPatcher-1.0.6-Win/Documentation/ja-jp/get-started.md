# Unity アプリケーションパッチャースタートガイド

Unity アプリケーションパッチャー __UNITY_VERSION_REPLACED_BY_BUILD__ は、Android/Windows/macOS アプリケーションのパッチ適用済みのバージョンを提供するツールです。

> **注意**: Unity アプリケーションパッチャーを Windows または Mac で実行して、Android/Windows/macOS アプリケーションにパッチを適用できます。以下の **Android/Windows/macOS アプリケーションにパッチを適用する** セクションはそれぞれ、パッチャーを実行しているオペレーティングシステム (ホスト) ではなく、パッチ適用対象のアプリケーションプラットフォーム (ターゲット) を指しています。

Android/Windows アプリケーションにパッチを適用する方法については、以下のページを参照してください。

| **トピック** | **説明** |
| :-------- | :-------------- |
| [Android アプリケーションにパッチを適用する](patch-android-applications.md)| Unity アプリケーションパッチャーを使用して Android アプリケーションにパッチを適用する方法を説明します。|
| [Windows アプリケーションにパッチを適用する](patch-windows-applications.md)| Unity アプリケーションパッチャーを使用して Windows アプリケーションにパッチを適用する方法を説明します。|
| [macOS アプリケーションにパッチを適用する](patch-macos-applications.md)| Unity アプリケーションパッチャーを使用して macOS アプリケーションにパッチを適用する方法を説明します。|

## このドキュメントを使用する方法

Unity アプリケーションパッチャーのドキュメントは、Visual Studio Code、Typora などのマークダウンをプレビューできるツールで表示できます。

Unity アプリケーションパッチャーのドキュメントを表示するには、マークダウン表示ツールの `Documentation` フォルダーを開きます。

> **ヒント**: ドキュメント内の画像のプレビューを表示するには、マークダウン表示ツールの `Documentation` フォルダー全体を開いてください。一部のツールでは、フォルダー全体ではなく特定のファイルを開いた場合にドキュメント内の画像が表示されないことがあります。

### ドキュメントの言語

ドキュメントは以下の言語に翻訳されており、対応するフォルダーからアクセスできます。 

* 日本語 (`ja-jp`)
* 韓国語 (`ko-kr`)
* 簡体字中国語 (`zh-cn`)

## 既知の問題

- macOS での Unity アプリケーションパッチャーの実行は、`UnityApplicationPatcher.app` のパス (`UnityApplicationPatcher.app` を含まない) が 174 文字より長いと失敗する場合があります。これによって、ツールの UI バージョンで空白の画面が表示されます。ツールのコマンドラインバージョンでは、`System.IO.DirectoryNotFoundException` がログに記録されますが、アプリケーションへのパッチ適用は成功します。
- Windows での Unity アプリケーションパッチャーの実行は、`UnityApplicationPatcher.exe` または `UnityApplicationPatcherCLI.exe` のパス (`UnityApplicationPatcher.exe` または `UnityApplicationPatcherCLI.exe を含まない) が 193 文字より長いと失敗する場合があります。これは、Android 向けアプリケーションにパッチを適用するときに失敗する原因となります。`UnityApplicationPatcher` 実行ファイルのパスが 201 文字より長いと、ツールの UI バージョンとコマンドラインバージョンの両方で致命的なエラーが発生し、ツールが起動できなくなります。
- Windows のコマンドラインツールで Unity アプリケーションパッチャーを使用すると、コマンドプロンプトに英語以外の文字が正しく表示されない場合があります。コマンドプロンプトのフォントを (例えば `SimSun-ExtB` に) 変更することで、この問題に対処できます。フォントを変更するには、コマンドプロンプトウィンドウの上部バーを右クリックして **Properties** を選択し、**Font** タブをクリックして新しいフォントを選択します。**OK** をクリックして変更を適用します。
