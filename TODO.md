# TODO

다음 패치 버전 (v1.1.0)

## 📋 해야할 목록

- [ ] Context에서 Entity Pool 사용하기. 현재 Entity 생성/삭제 방식으로는 성능 상의 문제와 더불어 ID 오버플로 문제 예상

## ✅ 완료

- [x] Systems.Add -> Systems.AddSystem으로 변경
- [x] Systems.AddSystem(new -()) -> Systems.AddSystem<T>(). 제네릭 지원하기
- [x] 각 ISystem이 Context 속성을 갖고 있도록 구현
- [x] Systems에 Add or Setup 될 때 Context가 할당된다.