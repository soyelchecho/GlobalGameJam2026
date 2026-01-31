using System;
using UnityEngine;

namespace Gameplay.Temporal
{
    public class TimeManager : MonoBehaviour
    {
        private static TimeManager instance;
        public static TimeManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<TimeManager>();

                    if (instance == null)
                    {
                        GameObject go = new GameObject("TimeManager");
                        instance = go.AddComponent<TimeManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        public event Action<TimeState> OnTimeStateChanged;

        [SerializeField] private TimeState currentState = TimeState.Present;

        public TimeState CurrentState => currentState;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetTimeState(TimeState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            OnTimeStateChanged?.Invoke(currentState);

            Debug.Log($"[TimeManager] Time changed to: {currentState}");
        }

        public void ToggleTimeState()
        {
            SetTimeState(currentState == TimeState.Present ? TimeState.Past : TimeState.Present);
        }
    }
}
