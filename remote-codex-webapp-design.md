# 원격 Codex 지원 Host/Guest 앱 설계 초안

## 1. 목표

컴퓨터를 잘 다루지 못하는 지인의 PC에 작은 프로그램을 설치해 두고, 내 PC의 Host 앱에서 그 PC에 접속하여 PowerShell, Codex CLI, 파일 전송, 필요 시 음성 통화를 통해 원격으로 개발 지원을 할 수 있게 만든다.

핵심 목표는 일반 원격 제어 프로그램보다 설치와 사용을 단순하게 만드는 것이다.

## 2. 기본 구조

```text
내 PC
RemoteCodex Host
= 관리자 화면 + 내장 서버
  |
  | Tailscale IP / HTTPS / SignalR WebSocket
  v
상대 PC
RemoteCodex Guest Agent
  |
  +-- PowerShell 실행
  +-- Codex CLI 실행
  +-- 파일 읽기/쓰기/전송
  +-- 마이크 음성 전송
```

별도 VPS나 클라우드 서버를 두지 않는다. 내 PC에 설치한 Host 앱이 서버 역할을 하고, 상대 PC의 Guest Agent가 내 PC Host로 접속한다.

내 PC가 꺼져 있거나 Host 앱이 실행 중이 아니면 상대 PC에는 접속할 수 없다. 이 제한은 현재 사용 목적에 맞는 조건으로 본다.

서로 다른 인터넷망에서 쓰기 위해 기본 연결 방식은 Tailscale을 우선으로 설계한다. Tailscale을 사용하면 공유기 포트포워딩 없이 내 PC와 상대 PC가 같은 사설망에 있는 것처럼 통신할 수 있다.

웹브라우저만으로는 상대 컴퓨터의 PowerShell 실행이나 로컬 파일 수정 권한을 안정적으로 줄 수 없으므로, 상대 PC에는 Windows용 Guest Agent 프로그램을 설치한다.

## 3. 구성 요소

### 3.1 Host 앱

내 PC에 설치해서 사용하는 관리자 프로그램이다. 관리자 화면과 내장 서버를 함께 가진다.

필요 기능:

- 내장 서버 실행/중지
- 현재 접속 주소 표시
- Tailscale IP 감지
- 상대 PC 접속 상태 확인
- 접속된 PC 목록 보기
- PowerShell 터미널 열기
- 명령 실행 결과 실시간 확인
- Codex CLI 실행
- 파일 업로드
- 파일 다운로드
- 상대 PC 폴더 탐색
- 간단한 로그 확인
- 음성 통화 시작/종료

### 3.2 Host 내장 서버

Host 앱 안에서 실행되는 로컬 서버다. Guest Agent는 이 서버로 직접 접속한다.

필요 기능:

- 상대 PC 등록
- 에이전트 연결 관리
- SignalR WebSocket 세션 관리
- 명령 전달
- 터미널 출력 전달
- 파일 전송 처리
- 실행 로그 저장
- 연결 토큰 관리
- 초대 코드 생성

### 3.3 Guest Agent

상대방 컴퓨터에 설치되는 프로그램이다.

필요 기능:

- Host 앱에 자동 접속
- PC 이름, 사용자명, OS 정보 전송
- PowerShell 명령 실행
- 실행 결과 실시간 전송
- Codex CLI 실행
- 파일 업로드 수신
- 파일 다운로드 요청 처리
- 폴더 목록 조회
- 부팅 시 자동 실행
- 에이전트 종료 기능

## 4. MVP 범위

처음부터 모든 기능을 넣지 않고, 1차 버전은 아래 기능만 만든다.

### 1차 MVP

- Host 설치 파일 제작
- Guest 설치 파일 제작
- Host 앱에서 내장 서버 실행
- Guest Agent가 Tailscale IP로 Host에 접속
- Host 앱에서 접속 상태 확인
- Host 터미널에서 PowerShell 명령 실행
- 명령 출력 실시간 표시
- 파일 업로드
- 파일 다운로드
- 간단한 실행 로그 저장

### 2차 기능

- Codex CLI 실행 전용 화면
- 프로젝트 폴더 선택
- 파일 탐색기 UI
- Host 화면에서 파일 내용 보기/수정
- 명령 템플릿
- 에이전트 자동 업데이트

### 3차 기능

- 음성 통화
- 화면 공유
- 원격 클릭/키보드 입력
- 여러 PC 동시 관리
- 권한별 사용자 관리

## 5. 권장 기술 스택

### Host 앱

- UI: Blazor 또는 WPF
- Backend: ASP.NET Core 내장 서버
- Realtime: SignalR
- DB: SQLite 또는 PostgreSQL
- 파일 저장: 내 PC 로컬 디스크

### Windows 에이전트

권장 후보:

- C#
  - Windows 제어와 PowerShell 실행에 편함
  - SignalR과 궁합이 좋음
- Go
  - 단일 exe 배포가 쉬움
  - 에이전트를 가볍게 만들기 좋음

초기 개발 추천 조합:

```text
Host 앱: C# Blazor Hybrid 또는 WPF
Host 내장 서버: ASP.NET Core + SignalR
Guest Agent: C# Windows 앱 또는 콘솔 앱
DB: SQLite
연결: Tailscale IP 기반
```

개인용으로 빠르게 만들기에는 C# 기반이 가장 단순하다.

## 6. 주요 화면 설계

### 6.1 Host 시작 화면

- 내장 서버 실행/중지
- Tailscale IP 표시
- 초대 코드 생성
- Guest 설치 파일 위치 표시

### 6.2 PC 목록 화면

표시 항목:

- PC 이름
- 사용자명
- 접속 상태
- 마지막 접속 시간
- OS 정보
- 에이전트 버전

주요 버튼:

- 터미널 열기
- 파일 보기
- Codex 실행
- 음성 연결
- 로그 보기

### 6.3 터미널 화면

기능:

- PowerShell 명령 입력
- 실시간 출력 표시
- 현재 작업 폴더 표시
- 명령 히스토리
- 중단 버튼

### 6.4 Codex 실행 화면

기능:

- 프로젝트 폴더 선택
- Codex 프롬프트 입력
- 실행 결과 확인
- 생성/수정된 파일 목록 확인

예시 명령:

```powershell
cd C:\Users\상대방\Desktop\project
codex "이 프로젝트 오류를 확인하고 수정해줘"
```

### 6.5 파일 전송 화면

기능:

- 상대 PC 폴더 탐색
- 파일 업로드
- 파일 다운로드
- 새 폴더 만들기
- 파일 삭제
- 텍스트 파일 간단 편집

## 7. 에이전트 동작 방식

Guest Agent는 실행되면 Host 내장 서버에 연결한다.

```text
1. 에이전트 실행
2. 저장된 토큰으로 Host에 접속
3. Host에 PC 정보 등록
4. Host 앱에서 명령을 받음
5. PowerShell 프로세스 실행
6. stdout/stderr를 Host로 실시간 전송
7. 명령 종료 상태를 Host에 보고
```

PowerShell 실행 시 기본 작업 폴더는 사용자 홈 또는 지정된 프로젝트 폴더로 둔다.

## 8. 설치 흐름

상대방이 컴퓨터를 잘 못한다는 전제를 기준으로 설치 과정을 최대한 단순하게 만든다.

### 8.1 Host 설치 흐름

```text
1. 내 PC에 RemoteCodex-Host-Setup.exe 설치
2. Host 앱 실행
3. Host 앱이 내장 서버 실행
4. Host 앱이 내 Tailscale IP를 표시
5. 초대 코드 생성
6. 상대방에게 Guest 설치 파일과 초대 코드 전달
```

### 8.2 Guest 설치 흐름

```text
1. 상대방이 RemoteCodex-Guest-Setup.exe 실행
2. Tailscale 설치 여부 확인
3. Tailscale이 없으면 설치 안내
4. Host 주소 또는 초대 코드 입력
5. Guest Agent가 Host 앱에 연결
6. 설치 완료
7. 이후 Guest Agent는 자동 실행
```

가능하면 설치 파일은 한 개의 `.exe` 또는 `.msi`로 만든다.

### 8.3 다른 망 연결 방식

기본 방식은 Tailscale을 사용한다.

```text
내 PC Host: 100.x.x.x:7777
상대 PC Guest: Host의 Tailscale IP로 접속
```

Tailscale 사용이 어려운 경우 대안은 아래 순서로 검토한다.

- Cloudflare Tunnel
- ngrok
- 공유기 포트포워딩

포트포워딩은 설정이 어렵고 실수 위험이 있으므로 기본 방식으로 두지 않는다.

## 9. 최소 보안 설계

개인용이라도 아래 정도는 반드시 넣는다.

- 관리자 로그인
- PC별 고유 토큰
- 초대 코드는 1회용
- 초대 코드 만료 시간 설정
- 모든 명령 실행 로그 저장
- 에이전트에서 연결 해제 버튼 제공
- Host와 Guest 통신은 HTTPS/WSS 사용
- Tailscale 네트워크 안에서만 접속 허용 옵션 제공

보안을 복잡하게 만들 필요는 없지만, 실수로 다른 사람이 접속할 수 있는 구조는 피한다.

## 10. 파일 전송 설계

업로드:

```text
Host 앱 -> Guest Agent -> 지정 폴더 저장
```

다운로드:

```text
Guest Agent -> Host 앱
```

큰 파일은 조각 단위로 나누어 전송한다.

## 11. 음성 기능 설계

음성 통화는 WebRTC를 사용하는 것이 적합하다.

초기에는 아래 중 하나로 처리한다.

옵션 A:

- Host 앱과 Guest Agent 사이에 WebRTC 연결
- 에이전트가 마이크 입력을 캡처
- Host 앱으로 음성 스트리밍

옵션 B:

- 음성 기능은 별도 링크를 사용
- 예: Host 앱 안에서 임시 음성방 생성
- 구현 난이도가 낮아질 수 있음

MVP에서는 음성 기능을 제외하고, 터미널과 파일 전송을 먼저 완성한다.

## 12. 개발 순서

1. Host 앱 기본 프로젝트 생성
2. Host 내장 서버 실행 기능 구현
3. SignalR Hub 구현
4. Guest Agent 기본 프로젝트 생성
5. Guest Agent에서 Host Tailscale IP로 접속
6. Guest 등록 및 접속 상태 표시
7. Host에서 명령 보내기
8. Guest에서 PowerShell 실행
9. 출력 실시간 표시
10. 파일 업로드/다운로드 구현
11. Codex CLI 실행 편의 기능 추가
12. Host/Guest 설치 파일 제작
13. 음성 기능 검토

## 13. 우선 결정할 것

- 다른 망 연결 방식을 무엇으로 둘지
  - Tailscale
  - Cloudflare Tunnel
  - ngrok
- 에이전트 언어
  - C#
  - Go
- Host UI 방식
  - Blazor Hybrid
  - WPF
- 설치 방식
  - 단일 exe
  - MSI 설치 프로그램
- 음성 기능을 MVP에 포함할지 여부

## 14. 현재 추천 방향

처음 버전은 아래 방향으로 진행한다.

```text
Host 앱: C# Blazor Hybrid 또는 WPF
Host 내장 서버: ASP.NET Core + SignalR
Guest Agent: C#
DB: SQLite
통신: Tailscale IP + HTTPS + SignalR WebSocket
MVP 기능: PowerShell 원격 실행 + 파일 전송
```

이 방향은 Windows 제어, PowerShell 실행, 실시간 통신을 한 기술권 안에서 처리할 수 있어 초기 구현 난이도가 낮다.

## 15. 배포 파일 구조

최종 사용자가 보는 설치 파일은 2개로 둔다.

```text
RemoteCodex-Host-Setup.exe
RemoteCodex-Guest-Setup.exe
```

개발 프로젝트는 아래처럼 나눈다.

```text
RemoteCodex
  /RemoteCodex.Host
  /RemoteCodex.Guest
  /RemoteCodex.Shared
```

Host 앱 안에 내장 서버가 포함되므로 별도의 서버 설치 파일은 만들지 않는다.

## 16. 임시 프로젝트 이름 후보

- Remote Codex Helper
- Codex Remote Assist
- Friend Dev Assist
- Easy Remote Dev
- PowerCodex Remote
