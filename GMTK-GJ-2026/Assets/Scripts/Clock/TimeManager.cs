using UnityEngine;
using System;

namespace Clock
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        public event Action<int> OnSecondChanged;
        public event Action<int> OnMinuteChanged;
        public event Action OnTimeExpired;

        [SerializeField]
        private int startingSeconds = 3600;

        private float _timer;
        private int _previousSecond = -1;
        private float _timeScale = 1f;

        public int RemainingSeconds => Mathf.CeilToInt(_timer);
        public int Hours => RemainingSeconds / 3600;
        public int Minutes => (RemainingSeconds % 3600) / 60;
        public int Seconds => RemainingSeconds % 60;
        public float TimeScale => _timeScale * GameItems.GetMultiplier(ItemStat.ClockSpeed);
        public float RemainingTimer => _timer;
        public float ElapsedTime => startingSeconds - _timer;
        public float NormalizedTime => Mathf.Clamp01(ElapsedTime / startingSeconds);
        public float TotalSecondsElapsed => ElapsedTime;
        public float CurrentSecondFraction => _timer % 60f;

        private GameManager _gameManager => GameManager.Instance;

        private void Awake()
        {
            VerifySingleton();
            _timer = startingSeconds;
        }

        private void Start()
        {
            _gameManager.OnGameReset += () => { _timer = startingSeconds; };
        }

        private void Update()
        {
            if (!_gameManager.GameActive) return;
            if (_timer <= 0)
                return;

            _timer -= Time.deltaTime * TimeScale;

            int currentSecond = Mathf.CeilToInt(_timer);

            if (currentSecond != _previousSecond)
            {
                _previousSecond = currentSecond;
                OnSecondChanged?.Invoke(Seconds);
                if (Seconds == 59)
                    OnMinuteChanged?.Invoke(Minutes);

                if (currentSecond <= 0)
                    OnTimeExpired?.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetTimeScale(float newScale)
        {
            _timeScale = newScale;
        }

        private void VerifySingleton()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}