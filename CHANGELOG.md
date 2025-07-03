# Changelog

All notable changes to this project will be documented in this file.

## [1.0.0] - 2025-01-03

### Added
- 초기 ECS 구현체 릴리즈
- **IComponent 인터페이스**: 순수한 데이터 컴포넌트 정의
- **Entity 클래스**: 컴포넌트 관리 (추가, 제거, 조회, 존재 확인)
- **Context 클래스**: 엔티티 생성 및 관리
- **Systems 클래스**: 시스템 등록 및 라이프사이클 관리
- **시스템 라이프사이클 인터페이스들**:
  - `ISetupSystem`: 초기화 시 한 번 실행
  - `ITickSystem`: 매 프레임 실행
  - `ICleanupSystem`: Tick 이후 정리 작업
  - `ITeardownSystem`: 마무리 시 한 번 실행
- **포괄적인 Unit Test 스위트**: 모든 핵심 기능에 대한 테스트
- **통합 테스트**: 실제 사용 시나리오 검증
- **Unity Package Manager 지원**: Git URL을 통한 패키지 설치

### Features
- **순수한 C# 구현**: Unity 의존성 최소화로 재사용성 극대화
- **체이닝 메서드 지원**: `systems.Add(sys1).Add(sys2)` 형태로 편리한 사용
- **타입 안전성 보장**: 제네릭을 활용한 컴파일 타임 타입 검사
- **학습 친화적인 구조**: 복잡한 최적화 없이 ECS 개념에 집중
- **메모리 효율적**: Dictionary 기반 컴포넌트 저장
- **확장 가능한 아키텍처**: 기본 구조 유지하면서 기능 추가 가능

### Technical Details
- Unity 2020.3 이상 지원
- .NET Standard 2.1 호환
- NUnit 기반 테스트 프레임워크
- Edit Mode 테스트 지원
- Assembly Definition 파일 포함
