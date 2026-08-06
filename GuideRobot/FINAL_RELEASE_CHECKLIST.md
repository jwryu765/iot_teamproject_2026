# GuideRobot Web HMI 최종 발행 체크리스트

## 1단계: 서버·관리자 화면 담당자에게 먼저 전달

전달 문서:

- `SERVER_HANDOFF.md`

서버·관리자 화면 담당자가 합의하고 구현할 항목:

- Qt 관리자 화면에서 수동 조종 기능을 켤 때 서버 상태를 `manual_mode: true`로 변경
- Qt 관리자 화면에서 수동 조종 기능을 끌 때 서버 상태를 `manual_mode: false`로 변경
- C++ 서버의 `GET /api/status` 응답에 JSON boolean 형식의 `manual_mode` 포함

권장 관리자 화면 → 서버 API:

```http
POST /api/manual-mode
Content-Type: application/json
```

```json
{
  "manual_mode": true
}
```

관리자 화면과 서버에 이미 다른 명령 규격이 있다면 그 규격을 사용해도 됩니다. Web HMI에는 `GET /api/status`의 `manual_mode` 값만 정확히 전달되면 됩니다.

## 2단계: 간소화된 안내 화면 확인

실제 값이 아닌 숫자 진행률과 약 25초짜리 임시 타이머는 제거했습니다. 안내 중에는 다음 정보만 표시합니다.

- 선택한 목적지
- “안내 중” 상태
- 이동 중임을 나타내는 경로 애니메이션
- 안내 취소 버튼

서버는 숫자 `progress` 값을 제공하지 않아도 됩니다. `manual_mode` 이외의 주행 상태·도착 여부 연동은 필요할 경우 후속으로 추가할 수 있습니다.

## 3단계: 실제 통합 테스트

- [ ] 일반 상태에서 목적지 선택 가능
- [ ] 안내 시작 명령이 서버에 정상 전달됨
- [ ] 안내 취소 명령이 서버에 정상 전달되고 실제 Nav2 목표가 취소됨
- [ ] 대기 중 수동 모드 ON 시 1초 안에 관리자 제어 화면 표시
- [ ] 수동 모드 OFF 시 1초 안에 관리자 제어 화면 자동 제거
- [ ] 안내 중 수동 모드 ON 시 안내 취소 버튼 잠금
- [ ] 안내 중 수동 모드 OFF 시 사용자 화면 조작 복구
- [ ] 페이지를 새로 열었을 때 이미 수동 모드라면 즉시 관리자 제어 화면 표시
- [ ] `manual_mode`가 문자열이 아닌 JSON boolean으로 전달됨
- [ ] Ubuntu 서버에서 Nginx 외부 주소로 접속 가능

## 4단계: 통합 테스트 후 Web HMI 최종 작업

- 테스트에서 발견된 API 필드명·화면 동작 수정
- 간소화된 안내 화면 최종 확인
- 운영 API 주소 `http://127.0.0.1:8080/` 확인
- Release 빌드 확인
- Production 모드 HTTP 응답 확인
- 서버 전달 문서 최신화
- Git 최종 커밋

## 5단계: 최종 발행본 생성

Ubuntu 서버에 .NET 10이 설치되어 있으므로 프레임워크 종속 방식으로 발행합니다.

최종 전달물:

- `GuideRobot.WebHmi-ubuntu-net10.zip`
- `README_UBUNTU.md`
- `SERVER_HANDOFF.md`
- ZIP SHA-256 체크섬

현재 생성된 ZIP은 연동 확인용 후보본입니다. 3단계 통합 테스트와 4단계 최종 수정이 끝난 후 새로 발행한 ZIP을 최종본으로 전달합니다.
