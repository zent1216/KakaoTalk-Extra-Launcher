# 카카오톡 추가 실행기 (KakaoTalk Extra Launcher)

일반 카카오톡은 Windows에서 그대로 실행하고, 두 번째 카카오톡을 [Sandboxie-Plus](https://sandboxie-plus.com/) 상자 `Kakao1`에서 버튼 하나로 엽니다.

- Sandboxie-Plus 명령행(`Start.exe`, `SbieIni.exe`)만 호출합니다.
- KakaoTalk.exe 수정, 코드 주입, 인증 우회는 하지 않습니다.
- 로그인 정보를 입력받거나 저장하지 않습니다. 로그인은 카카오톡 창에서 직접 합니다.

## 요구 사항

- Windows 10/11 (.NET Framework 4.x는 Windows에 기본 포함)
- Sandboxie-Plus 설치
- 카카오톡 PC 버전 설치

## 사용법

1. 일반 카카오톡을 먼저 실행합니다.
2. `KakaoTalk-Extra-Launcher.exe`를 실행하고 **추가 계정 실행**을 누릅니다.
3. `Kakao1` 상자가 없으면 자동으로 만든 뒤 카카오톡을 엽니다.
4. 상자 안에서 카카오톡 창이 실제로 뜨는 것을 확인해야 "성공"으로 기록합니다.

실행 파일은 [Releases](../../releases)에서 받을 수 있습니다. exe 하나만 있으면 됩니다.

## 경로 자동 탐색

| 대상 | 찾는 순서 |
| --- | --- |
| Sandboxie `Start.exe` | 설치 정보(레지스트리) → `SbieSvc` 서비스 위치 → 기본 설치 폴더 |
| `KakaoTalk.exe` | 설치 정보(레지스트리) → 실행 중인 카카오톡 → 기본 설치 폴더 |

찾은 경로는 `%APPDATA%\KakaoExtraLauncher\settings.ini`에 저장하고, 파일이 사라졌을 때만 다시 찾습니다. 화면의 **찾기** 버튼으로 직접 지정할 수도 있습니다.

## 빌드

Visual Studio 없이 Windows에 들어 있는 `csc.exe`로 빌드합니다.

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

`build.ps1`은 아이콘(`obj\app.ico`)을 먼저 그려 만든 뒤 `KakaoTalk-Extra-Launcher.exe`를 컴파일합니다.

| 파일 | 내용 |
| --- | --- |
| `ExtraLauncher.cs` | 런처 화면과 동작 |
| `IconArt.cs` | 앱 아이콘 그림 (헤더와 아이콘 파일이 같이 사용) |
| `MakeIcon.cs` | 여러 크기의 아이콘 파일을 만드는 빌드 도구 |
| `build.ps1` | 빌드 스크립트 |
