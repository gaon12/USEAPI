# USEAPI

WinForms 기반 텍스트 뷰어, 웹 검색, Papago 번역 도구입니다.

## 주요 기능

- 텍스트 파일을 열어 내용을 확인합니다.
- 선택한 텍스트를 Papago 번역 입력창으로 보냅니다.
- 선택한 텍스트를 네이버 검색으로 바로 검색합니다.
- 기본 홈 URL을 설정 창에서 저장합니다.

## 설정 저장

홈 URL은 실행 파일 위치가 아니라 사용자별 앱 데이터 폴더에 저장됩니다.

```text
%LOCALAPPDATA%\USEAPI\settings.json
```

기존 저장소의 `USEAPI\textFile\URL.txt`는 첫 실행 마이그레이션용으로만 읽습니다. 새로 저장한 값은 위 설정 파일에 기록됩니다. 설정을 읽거나 쓰다 실패하면 `%LOCALAPPDATA%\USEAPI\error.log`에 원인이 기록되고, 기본 홈은 `https://www.naver.com/`을 사용합니다.

## Papago API 키

API 키는 소스 코드에 넣지 않습니다. 실행 전에 환경 변수로 설정하세요.

```powershell
$env:NAVER_CLIENT_ID = "발급받은 Client ID"
$env:NAVER_CLIENT_SECRET = "발급받은 Client Secret"
```

영구 설정이 필요하면 Windows 사용자 환경 변수에 같은 이름으로 등록하면 됩니다.

## 빌드

Visual Studio 또는 MSBuild로 `USEAPI.sln`을 빌드합니다.

```powershell
MSBuild .\USEAPI.sln /p:Configuration=Debug /p:Platform="Any CPU"
```
