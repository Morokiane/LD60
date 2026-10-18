using UnityEngine;
using System;

namespace Controllers {
    public class GameClock : MonoBehaviour {
        [Header("Starting Date and Time")]
        [SerializeField] private int startYear = 1992;
        [SerializeField] private int startMonth = 1;
        [SerializeField] private int startDay = 2;
        [SerializeField] private int startHour = 1;
        [SerializeField] private int startMinute = 1;

        [Header("Clock Settings")]
        [SerializeField] private float realSecondsPerGameMinute = 1f;

        private DateTime currentDateTime;
        private float elapsedSeconds;

        public DateTime CurrentDateTime => currentDateTime;

        public event Action<DateTime> OnTimeChanged;
        public event Action<DateTime> OnNewDay;

        private void Awake() {
            currentDateTime = new DateTime(startYear, startMonth, startDay, startHour, startMinute, 0);
        }

        private void Update() {
            elapsedSeconds += Time.deltaTime;

            while (elapsedSeconds >= realSecondsPerGameMinute) {
                elapsedSeconds -= realSecondsPerGameMinute;
                AdvanceMinute();
            }
        }

        private void AdvanceMinute() {
            DateTime previousDate = currentDateTime.Date;
            currentDateTime = currentDateTime.AddMinutes(1);
            OnTimeChanged?.Invoke(currentDateTime);

            if (currentDateTime.Date != previousDate) {
                OnNewDay?.Invoke(currentDateTime);
                Debug.Log($"New day: {currentDateTime:dd/MM/yyyy}");
            }
        }

        public string TimeString() {
            return currentDateTime.ToString("HH:mm");
        }

        public string DateString() {
            return currentDateTime.ToString("dd/MM/yyyy");
        }
    }
}
