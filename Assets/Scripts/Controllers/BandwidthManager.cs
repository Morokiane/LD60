using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;

namespace Controllers {
    [System.Serializable]
    public class BBSUser {
        public float downloadDemand;
        public float uploadDemand;
        public float downloadUsage;
        public float uploadUsage;
        public float sessionTime;

        public BBSUser(float download, float upload, float duration) {
            downloadDemand = download;
            uploadDemand = upload;
            sessionTime = duration;
        }
    }

    public class BandwidthManager : MonoBehaviour {
        [Header("Hardware")]
        [SerializeField] private int serverCount = 1;
        [SerializeField] private int modemCount = 1;

        [Header("Hardware Limits")]
        [SerializeField] private int modemsPerServer = 4;
        [SerializeField] private int usersPerModem = 4;
        [SerializeField] private int modemPorts = 0;

        [Header("Hardware Reliability")]
        [SerializeField] private float modemFailureChancePerMinute = 0.02f;
        [SerializeField] private float serverFailureChancePerMinute = 0.02f;
        [SerializeField] private float gracePeriodInGameDays = 7f;

        [Header("Repair Costs")]
        [SerializeField] private float modemRepairCost = 15f;
        [SerializeField] private float serverRepairCost = 75f;

        public int ServerCount => serverCount;
        public int ModemCount => modemCount;
        public int UsersPerModem => usersPerModem;
        public int MaxModems => serverCount * modemsPerServer;
        // public int MaxUsers => Mathf.Min(modemCount, MaxModems) * usersPerModem;
        public int MaxUsers => WorkingModems * usersPerModem;
        public int BrokenModems => brokenModems;
        public int BrokenServers => brokenServers;
        public float ModemRepairCost => modemRepairCost;
        public float ServerRepairCost => serverRepairCost;
        
        [Header("Network Capacity (KB/s)")]
        [Min(0.1f)] public float downloadCapacity = 30f;
        [Min(0.1f)] public float uploadCapacity = 10f;

        [Header("Users")] public int maximumConnections = 4;
        public List<BBSUser> users = new();
        public float DownloadDemand { get; private set; }
        public float UploadDemand { get; private set; }
        public float Money { get; private set; }
        public float DownloadUsage => Mathf.Clamp01(DownloadDemand / downloadCapacity);
        public float UploadUsage => Mathf.Clamp01(UploadDemand / uploadCapacity);

        [SerializeField] private float feePerUserPerHour = 0.10f;
         
        // Overall network utilization is the busiest direction.
        public float BandwidthUsage => Mathf.Max(DownloadUsage, UploadUsage);
        public int UserCount => users.Count;

        [Header("User Demand")]
        [SerializeField] private float connectionDemand = 0.2f;
        [SerializeField] private float demandGrowthPerMinute = 0.01f;
        public float ConnectionDemand => connectionDemand;

        [Header("Upgrade Costs")]
        [SerializeField] private float modemBaseCost = 50f;
        [SerializeField] private float modemPortBaseCost = 25f;
        [SerializeField] private float serverBaseCost = 250f;
        [SerializeField] private float bandwidthBaseCost = 50f;

        [Header("Cost Multipliers")]
        [SerializeField] private float modemCostMultiplier = 1.25f;
        [SerializeField] private float modemPortCostMultiplier = 1.18f;
        [SerializeField] private float serverCostMultiplier = 1.22f;
        [SerializeField] private float bandwidthCostMultiplier = 1.27f;

        [Header("User Bandwidth Usage")]
        [SerializeField] private float minUploadUsage = 2f;
        [SerializeField] private float maxUploadUsage = 20f;
        [SerializeField] private float minDownloadUsage = 2f;
        [SerializeField] private float maxDownloadUsage = 20f;

        [Header("Monthly Bill")]
        [SerializeField] private GameClock gameClock;
        [SerializeField] private float monthlyBillAmount = 250f;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI infoText;

        private int modemUpgradesPurchased;
        private int modemPortsUpgradesPurchased;
        private int serverUpgradesPurchased;
        private int bandwidthUpgradesPurchased;
        private int brokenModems;
        private int brokenServers;
        private DateTime billDueDate;
        private bool billPaid;
        private bool isShutdown;
        private DateTime hardwareProtectionEnds;
        private bool hardwareProtectionActive = true;
        private string info;

        public bool IsShutdown => isShutdown;

        private void Start() {
            InitializeMonthlyBill();
            hardwareProtectionEnds = gameClock.CurrentDateTime.AddDays(gracePeriodInGameDays);
        }

        private void Update() {
            RefreshMonthlyBill();
            if (isShutdown) return;

            Money += users.Count * feePerUserPerHour * Time.deltaTime;

            connectionDemand += demandGrowthPerMinute / 15f * Time.deltaTime;
            connectionDemand = Mathf.Clamp01(connectionDemand);

            // Remove users whose sessions have ended.
            for (int i = users.Count - 1; i >= 0; i--) {
                users[i].sessionTime -= Time.deltaTime;

                if (users[i].sessionTime <= 0f)
                    users.RemoveAt(i);
            }

            ProcessHardwareFailure(Time.deltaTime);
            RecalculateBandwidth();
        }

        private void InitializeMonthlyBill() {
            DateTime currentDate = gameClock.CurrentDateTime.Date;

            billDueDate = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));
            billPaid = false;
            isShutdown = false;

            RefreshMonthlyBill();
        }

        private void RefreshMonthlyBill() {
            bool shouldShutdown = !billPaid && gameClock.CurrentDateTime.Date >= billDueDate;

            if (shouldShutdown && !isShutdown) {
                isShutdown = true;

                users.Clear();
                infoText.text = "Monthly bill overdue! BBS Shutdown";
                Debug.Log("Monthly bill overdue! BBS Shutdown");
            }
        }

        public bool PayMonthlyBill() {
            if (billPaid || !IsBillButtonActive) return false;

            if (Money < monthlyBillAmount) {
                infoText.text = "Not enough money to pay bill";
                Debug.Log("Not enough money to pay bill");
                return false;
            }

            Money -= monthlyBillAmount;
            billPaid = true;

            DateTime nextMonth = billDueDate.AddDays(1);

            billDueDate = new DateTime(nextMonth.Year, nextMonth.Month, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
            billPaid = false;
            RefreshMonthlyBill();

            infoText.text = "Bill paid";
            Debug.Log("Bill paid");
            return true;
        }

        public void PayMonthlyBillButton() {
            PayMonthlyBill();
        }

        public bool AddRandomUser() {
            if (isShutdown) return false;

            int activity = Random.Range(0, 3);

            float download = 0.5f;
            float upload = 0.2f;

            switch (activity) {
                case 0:
                    download = Random.Range(0.5f, 2f);
                    upload = Random.Range(0.1f, 0.5f);
                break;

                case 1:
                    download = Random.Range(5f, 12f);
                    upload = Random.Range(0.1f, 0.5f);
                break;

                case 2:
                    download = Random.Range(0.1f, 0.5f);
                    upload = Random.Range(1f, 10f);
                break;
            }

            if (users.Count >= MaxUsers) return false;

            float availableUpload = uploadCapacity - GetCurrentUploadUsage();
            float availableDownload = downloadCapacity - GetCurrentDownloadUsage();

            if (upload > availableUpload || download > availableDownload) return false;
            
            float duration = Random.Range(15f, 45f);

            if (users.Count >= MaxUsers || GetCurrentDownloadUsage() + download > downloadCapacity || GetCurrentUploadUsage() + upload > uploadCapacity) {
                return false;
            }

            users.Add(new BBSUser(download, upload, duration));

            RecalculateBandwidth();
            return true;
        }

        /* public bool AddRandomUser() {
            if (users.Count >= MaxUsers) {
                return false;
            }

            // 0 = reading, 1 = downloading, 2 = uploading
            int activity = Random.Range(0, 3);

            float download = 0.5f;
            float upload = 0.2f;

            switch (activity) {
                case 0: // Reading message boards
                    download = Random.Range(0.5f, 2f);
                    upload = Random.Range(0.1f, 0.5f);
                    break;

                case 1: // Downloading a file
                    download = Random.Range(5f, 12f);
                    upload = Random.Range(0.1f, 0.5f);
                    break;

                case 2: // Uploading a file
                    download = Random.Range(0.1f, 0.5f);
                    upload = Random.Range(2f, 8f);
                    break;
            }

            float duration = Random.Range(15f, 45f);

            users.Add(new BBSUser(download, upload, duration));

            RecalculateBandwidth();
            return true;
        } */

        private void RecalculateBandwidth() {
            DownloadDemand = 0f;
            UploadDemand = 0f;

            foreach (BBSUser user in users) {
                DownloadDemand += user.downloadDemand;
                UploadDemand += user.uploadDemand;
            }
        }

        public float GetDownloadSpeedFactor() {
            if (DownloadDemand <= 0f) {
                return 1f;
            }

            return Mathf.Min(1f, downloadCapacity / DownloadDemand);
        }

        public float GetUploadSpeedFactor() {
            if (UploadDemand <= 0f) {
                return 1f;
            }

            return Mathf.Min(1f, uploadCapacity / UploadDemand);
        }

        public string FormatBandwidth(float bandwidth) {
            if (bandwidth >= 1000f) {
                return $"{bandwidth / 1000f:0.##} MB/s";
            }

            return $"{bandwidth:0.##} KB/s";
        }

        public void BuyModemButton() {
            BuyModem();
        }

        public void BuyServerButton() {
            BuyServer();
        }

        public bool BuyModem() {
            if (modemCount >= MaxModems || Money < ModemCost) return false;
            
            Money -= ModemCost;
            modemCount++;
            modemUpgradesPurchased++;
            return true;
        }

        public void IncreaseModemPorts() {
            if (modemPorts < 33 || Money < ModemPortsCost) {
                usersPerModem += 4;
                modemPorts += 4;
                Money -= ModemPortsCost;
                modemPortsUpgradesPurchased++;
            }          
        }

        public bool BuyServer() {
            if (Money < ServerCost) return false;

            serverCount++;
            modemCount++;
            Money -= ServerCost;
            serverUpgradesPurchased++;

            return true;
        }

        public void IncreaseBandwidth() {
            if (Money < BandwidthCost) return;

            uploadCapacity += 50;
            downloadCapacity += 50;
            Money -= BandwidthCost;
            bandwidthUpgradesPurchased++;
        }

        private float CalculateUpgradeCost(float baseCost, float multiplier, int upgradesPurchased) {
            return Mathf.Ceil(baseCost * Mathf.Pow(multiplier, upgradesPurchased));
        }

        private float GetCurrentUploadUsage() {
            float total = 0f;

            foreach (BBSUser user in users) {
                total += user.uploadUsage;
            }

            return total;
        }

        private float GetCurrentDownloadUsage() {
            float total = 0f;

            foreach (BBSUser user in users) {
                total += user.downloadUsage;
            }

            return total;
        }

        private void ProcessHardwareFailure(float deltaTime) {
            if (gameClock.CurrentDateTime < hardwareProtectionEnds) return;
            int workingModemCount = modemCount - brokenModems;

            for (int i = 0; i < workingModemCount; i++) {
                float chance = modemFailureChancePerMinute * deltaTime / 60f;

                if (Random.value < chance) {
                    brokenModems++;
                    infoText.text = "A modem has broke";
                    Debug.Log("A modem has broke");
                }
            }

            int workingServerCount = serverCount - brokenServers;

            for (int i = 0; i < workingServerCount; i++) {
                float chance = serverFailureChancePerMinute * deltaTime / 60f;

                if (Random.value < chance) {
                    brokenServers++;
                    infoText.text = "A server has broke";
                    Debug.Log("A server has broken");
                }
            }
        }

        public bool RepairModems() {
            if (brokenModems <= 0 || Money < modemRepairCost) return false;

            Money -= modemRepairCost;
            brokenModems--;

            infoText.text = "Modem repaired";
            Debug.Log("Modem repaired");
            return true;
        }

        public bool RepairServers() {
            if (brokenServers <= 0 || Money < serverRepairCost) return false;

            Money -= serverRepairCost;
            brokenServers--;

            infoText.text = "Server repaired";
            Debug.Log("Server repaired");
            return true;
        }

        public float ModemCost => CalculateUpgradeCost(modemBaseCost, modemCostMultiplier, modemUpgradesPurchased);

        public float ModemPortsCost => CalculateUpgradeCost(modemPortBaseCost, modemPortCostMultiplier, modemPortsUpgradesPurchased);

        public float ServerCost => CalculateUpgradeCost(serverBaseCost, serverCostMultiplier, serverUpgradesPurchased);

        public float BandwidthCost => CalculateUpgradeCost(bandwidthBaseCost, bandwidthCostMultiplier, bandwidthUpgradesPurchased);

        public int WorkingServers => Mathf.Max(0, serverCount - brokenServers);

        public int WorkingModems => Mathf.Min(Mathf.Max(0, modemCount - brokenModems), WorkingServers * modemsPerServer);

        public bool IsBillButtonActive => !billPaid && gameClock != null && gameClock.CurrentDateTime >= billDueDate.AddDays(-5);

        public float MonthlyBillAmount => monthlyBillAmount;

        public DateTime BillDueDate => billDueDate;
    }
}
