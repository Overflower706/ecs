using System;
using UnityEngine;

namespace OVFL.ECS
{
    /// <summary>
    /// Event Entity의 마커 컴포넌트. EventCleanupSystem이 이 컴포넌트를 통해 모든 Event를 감지합니다.
    /// </summary>
    public class EventMetadataComponent : IComponent
    {
        /// <summary>Event가 생성된 시간 (Time.time)</summary>
        public float CreatedTime { get; set; }

        /// <summary>Event 타입 이름 (디버깅용)</summary>
        public string EventTypeName { get; set; }

        /// <summary>FixedUpdate 주기 이벤트 여부. true면 FixedEventCleanupSystem이 처리합니다.</summary>
        public bool IsFixed { get; set; }

#if UNITY_EDITOR
        /// <summary>Event 생성 위치 StackTrace (에디터 전용)</summary>
        public string StackTrace { get; set; }
#endif
    }
}
