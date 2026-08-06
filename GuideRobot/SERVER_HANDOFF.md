# GuideRobot 서버 담당자 전달 사항

## 1. 현재 구현 상태

- 사용자용 Blazor Web HMI에서 수동 제어 잠금 화면 구현이 완료되었습니다.
- Web HMI는 C++ 서버의 `GET /api/status`를 1초마다 호출합니다.
- 서버가 `manual_mode: true`를 반환하면 사용자 화면에 “관리자가 제어 중입니다” 전체 화면이 표시됩니다.
- 이때 목적지 선택, 안내 시작, 안내 취소가 모두 잠깁니다.
- 서버가 `manual_mode: false`를 반환하면 잠금이 자동으로 해제됩니다.
- 현재 방식은 WebSocket 푸시가 아니라 1초 간격 HTTP 폴링입니다.

현재 빠져 있는 연결은 다음 두 가지입니다.

- Qt 관리자 화면에서 수동 조종 기능을 켜거나 끌 때 C++ 서버로 보내는 신호
- C++ 서버가 해당 상태를 저장하고 `GET /api/status`의 `manual_mode`로 반환하는 처리

사용자 Web HMI에서 `manual_mode`를 받아 창을 표시하거나 해제하는 부분은 이미 구현되어 있습니다.

## 2. C++ 서버에서 반드시 제공할 상태 API

```http
GET /api/status
```

정상 응답 예시:

```http
HTTP/1.1 200 OK
Content-Type: application/json
```

```json
{
  "status": "moving",
  "manual_mode": true
}
```

`manual_mode`는 문자열이 아닌 JSON boolean 형식의 `true` 또는 `false`여야 합니다. 기존 상태 응답에 다른 필드가 함께 있어도 괜찮습니다.

## 3. 관리자 화면에서 수동 모드를 변경하는 권장 API

Qt 관리자 화면과 C++ 서버 사이의 수동 모드 변경 API는 아직 담당자 간에 합의하거나 구현하지 않은 상태입니다. 새로 정한다면 아래 규격을 권장합니다.

```http
POST /api/manual-mode
Content-Type: application/json
```

수동 조종 시작:

```json
{
  "manual_mode": true
}
```

수동 조종 종료:

```json
{
  "manual_mode": false
}
```

성공 응답 예시:

```json
{
  "manual_mode": true
}
```

서버에 이미 수동 모드를 변경하는 명령이나 함수가 있다면 위 API를 새로 만들 필요는 없습니다. 기존 명령을 사용하되, 최종 상태가 `GET /api/status`의 `manual_mode`에 반영되기만 하면 Web HMI와 정상 연동됩니다.

## 4. 실제 방향 조종 명령

전진·후진·좌회전·우회전·정지 명령은 Qt 관리자 화면과 C++ 서버 또는 로봇 제어부 사이에서 처리합니다. 이 명령들은 사용자 Web HMI로 보내지 않습니다.

아직 방향 조종 HTTP 규격도 정해지지 않았다면 다음과 같이 분리할 수 있습니다.

```http
POST /api/manual-control
Content-Type: application/json
```

```json
{
  "command": "forward"
}
```

사용 가능한 명령 권장값:

- `forward`
- `backward`
- `left`
- `right`
- `stop`

방향 명령은 `manual_mode`가 `true`일 때만 서버가 처리하는 것을 권장합니다. 현재 필수 요구사항은 관리자가 수동 조종 기능을 해제했을 때 `manual_mode`를 `false`로 변경하는 것입니다.

관리자 화면 종료나 통신 끊김 시 자동 정지·자동 해제하는 동작은 안전성을 위한 추가 권장사항이며, 현재 합의된 필수 요구사항은 아닙니다.

## 5. 전체 동작 순서

1. 관리자가 Qt 화면에서 수동 조종 시작을 누릅니다.
2. Qt 화면이 서버의 수동 모드 상태를 `true`로 변경합니다.
3. C++ 서버는 현재 `manual_mode` 값을 `true`로 저장합니다.
4. 사용자 Web HMI가 최대 1초 안에 `GET /api/status`로 변경된 값을 확인합니다.
5. 사용자 화면에 “관리자가 제어 중입니다”가 표시되고 사용자 조작이 잠깁니다.
6. Qt 관리자 화면의 방향 조종 명령은 C++ 서버와 로봇 제어부에서 처리합니다.
7. 관리자가 Qt 화면에서 수동 조종 기능을 해제하면 Qt 화면이 서버의 수동 모드 상태를 `false`로 변경합니다.
8. 사용자 Web HMI가 변경된 값을 확인하고 잠금을 해제합니다.

## 6. 기존 Web HMI 명령 API

목적지 안내:

```http
POST /api/command
Content-Type: application/json
```

```json
{
  "destination": "room_301"
}
```

안내 취소:

```http
POST /api/command
Content-Type: application/json
```

```json
{
  "command": "cancel"
}
```

## 7. 배포 기본 주소

- Blazor Web HMI: `http://127.0.0.1:5000`
- C++ Robot API: `http://127.0.0.1:8080/`
- 외부 접속: Nginx를 통한 `http://서버IP/`
