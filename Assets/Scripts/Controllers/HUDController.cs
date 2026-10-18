using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Controllers {
    public class HUDController : MonoBehaviour {
        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI versionText;
        [SerializeField] private TextMeshProUGUI usersText;
        [SerializeField] private GameClock gameClock;
        [SerializeField] private TextMeshProUGUI dateText;
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("Network")]
        [SerializeField] private BandwidthManager bbsManager;
        [SerializeField] private Image uploadFill;
        [SerializeField] private TextMeshProUGUI uploadText;
        [SerializeField] private Image downloadFill;
        [SerializeField] private TextMeshProUGUI downloadText;
        [SerializeField] private Image bandwidthFill;

        [Header("Resources")]
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private TextMeshProUGUI modemsText;
        [SerializeField] private TextMeshProUGUI modemPortsText;
        [SerializeField] private TextMeshProUGUI serversText;

        [Header("Errors")]
        [SerializeField] private GameObject modemError;
        [SerializeField] private GameObject serverError;
        [SerializeField] private Button modemRepairButton;
        [SerializeField] private Button serverRepairButton;
        [SerializeField] private TextMeshProUGUI modemRepairText;
        [SerializeField] private TextMeshProUGUI serverRepairText;

        [Header("Upgrade Costs")]
        [SerializeField] private TextMeshProUGUI modemCost;
        [SerializeField] private TextMeshProUGUI modemPortsCost;
        [SerializeField] private TextMeshProUGUI serverCost;
        [SerializeField] private TextMeshProUGUI bandwidthCost;
        [SerializeField] private Button billButton;
        [SerializeField] private TextMeshProUGUI billCost;

        [SerializeField] private Button modemUpgrade;
        [SerializeField] private Button modemPortsUpgrade;
        [SerializeField] private Button serverUpgrade;
        [SerializeField] private Button bandwidthUpgrade;

        [Header("User Spawning")]
        [SerializeField] private float minimumSpawnTime = 4f;
        [SerializeField] private float maximumSpawnTime = 10f;

        [Header("Peak Hours")]
        [SerializeField] private int peakStartHour = 16;
        [SerializeField] private int peakEndHour = 22;
        [SerializeField] private float peakDemandMultiplier = 2.5f;
        [SerializeField] private float peakSpawnTimeMultiplier = 0.5f;

        [SerializeField] private GameObject settingsWindow;

        private Coroutine userSpawnRoutine;

        private void Start() {
            if (bbsManager == null) {
                Debug.LogError("LevelController requires a BandwidthManager reference.", this);
                enabled = false;
                return;
            }

            uploadText.text = $"{bbsManager.uploadCapacity:0.#}";
            downloadText.text = $"{bbsManager.downloadCapacity:0.#}";

            userSpawnRoutine = StartCoroutine(SpawnUsers());
            UpdateHUD();
        }

        private void Update() {
            UpdateHUD();
        }

        private IEnumerator SpawnUsers() {
            while (true) {
                bool isPeakHours = IsPeakHours();

                float waitMultiplier = isPeakHours ? peakSpawnTimeMultiplier : 1f;

                float wait = Random.Range(minimumSpawnTime, maximumSpawnTime) * waitMultiplier;

                yield return new WaitForSeconds(wait);

                isPeakHours = IsPeakHours();

                float demand = bbsManager.ConnectionDemand;

                if (isPeakHours) {
                    demand = Mathf.Max(demand, 0.95f);
                }

                if (Random.value < demand) {
                    bbsManager.AddRandomUser();
                }
            }
        }

        private bool IsPeakHours() {
            int hour = gameClock.CurrentDateTime.Hour;

            return hour >= peakStartHour && hour < peakEndHour;
        }

        /* private IEnumerator SpawnUsers() {
            while (true) {
                float wait = Random.Range(minimumSpawnTime, maximumSpawnTime);

                yield return new WaitForSeconds(wait);

                // 50% chance that a new user attempts to connect.
                if (Random.value < bbsManager.ConnectionDemand) {
                    bool connected = bbsManager.AddRandomUser();

                    if (!connected) {
                        Debug.Log("BBS is at connection capacity.");
                    }
                }
            }
        } */

        private void UpdateHUD() {
            float uploadUsage = bbsManager.UploadUsage;
            float downloadUsage = bbsManager.DownloadUsage;
            float bandwidthUsage = bbsManager.BandwidthUsage;

            dateText.text = gameClock.DateString();
            timeText.text = gameClock.TimeString();

            // Progress bars use normalized values between 0 and 1.
            uploadFill.fillAmount = uploadUsage;
            downloadFill.fillAmount = downloadUsage;
            bandwidthFill.fillAmount = bandwidthUsage;

            // uploadText.text = $"{bbsManager.UploadDemand:0.0} / " + $"{bbsManager.uploadCapacity:0.#}";
            // downloadText.text = $"{bbsManager.DownloadDemand:0.0} / " + $"{bbsManager.downloadCapacity:0.#}";
            uploadText.text = bbsManager.FormatBandwidth(bbsManager.uploadCapacity);
            downloadText.text = bbsManager.FormatBandwidth(bbsManager.downloadCapacity);
            usersText.text = bbsManager.UserCount.ToString();
            modemsText.text = $"{bbsManager.ModemCount} / " + $"{bbsManager.MaxModems}";
            modemPortsText.text = $"({bbsManager.UsersPerModem} users per)";
            serversText.text = $"{bbsManager.ServerCount} / " + $"{bbsManager.ServerCount}";
            usersText.text = $"{usersText.text} / " + $"{bbsManager.MaxUsers}";

            moneyText.text = $"{bbsManager.Money:0}€";

            modemCost.text = $"{bbsManager.ModemCost:N0}€";
            modemPortsCost.text = $"{bbsManager.ModemPortsCost:N0}€";
            serverCost.text = $"{bbsManager.ServerCost:N0}€";
            bandwidthCost.text = $"{bbsManager.BandwidthCost:N0}€";

            modemUpgrade.interactable = bbsManager.Money >= bbsManager.ModemCost && bbsManager.ModemCount < bbsManager.MaxModems;
            modemPortsUpgrade.interactable = bbsManager.Money >= bbsManager.ModemPortsCost;
            serverUpgrade.interactable = bbsManager.Money >= bbsManager.ServerCost;
            bandwidthUpgrade.interactable = bbsManager.Money >= bbsManager.BandwidthCost;

            modemRepairText.text = $"{bbsManager.ModemRepairCost:N0}€";
            serverRepairText.text = $"{bbsManager.ServerRepairCost:N0}€";

            modemRepairButton.interactable = bbsManager.Money >= bbsManager.ModemCost && bbsManager.BrokenModems > 0;
            serverRepairButton.interactable = bbsManager.Money >= bbsManager.ServerCost && bbsManager.BrokenServers > 0;

            if (bbsManager.BrokenModems > 0) {
                modemError.SetActive(true);
            }

            if (bbsManager.BrokenServers > 0) {
                serverError.SetActive(true);
            }

            billButton.interactable = bbsManager.IsBillButtonActive && bbsManager.Money >= bbsManager.MonthlyBillAmount;
            billCost.text = $"{bbsManager.MonthlyBillAmount:N0}€";
        }

        public void RepairModem() {
            bbsManager.RepairModems();
        }

        public void RepairServer() {
            bbsManager.RepairServers();
        }

        public void OpenMenu() {
            settingsWindow.SetActive(!settingsWindow.activeSelf);
        }

        private void OnDestroy() {
            if (userSpawnRoutine != null)
                StopCoroutine(userSpawnRoutine);
        }
    }
}
