# GuideRobot Web HMI Ubuntu 배포

## 구성

- Ubuntu 서버에 .NET 10 Runtime이 설치되어 있어야 합니다.
- Blazor Web HMI는 기본적으로 `http://127.0.0.1:5000`에서 실행합니다.
- C++ Robot API는 기본적으로 `http://127.0.0.1:8080/`을 사용합니다.
- 외부 사용자는 Nginx를 통해 `http://서버IP/`로 접속합니다.

## 실행

발행 ZIP을 서버에 복사하여 압축을 푼 뒤 발행 폴더에서 실행합니다.

```bash
dotnet GuideRobot.WebHmi.dll --urls http://127.0.0.1:5000
```

API 주소가 다르면 환경 변수로 덮어쓸 수 있습니다.

```bash
RobotServer__BaseUrl=http://127.0.0.1:8080/ \
dotnet GuideRobot.WebHmi.dll --urls http://127.0.0.1:5000
```

## 수동 제어 상태 연동

웹 HMI는 아래 API를 1초마다 호출합니다.

```http
GET /api/status
```

응답에는 JSON boolean 형식의 `manual_mode`가 포함되어야 합니다.

```json
{
  "manual_mode": true
}
```

- `true`: 사용자 조작 잠금 및 “관리자가 제어 중입니다” 전체 화면 표시
- `false`: 잠금 해제

Qt 관리자 화면이 서버의 `manual_mode`를 변경하는 API는 관리자 화면과 C++ 서버 사이에서 정하면 됩니다. 웹 HMI는 `GET /api/status`의 최종 상태만 사용합니다.

## 목적지 도착 상태 연동

웹 HMI는 같은 `GET /api/status` 응답의 `status` 필드도 확인합니다.

```json
{
  "status": "arrived",
  "manual_mode": false
}
```

- 안내 명령 접수: `status: "moving"`
- 로봇의 실제 도착·정지 확인: `status: "arrived"`
- 안내 취소 또는 대기: `status: "idle"`

안내 중 `arrived`가 반환되면 웹 화면에 “목적지에 도착했습니다”가 표시됩니다.

## 기존 명령 API

목적지 안내:

```http
POST /api/command
Content-Type: application/json

{"destination":"room_301"}
```

안내 취소:

```http
POST /api/command
Content-Type: application/json

{"command":"cancel"}
```
