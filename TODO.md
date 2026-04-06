# TODO

## 다음 버전

현재 버전: **v1.9.1**

## 계획
[x] 현재 Systems의 동작 방식은 함수가 순서대로 실행하는 중에 하나의 system에서 오류가 나면 그 이후 System이 줄줄이 실패하는 방식. 이 부분 검토 필요
[x] Unregister 대신 AddSystem과 일치하는 Remove 또는 다른 표현
[x] EventQueueComponent를 외부에 노출하는게 아니라 UnityDOTS의 Entity Command Buffer처럼 Context를 통해 Event 예약을 거는 방식으로. 내부적으로 QueueComponent 처리. 또는 그에 맞는 다른 구현
[x] FixedEvent 관련 내용 구현하기

## 테스트 보완
[x] 현재 구현된 내용을 기반으로 부족한 테스트 있는지 자동으로 확인 후 작성
