# macOS 애플리케이션 패치

다음 섹션에 나와 있는 대로 Unity 애플리케이션 패치 프로그램을 사용하여 macOS 애플리케이션을 패치할 수 있습니다.

| **주제** | **설명** |
| :-------- | :-------------- |
| [macOS 애플리케이션 패치](#patch-a-macos-application-using-windows-or-mac) | Windows나 Mac에서 Unity 애플리케이션 패치 프로그램을 사용합니다. |
| [커맨드 라인 툴](#command-line-tool-macos-application-patcher) | 커맨드 라인 툴을 사용하여 macOS 애플리케이션을 패치합니다. |
| [애플리케이션 반환 코드](#application-return-codes) | 자동화 및 문제 해결을 위한 애플리케이션의 종료 코드를 파악합니다. |
| [코드 서명 및 인증](#code-signing-and-notarization) | macOS 애플리케이션을 패치한 후 애플리케이션의 코드 서명 및 인증에 도움이 되는 정보입니다. |

<a id="patch-a-macos-application-using-windows-or-mac"></a>
## macOS 애플리케이션 패치(Windows나 Mac 사용)

1. Windows나 Mac에서 애플리케이션을 실행합니다.
2. 사이드바 메뉴에서 **macOS** 버튼을 선택합니다.
3. **애플리케이션 경로** 필드로 이동하여 **찾아보기** 버튼을 선택합니다.
4. 파일 탐색기를 사용하여 애플리케이션 번들을 찾습니다. 예를 들면 다음과 같습니다. `Unity.app` 서버 애플리케이션 폴더 내에서 전용 서버 애플리케이션 위치 `UnityPlayer.dylib`를 패치하고 있는 경우.
5. 파일 탐색기 창에서 `Unity.app` 또는 `UnityPlayer.dylib` 파일을 선택한 후 **열기**를 클릭합니다.
   1. **참고:** **Windows**에서 이 UI 툴을 사용하는 경우, **찾아보기** 기능은 `UnityPlayer.dylib` 파일만 지원합니다. `myApplication.app/Conents/Frameworks/UnityPlayer.dylib`에서 찾을 수 있습니다. 또한 애플리케이션의 전체 경로를 다음 필드에 입력하여 `Unity.app`을 패치할 수도 있습니다.
6. **패치** 버튼을 누릅니다.

> **참고**: Unity `2018.2` 이하 버전으로 제작된 Unity 애플리케이션에는 별도의 `UnityPlayer.dylib`이 포함되어 있지 않습니다. 이 경우에는 애플리케이션 번들(`.app`)을 선택하세요.

![Unity 애플리케이션 패치 프로그램 macOS.](../images/unity-application-patcher-macos-kr.png)<br/>*macOS 애플리케이션을 패치하는 툴입니다.*

패치에 성공한 경우, **패치 결과** 로그 헤더에 **성공**이 표시되고, **결과 로그**에 패치 프로세스에 관한 정보가 포함됩니다.

패치에 실패한 경우, 툴의 하단 로그에 패치 프로세스와 실패 요인에 관한 정보가 제공됩니다. 또한 **결과 로그** 폴드아웃 아래 **로그 열기** 버튼은 추가 진단을 위해 애플리케이션 콘솔 로그를 텍스트 파일로 엽니다.

**양식 지우기** 버튼은 버전 정보와 서명 정보를 지우고, **로그 지우기** 버튼은 UI에서 결과 로그를 지웁니다.

<a id="command-line-tool-macos-application-patcher"></a>
## 커맨드 라인 툴(macOS 애플리케이션 패치 프로그램)

이 툴은 애플리케이션의 `UnityPlayer.dylib`를 유니티 웹사이트에서 다운로드하여 보안이 강화된 패치된 버전으로 전환하는 커맨드 라인을 지원합니다.

`Windows`에서의 커맨드 라인 사용법:

```shell
UnityApplicationPatcherCLI -macos -applicationPath <path/to/my/application.app>
UnityApplicationPatcherCLI -macos -unityPlayerLibrary <path/to/my/application.app/Contents/Frameworks/UnityPlayer.dylib>
UnityApplicationPatcherCLI -macos -unityPlayerLibrary <path/to/my/application/UnityPlayer.dylib>
UnityApplicationPatcherCLI -macos -unityPlayerLibrary <path/to/UnityPlayer.dylib> -allowStandaloneLibrary
```

`macOS`에서의 커맨드 라인 사용법:

```shell
UnityApplicationPatcher.app/Contents/MacOS/UnityApplicationPatcherCLI -macos -applicationPath <path/to/my/application.app>
UnityApplicationPatcher.app/Contents/MacOS/UnityApplicationPatcherCLI -macos -unityPlayerLibrary <path/to/my/application.app/Contents/Frameworks/UnityPlayer.dylib>
UnityApplicationPatcher.app/Contents/MacOS/UnityApplicationPatcherCLI -macos -unityPlayerLibrary <path/to/my/application/UnityPlayer.dylib>
UnityApplicationPatcher.app/Contents/MacOS/UnityApplicationPatcherCLI -macos -unityPlayerLibrary <path/to/UnityPlayer.dylib> -allowStandaloneLibrary
```

> **참고**: 커맨드 라인 사용법, 옵션, 인자에 대한 자세한 내용을 보려면 `-help` 또는 `-h` 인자를 사용하세요.

<a id="application-return-codes"></a>
### 애플리케이션 반환 코드

Unity 애플리케이션 패치 프로그램은 패치 작업의 결과를 나타내는 특정 반환 코드와 함께 종료됩니다. 이러한 코드는 특히 자동화, 스크립팅 및 문제 해결에 유용합니다.

| 코드 | 설명                        | 시나리오                                                                                                      |
|------|------------------------------------|---------------------------------------------------------------------------------------------------------------|
| 0    | 성공                            | 패치가 성공적으로 적용되었거나 help 커맨드가 성공적으로 실행되었습니다.                                          |
| 1    | 패치 실패(일반)             | 패치 작업이 어떤 이유에서든 실패했습니다.                                                                        |
| 2    | 패치를 찾을 수 없음(실패 시)       | 이 바이너리에 대한 패치를 찾을 수 없습니다.                                                                 |
| 3    | 예외 발견                   | 패치 프로세스 중 예외가 발견되었습니다.                                                          |
| 64   | 잘못된 커맨드 라인 인자      | 잘못된 커맨드 라인 인자가 수신되었습니다. 위의 커맨드 라인 인자를 참고하세요.                                |
| 183  | 패치가 필요하지 않음(이미 적용됨) | 패치를 적용할 수 없으나 성공적인 결과로 간주됩니다(패치가 이미 적용되었거나 필요하지 않음). |

<a id="code-signing-and-notarization"></a>
## 코드 서명 및 인증

패치 프로세스는 모든 기존 코드 서명을 무효화합니다. 애플리케이션이 이전에 서명 및/또는 인증된 경우, 다시 서명 및/또는 인증해야 합니다.

> **주의**: 패치 프로세스는 이전에 서명한 방식에 관계없이 애플리케이션을 임시 서명 상태로 만듭니다. 애플리케이션을 패치한 후, 애플리케이션이 어떻게 서명되었는지 확인하고, 필요 시 다시 서명 및 인증하세요.

애플리케이션을 서명 및/또는 인증하는 방법은 다음을 참조하세요.

* [Code sign and notarize your macOS application(macOS 애플리케이션 코드 서명 및 인증)](https://docs.unity3d.com/6000.3/Documentation/Manual/macos-building-notarization.html)(Unity 매뉴얼)
* [Notarizing macOS software before distribution(배포 전 macOS 소프트웨어 인증](https://developer.apple.com/documentation/security/notarizing-macos-software-before-distribution)(Apple 개발자 기술 자료)
