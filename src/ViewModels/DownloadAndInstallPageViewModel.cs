﻿using AdvancedSharpAdbClient;
using CommunityToolkit.Mvvm.ComponentModel;
using HuaweiHMSInstaller.Helper;
using HuaweiHMSInstaller.Integrations.Analytics;
using HuaweiHMSInstaller.Models;
using HuaweiHMSInstaller.Pages;
using HuaweiHMSInstaller.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;

namespace HuaweiHMSInstaller.ViewModels
{
    public sealed partial class DownloadAndInstallPageViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IAdbOperationService _adbOperationService;
        private readonly AppSettings _settings;
        private readonly AnalyticsSubject _analyticsSubject;
        private readonly IHttpClientFactory _httpClient;


        public SearchListItem SearchListItem { get; set; }

        private List<DeviceData> adbDevices;
        private string adbFolderPath;
        private string adbPath;
        private const string AdbFolder = "adb_server";
        private const long AdbFileSize = 5938176;
        // Define an array of messages for each progress range
        public Dictionary<string, bool> AdbProgressMessages;


        private int[] thresholds;
        private readonly double[] thresholdsHmsInfo = { 20, 40, 60, 70, 80 };
        private Dictionary<string, bool> checkHmsOperation = new();

        public Action<float> OnProgressChanged;
        public Action OnThresholdReached;

        public DownloadAndInstallPageViewModel(
                INavigationService navigationService, 
                IAdbOperationService adbOperationService,
                AnalyticsSubject analyticsSubject,
                IHttpClientFactory httpClient,
                IConfiguration configuration)
            : base(navigationService)
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(DownloadAndInstallPageViewModel)}:  ctor");
            _adbOperationService = adbOperationService;
            _settings = configuration.GetSection("Settings").Get<AppSettings>();
            _httpClient = httpClient;
            _analyticsSubject = analyticsSubject;
            _analyticsSubject.Notify("Download and Install Page Loaded");
            Init();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("SearchListItem"))
            {
                SearchListItem = query["SearchListItem"] as SearchListItem;
            }
            OnPropertyChanged(nameof(SearchListItem));
        }

        ~DownloadAndInstallPageViewModel()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(DownloadAndInstallPageViewModel)}:  deconstructor");
        }

        private void Init()
        {
            AdbProgressMessages = AdbMessagesConst.InitializeMessages();
            adbPath = Path.Combine(_settings.ProjectOperationPath, AdbFolder, "platform-tools", "adb.exe");
            adbFolderPath = Path.Combine(_settings.ProjectOperationPath, AdbFolder);
        }

        public async void NavigateToThanksPage()
        {
            await NavigateToAsync<ThanksPage>();
        }

        public async ValueTask<List<DeviceData>> AdbServerandDeviceCheckAsync()
        {

            var adbServerCheck = await AdbServerOperationAsync();

            if (adbServerCheck)
            {
                adbDevices = await _adbOperationService.GetDevices();
            }

            return adbDevices;
        }

        // Define a method to perform the adb server operation asynchronously
        private async ValueTask<bool> AdbServerOperationAsync()
        {
            // Create a progress object that invokes the UpdateProgress method
            IProgress<float> progressAdb = new Progress<float>(OnProgressChanged);

            // Check if the adb server is running
            var adbServerCheck = _adbOperationService.CheckAdbServer();

            try
            {
                if (!adbServerCheck)
                {
                    // Check if the adb folder file exists
                    var hasAdbFolderFile = AdbFolderFileCheckOperation();

                    if (!hasAdbFolderFile)
                    {
                        // Download the adb from internet and check again
                        await _adbOperationService.DownloadAdbFromInternetAsync(progressAdb);
                        hasAdbFolderFile = AdbFolderFileCheckOperation();
                    }

                    if (hasAdbFolderFile)
                    {
                        // Start the adb server and check the status
                        var status = AdbServer.Instance.StartServer(adbPath, true);

                        if (status == StartServerResult.Started || status == StartServerResult.AlreadyRunning)
                        {
                            // Disable the adb progress messages and create an adb client
                            DisableAdbProgressMessages();
                            adbServerCheck = true;
                            await _adbOperationService.CreateAdbClient();
                        }
                    }
                }
                else
                {
                    // Disable the adb progress messages, update the thresholds and create an adb client
                    DisableAdbProgressMessages();
                    OnThresholdReached?.Invoke();
                    await _adbOperationService.CreateAdbClient();
                }
            }
            catch (Exception)
            {
                // Kill the adb process and retry the operation
                WorkingProcessAndPort.KillProcess("adb");
                await AdbServerOperationAsync();
            }

            return adbServerCheck;
        }

        private bool AdbFolderFileCheckOperation()
        {
            var hasAdbFolder = Directory.Exists(adbFolderPath);
            var hasAdbFile = File.Exists(adbPath);

            if (hasAdbFolder && hasAdbFile)
            {
                var adbFolderFileCount = Directory.GetFiles(Path.Combine(adbFolderPath, "platform-tools"))?.Length; //must be 15
                var adbFileSize = new FileInfo(adbPath).Length;
                if (adbFolderFileCount == 15 && adbFileSize == AdbFileSize) return true;
            }
            return false;

        }

        // Define a method to disable the adb progress messages and update the thresholds
        private void DisableAdbProgressMessages()
        {
            AdbProgressMessages[AdbMessagesConst.DownloadingADBDriver] = false;
            AdbProgressMessages[AdbMessagesConst.InstallingADBDriver] = false;
            OnThresholdReached?.Invoke();
        }

        public async ValueTask<bool> CheckFileSize(FileInfo fileInfo, string url)
        {
            long fileSize = fileInfo.Length;
            var downloadFileSize = await HttpClientExtensions.GetFileSizeAsync(_httpClient, url);
            return fileSize == downloadFileSize;
        }

        public string UpdateCommentLabel(Dictionary<string, bool> adbProgressMessages, int currentThresholdIndex, string gameName)
        {
            StringBuilder commentBuilder = new StringBuilder();
            // Update the message
            // Update the comment label text based on the current progress and message index
            var adbProgressMsg = adbProgressMessages.Where(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
            if (adbProgressMsg.Count > currentThresholdIndex)
            {
                commentBuilder.Clear();
                var value = adbProgressMsg[currentThresholdIndex];
                if (value == AdbMessagesConst.DownloadingGame || value == AdbMessagesConst.InstallingGame)
                {
                    value += ": " + gameName;
                }
                commentBuilder.Append(value);
            }

            return commentBuilder.ToString();
        }
    }
}