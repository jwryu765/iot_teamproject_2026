# 2026 ROS 2 자율주행 안내 로봇 프로젝트

윈도우 플랫폼 기반 IoT 시스템 개발 과정 파이널 팀프로젝트

## 사전 학습 및 시뮬레이션

### ROS 2 Jazzy SLAM·자율탐사 연습

- [VMware + Gazebo + TurtleBot3를 활용한 SLAM·자율탐사](./ROS2_SLAM_PRACTICE.md)

## 실물 자율주행 안내 로봇 프로젝트

### 프로젝트 요구사항

- [자율주행 안내 로봇 요구사항 정의서](./01-requirements.md)

### 팀원 역할 분담

- [5인 팀 개발 역할 및 협업 구조](./02-team-roles.md)

## 웹 HMI (GuideRobot.WebHmi)

사용자 화면과 로봇 부착 디스플레이는 하나의 반응형 웹 HMI로 제공합니다.

- 프로젝트 위치: `GuideRobot/GuideRobot.WebHmi`
- 기술: C#, .NET 10, ASP.NET Core Blazor Server
- 지원 화면: 로봇 디스플레이(1024×600), 휴대폰, PC 브라우저
- 관리자 제어 화면: Qt

### 현재 시스템 구조

```text
휴대폰 · 로봇 디스플레이
          |
       웹 HMI
          |
     C++ 로봇 API 서버
          |
라즈베리파이 · 로봇 하드웨어

Qt 관리자 화면 ──> C++ 로봇 API 서버 ──> 웹 HMI 상태 전달(예정)
```

개발 환경에서는 Blazor 웹 HMI가 개발 PC에서 실행되고, 서버 담당자의 C++ API 서버에 명령을 전송합니다. 최종 배포 시에는 웹 HMI도 API 서버가 실행되는 서버 컴퓨터에 함께 배포합니다.

### 웹 HMI → 서버 API 규격

기본 API 경로는 `POST /api/command`이며, 테스트용 ngrok 주소는 변경될 수 있으므로 개발 환경의 `appsettings.Development.json`에서 관리합니다.

#### 목적지 안내

```json
{
  "destination": "room_301"
}
```

목적지 ID는 다음과 같이 통일합니다.

| 목적지 ID | 화면 표시 |
| --- | --- |
| `room_301` | 강의실 301호 |
| `room_302` | 강의실 302호 |
| `room_303` | 행정실 303호 |
| `room_304` | 강사대기실 304호 |
| `room_305` | 강사실 305호 |

#### 안내 취소

```json
{
  "command": "cancel"
}
```

서버는 정상 처리 시 HTTP 200과 JSON 응답을 반환해야 합니다. HTTP 요청 본문이 TCP 수신 과정에서 나뉘어 도착할 수 있으므로, 서버는 `Content-Length`만큼 본문을 모두 받은 뒤 JSON을 파싱해야 합니다.

### 관리자 수동 제어 연동 계획

Qt 관리자 화면이 수동 제어를 활성화하면 서버의 `manual_mode` 상태가 변경됩니다.

```json
{
  "manual_mode": true
}
```

서버의 `GET /api/status` 응답에는 최소한 아래 상태가 포함되어야 합니다.

```json
{
  "status": "moving",
  "manual_mode": true
}
```

웹 HMI는 이 상태를 주기적으로 조회하여, `manual_mode`가 `true`인 동안 목적지 선택과 안내 시작을 비활성화하고 “관리자가 제어 중입니다”를 표시할 예정입니다.

### 서버 배포 구조

최종 배포 시 서버 컴퓨터에서 다음 구성으로 실행합니다.

```text
휴대폰 · 로봇 디스플레이
          |
      http://서버IP/
          |
       Nginx :80
          |
  Blazor Web HMI :5000
          |
  C++ Robot API :8080
```

- 사용자는 `http://서버IP/`로 웹 화면에 접속합니다.
- Blazor 웹 HMI는 서버 내부 주소 `http://127.0.0.1:8080/`로 C++ API를 호출합니다.
- C++ API의 8080 포트는 외부에 직접 공개하지 않는 것을 원칙으로 합니다.
- 외부 인터넷 테스트가 필요할 때는 Nginx의 80번 포트에 ngrok를 연결합니다.

### 남은 실제 연동 항목

- 서버 → 웹: 로봇 연결 상태, 이동 진행률, 도착 여부 수신
- 서버 → 웹: `manual_mode` 상태 수신 및 사용자 제어 잠금
- 안내 취소 명령을 실제 Nav2 목표 취소 동작과 연결
- 실제 상태 수신 후 임시 진행률 타이머 제거
