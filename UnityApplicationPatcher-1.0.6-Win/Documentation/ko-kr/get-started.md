# Unity 애플리케이션 패치 프로그램 시작하기

Unity 애플리케이션 패치 프로그램(__UNITY_VERSION_REPLACED_BY_BUILD__)은 Android, Windows 및 macOS 애플리케이션의 패치된 버전을 제공하는 툴입니다.

> **참고**: Windows나 Mac에서 Unity 애플리케이션 패치 프로그램을 실행하거나 Android, Windows 또는 macOS 애플리케이션을 패치할 수 있습니다. 다음 섹션에서 **Android/Windows/macOS 애플리케이션 패치**는 패치 프로그램을 실행하는 운영체제(호스트)가 아닌 패치하는 애플리케이션의 플랫폼(타겟)을 의미합니다.

Android 및 Windows 애플리케이션을 패치하는 방법을 알아보려면 다음 페이지를 참조하세요.

| **주제** | **설명** |
| :-------- | :-------------- |
| [Android 애플리케이션 패치](patch-android-applications.md) | Unity 애플리케이션 패치 프로그램으로 Android 애플리케이션을 패치하는 방법을 알아봅니다. |
| [Windows 애플리케이션 패치](patch-windows-applications.md) | Unity 애플리케이션 패치 프로그램으로 Windows 애플리케이션을 패치하는 방법을 알아봅니다. |
| [macOS 애플리케이션 패치](patch-macos-applications.md) | Unity 애플리케이션 패치 프로그램으로 macOS 애플리케이션을 패치하는 방법을 알아봅니다. |

## 이 기술 자료를 활용하는 방법

Visual Studio Code, Typora 등 Markdown을 미리 볼 수 있는 모든 툴에서 Unity 애플리케이션 패치 프로그램에 대한 기술 자료를 볼 수 있습니다.

Unity 애플리케이션 패치 프로그램 기술 자료를 보려면 Markdown 뷰 툴에서 `Documentation` 폴더를 엽니다.

> **팁**: 기술 자료에서 이미지를 미리 볼 수 있도록 하려면 Markdown 뷰 툴에서 전체 `Documentation` 폴더를 엽니다. 일부 툴에서는 전체 폴더 대신 특정 파일을 열 경우 기술 자료에 이미지가 표시되지 않을 수 있습니다.

### 기술 자료 언어

기술 자료는 다음과 같은 언어로 번역되어 있으며 해당 폴더에서 확인할 수 있습니다.

* 일본어(`ja-jp`)
* 한국어(`ko-kr`)
* 중국어 간체(`zh-cn`)

## 알려진 문제

- `UnityApplicationPatcher.app`의 경로(`UnityApplicationPatcher.app` 제외)가 174자를 초과할 경우, macOS에서 Unity 애플리케이션 패치 프로그램 실행이 실패할 수 있습니다. 이 경우, UI 버전의 툴이 빈 화면으로 렌더링됩니다. 커맨드 라인 버전의 툴은 `System.IO.DirectoryNotFoundException`을 로깅하지만 앱을 패치하는 데는 성공합니다.
- `UnityApplicationPatcher.exe` 또는 `UnityApplicationPatcherCLI.exe`의 경로(`UnityApplicationPatcher.exe` 또는 `UnityApplicationPatcherCLI.exe` 제외)가 193자를 초과할 경우, Windows에서 Unity 애플리케이션 패치 프로그램 실행이 실패할 수 있습니다. 이 경우, Android 애플리케이션을 패치할 때 오류가 발생합니다. `UnityApplicationPatcher` 실행 파일의 경로가 201자를 초과할 경우, UI 버전과 커맨드 라인 버전의 툴 모두에서 치명적인 오류가 발생하며 툴을 실행할 수 없습니다.
- Windows에서 커맨드 라인 툴을 통해 Unity 애플리케이션 패치 프로그램을 사용할 때 영어가 아닌 문자는 명령 프롬프트에 올바르게 렌더링되지 않을 수 있습니다. 명령 프롬프트 글꼴을 변경(예: `SimSun-ExtB`로)함으로써 이 문제를 해결할 수 있습니다. 글꼴을 변경하려면 명령 프롬프트 창의 상단 바를 오른쪽 클릭하여 **속성**을 선택한 다음 **글꼴** 탭을 클릭하고 새 글꼴을 선택합니다. **확인**을 클릭하여 변경 사항을 적용합니다.
