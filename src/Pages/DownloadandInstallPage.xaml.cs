using AdvancedSharpAdbClient;
using HuaweiHMSInstaller.Helper;
using HuaweiHMSInstaller.Models;
using HuaweiHMSInstaller.Services;
using LocalizationResourceManager.Maui;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Text;
using static AdvancedSharpAdbClient.DeviceCommands.PackageManager;
using ServiceProvider = HuaweiHMSInstaller.Services.ServiceProvider;

namespace HuaweiHMSInstaller.Pages;

public partial class DownloadandInstallPage : ContentPage
{
    #region Definations
    private const string AdbFolder = "adb_server";
    private const long AdbFileSize = 5938176;
    // Define an array of messages for each progress range
    private Dictionary<string, bool> AdbProgressMessages;
    private string[] hmsInfoMessage;
	// Define an array of progress thresholds for each message
	private int[] thresholds;
	private readonly double[] thresholdsHmsInfo = new double[] { 20, 40, 60, 70, 80 };
    private Dictionary<string,bool> checkHmsOperation = new();
    private List<InstallApkModel> apkRecords = new();
    // Initialize variables to keep track of the current message index
    private int index = 0;
	private int indexHmsInfo = 0;
	private double globalProgress = 0;
	// Use StringBuilder for string concatenation
	private StringBuilder commentBuilder = new ();
	private StringBuilder hmsInfoBuilder = new ();

	private string adbPath;
	private string adbFolderPath;
	//HMS Links
	private const string AppGallery = "https://appgallery.cloud.huawei.com/appdl/C27162?";
	private const string HmsCore = "https://appgallery.cloud.huawei.com/appdl/C10132067?";

	private string InstallGame;
    private string GameName;
    private readonly SearchListItem GameItem;

    private CancellationTokenSource cancellationTokenSrc;
    private bool ErrorWhenDownloadingApk; 
    private List<DeviceData> adbDevices = Enumerable.Empty<DeviceData>().ToList();

    #endregion

    private readonly IAdbOperationService _adbOperationService;
    private readonly IAppGalleryService _appGalleryService;
	private readonly GlobalOptions _options;
    private readonly ILocalizationResourceManager _localizationResourceManager;
    private readonly IHttpClientFactory _httpClient;

    public DownloadandInstallPage(SearchListItem item)
	{
        _adbOperationService = ServiceProvider.GetService<IAdbOperationService>();
        _appGalleryService = ServiceProvider.GetService<IAppGalleryService>();
        _localizationResourceManager = ServiceProvider.GetService<ILocalizationResourceManager>();
        _options = ServiceProvider.GetService<IOptions<GlobalOptions>>().Value;
        _httpClient = ServiceProvider.GetService<IHttpClientFactory>();

        GameItem = item;
        Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

        InitializeComponent();
        Init();

    }

    private void Init()
    {
        ErrorWhenDownloadingApk = false;
        InstallGame = $"https://appgallery.cloud.huawei.com/appdl/{GameItem.AppId}?";
        GameName = GameItem.Name;

        AdbProgressMessages = AdbMessagesConst.InitializeMessages();
        hmsInfoMessage = HmsInfoMessagesConst.InitializeMessages();

        ThresholdOperation();
        adbPath = Path.Combine(_options.ProjectOperationPath, AdbFolder, "platform-tools", "adb.exe");
        adbFolderPath = Path.Combine(_options.ProjectOperationPath, AdbFolder);

        Dispatcher.DispatchAsync(async () =>
        {
            await DownloadAndInstallOperationAsync();
        });
    }
	private async Task DownloadAndInstallOperationAsync(){

		//label add ... animation use behavior
		this.dotAnimation.Behaviors.Add(new DotAnimationBehavior());
        this.commentLabel.Text = _localizationResourceManager.GetValue("emulator_config_progress");

        var adbDevices = await AdbServerandDeviceCheckAsync();

        if (adbDevices is null or { Count: 0 })
        {
            await AlertForNotHaveAdbDevices();
        }
        else
        {
            var checkInternet = await NetworkUtils.CheckForInternetConnectionAsync();
            var checkHuaweiService = await _appGalleryService.CheckAppGalleryCloudServiceAsync();
            if (checkInternet && checkHuaweiService)
            {
                await DownloadandInstallHmsAppsAndNavigateAsync(adbDevices.First());
            }
            else
            {
                var msg = checkInternet && !checkHuaweiService ? _localizationResourceManager.GetValue("huawei_server_unreachable") : _localizationResourceManager.GetValue("internet_connection_error");
                await AlertForNotHaveInternetAsync(msg);
            }
        }
    }
    private async ValueTask<List<DeviceData>> AdbServerandDeviceCheckAsync() {

        var adbServerCheck = await AdbServerOperationAsync();

        if (adbServerCheck)
        {
            adbDevices = await _adbOperationService.GetDevices();
        }

        return adbDevices;
    }
    private async Task DownloadandInstallHmsAppsAndNavigateAsync(DeviceData device)
    {
        try
        {
            var result = await DownloadHMSAppsAsync();
            if (result)
            {
                await InstallHmsAppsAsync(device);
                await FakeProgressAsync();
                await Task.Delay(2000);
                await Application.Current.MainPage.Navigation.PushAsync(new ThanksPage(), true);
            }
        }catch(Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

    }
    // Define a method to update the progress bar and the labels based on the value
    private void UpdateProgress(float value)
    {
        // Calculate the real value based on the thresholds length and a factor of 2
        var realValue = (value / thresholds.Length) * 2;

        // Update the global progress and the progress bar value
        globalProgress = realValue;
        this.progressBar.Progress = realValue;

        // If the progress exceeds the current threshold, increment the message index
        if (globalProgress >= thresholds[index] / 100f)
        {
            index++;
        }
        if (globalProgress >= thresholdsHmsInfo[indexHmsInfo] / 100f)
        {
            indexHmsInfo++;
        }

        // Update the comment label text based on the current message index
        commentBuilder.Clear();
        commentBuilder.Append(AdbProgressMessages.ElementAt(index).Key);
        this.commentLabel.Text = commentBuilder.ToString();

        // Update the HMS info label text based on the current message index
        hmsInfoBuilder.Clear();
        hmsInfoBuilder.Append(hmsInfoMessage[indexHmsInfo]);
        this.HMSInfoLabel.Text = hmsInfoBuilder.ToString();
    }
    // Define a method to disable the adb progress messages and update the thresholds
    private void DisableAdbProgressMessages()
    {
        AdbProgressMessages[AdbMessagesConst.DownloadingADBDriver] = false;
        AdbProgressMessages[AdbMessagesConst.InstallingADBDriver] = false;
        ThresholdOperation();
    }
    // Define a method to perform the adb server operation asynchronously
    private async ValueTask<bool> AdbServerOperationAsync()
    {
        // Create a progress object that invokes the UpdateProgress method
        IProgress<float> progressAdb = new Progress<float>(UpdateProgress);

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
                ThresholdOperation();
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
    private async Task ResettingDownloadApkOperationAsync() 
    {
        this.timerReconnect.IsVisible = false;
        var totalApp = AdbProgressMessages.Where(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.Count; //7   

        var files = Directory.GetFiles(_options.ProjectOperationPath);

        foreach(var record in apkRecords){
            var file = files.FirstOrDefault(x => x.Contains(record.Name));
            if(file != null) {
                await CheckApkFileSizeAsync(record);
            }

        }

        var lastInstalled = apkRecords.LastOrDefault(t => t.IsDownloaded);
        //chage AdProgressMessages value for last installed app
        var installedApp = apkRecords.Count(t => t.IsDownloaded); //2

        if (installedApp == 0) return;

        index = 0;
        indexHmsInfo = 0;

        //var realValue = thresholds[installedApp - 1] / 100.0;

        // Update the global progress and the progress bar value
        globalProgress = 0;
        this.progressBar.Progress = 0;
    }

    #region Common
    private void ThresholdOperation()
    {
        var keys = AdbProgressMessages.Where(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
        var count = keys.Count;
        var factor = 100 / count;
        thresholds = keys.Select(x => keys.IndexOf(x) * factor + factor).ToArray();
        thresholds[^1] = Math.Max(thresholds[^1], 101);
        // all keys add in with false value checkHmsOperation
        checkHmsOperation =  keys.ToDictionary(x => x, x => false);
    }
    private void UpdateProgressBar(float value, Queue<double> queueProgress)
    {
        var realValue = Math.Round(value / thresholds.Length, 3);
        if (queueProgress.Count != 0)
        {
            if (queueProgress.Any(t => t <= realValue))
                realValue -= queueProgress.Dequeue();
            else
            {
                queueProgress.Dequeue();
                realValue = 0;
            }

        }
        // If the progress exceeds the current threshold, increment the message index
        globalProgress += realValue;
        Dispatcher.Dispatch(new Action(() =>
        {
            this.progressBar.Progress = globalProgress;
        }));

        if (thresholds.Length == index) return;

        if (globalProgress >= thresholds[index] / 100f)
        {
            index++;
        }
        if (thresholdsHmsInfo.Length == indexHmsInfo) return;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
        if (globalProgress >= thresholdsHmsInfo[indexHmsInfo] / 100f)
        {
            indexHmsInfo++;
        }
        queueProgress.Enqueue(Math.Round(value / thresholds.Length, 3));
        
    }
    private void UpdateCommentLabel()
    {
        // Update the message
        // Update the comment label text based on the current progress and message index
        var adbProgressMsg = AdbProgressMessages.Where(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
        if (adbProgressMsg.Count > index)
        {
            commentBuilder.Clear();
            var value = adbProgressMsg[index];
            if(value == AdbMessagesConst.DownloadingGame || value == AdbMessagesConst.InstallingGame)
            {
                value += ": " + GameName;
            }
            commentBuilder.Append(value);
            Dispatcher.Dispatch(delegate
            {
                this.commentLabel.Text = commentBuilder.ToString();
            });
        }
    }
    private void UpdateHMSInfoLabel()
    {

        hmsInfoBuilder.Clear();
        hmsInfoBuilder.Append(hmsInfoMessage[indexHmsInfo >= hmsInfoBuilder.Length ? (indexHmsInfo != 0 ? indexHmsInfo - 1 : indexHmsInfo) : indexHmsInfo]);
        Dispatcher.Dispatch(delegate
        {
            this.HMSInfoLabel.Text = hmsInfoBuilder.ToString();
        });
    }
    private async Task FakeProgressAsync()
    {
        await Task.Run(() =>
        {
            var queueProgress = new Queue<double>();
            for (int i = 0; i < 100; i++)
            {
                float value = Convert.ToSingle(i) / 100;
                UpdateProgressBar(value, queueProgress);
                UpdateCommentLabel();
                UpdateHMSInfoLabel();
                Thread.Sleep(20);
            }
        });
    }
    private async ValueTask<bool> CheckFileSize(FileInfo fileInfo, string url)
    {
        long fileSize = fileInfo.Length;
        var downloadFileSize = await HttpClientExtensions.GetFileSizeAsync(_httpClient, url);
        return fileSize == downloadFileSize;
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
    private async Task AlertForNotHaveAdbDevices()
    {
        await Task.Delay(1000);
        var errorLanguage = _localizationResourceManager.GetValue("error");
        var emulatorConnectLanguage = _localizationResourceManager.GetValue("emulator_try_again_connect");
        var cancelLanguage = _localizationResourceManager.GetValue("cancel");
        var retryLanguage = _localizationResourceManager.GetValue("retry");

        var result = await DisplayAlert(errorLanguage, emulatorConnectLanguage, retryLanguage, cancelLanguage, FlowDirection.LeftToRight);

        if (result)
        {
            ErrorWhenDownloadingApk = false;
            await DownloadAndInstallOperationAsync();
        }
        else
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
    private async Task AlertForNotHaveInternetAsync(string message)
    {
        await Task.Delay(1000);
        var errorLanguage = _localizationResourceManager.GetValue("error");
        var emulatorConnectLanguage = message;
        var cancelLanguage = _localizationResourceManager.GetValue("cancel");
        var retryLanguage = _localizationResourceManager.GetValue("retry");

        var result = await DisplayAlert(errorLanguage, emulatorConnectLanguage, retryLanguage, cancelLanguage, FlowDirection.LeftToRight);

        if (result)
        {
            var checkInternet = await NetworkUtils.CheckForInternetConnectionAsync();
            var checkHuaweiService = await _appGalleryService.CheckAppGalleryCloudServiceAsync();
            if (!(checkInternet && checkHuaweiService))
            {
                var msg = checkInternet && !checkHuaweiService ? _localizationResourceManager.GetValue("huawei_server_unreachable") : _localizationResourceManager.GetValue("internet_connection_error");
                await AlertForNotHaveInternetAsync(msg);
                return;
            }
            cancellationTokenSrc?.Cancel();
            await ResettingDownloadApkOperationAsync();
            await DownloadandInstallHmsAppsAndNavigateAsync(adbDevices.First());
        }
        else
        {
            cancellationTokenSrc?.Cancel();
            await Navigation.PopAsync();
        }
    }
    private void CheckInternetAccess(object sender, bool result)
    {
        if (!result)
        {
            cancellationTokenSrc?.Cancel();
            ErrorWhenDownloadingApk = true;
            _ = Dispatcher.DispatchAsync(async () =>
            {
                await AlertForNotHaveInternetAsync(_localizationResourceManager.GetValue("internet_connection_error"));
            });
        }
    }
    #endregion

    #region DownloadHMSApps
    ///TODO If lost internet connection when download apk, try again connecting it automaticaLly one time but can't connect show an error popup.
    private async ValueTask<bool> DownloadHMSAppsAsync()
    {
        Directory.CreateDirectory(_options.ProjectOperationPath);
        apkRecords = new List<InstallApkModel>
        {
            new($"{nameof(HmsCore)}.apk", HmsCore, AdbMessagesConst.DownloadingHMSCore),
            new($"{nameof(AppGallery)}.apk", AppGallery, AdbMessagesConst.DownloadingHMSAppGallery),
            new($"{GameItem.AppId}.apk", InstallGame, AdbMessagesConst.DownloadingGame)
        };
        var queueProgress = new Queue<double>();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,// or any other value

        };

        try
        {
            await Parallel.ForEachAsync(apkRecords, parallelOptions, async (model, i) =>
            {
                if (ErrorWhenDownloadingApk) return;
                await CheckApkFileSizeAsync(model);
            });
        }
        catch (Exception ex)
        {
            throw;
        }


        ThresholdOperation();

        var apkRecordNotInstalled = apkRecords.Where(t => !t.IsDownloaded).ToList();

        foreach (var model in apkRecordNotInstalled)
        {
            if (ErrorWhenDownloadingApk) break;
            //CheckApkFileExist(model);
            var progressHMSApps = ProgressBarOperation(queueProgress);
            await DownloadApkAsync(model.DownloadUrl, model.Name, progressHMSApps);
            model.IsDownloaded = true;
        }

        return !ErrorWhenDownloadingApk;
    }
    private async Task CheckApkFileSizeAsync(InstallApkModel model)
    {
        var filePath = Path.Combine(_options.ProjectOperationPath, model.Name);
        var exist = File.Exists(filePath);
        if (exist)
        {
            FileInfo fileInfo = new(filePath);
            var checkFileSize = await CheckFileSize(fileInfo, model.DownloadUrl);
            if (checkFileSize)
            {
                model.IsDownloaded = true;
                AdbProgressMessages[model.Description] = false;
            }
            else
            {
                File.Delete(filePath);
            }
        }
    }
    private void CheckApkFileExist(InstallApkModel model)
    {
        var filePath = Path.Combine(_options.ProjectOperationPath, model.Name);
        var exist = File.Exists(filePath);
        if (exist)
        {
            AdbProgressMessages[model.Description] = false;
            ThresholdOperation();
        }
    }
    private IProgress<float> ProgressBarOperation(Queue<double> queueProgress)
    {
        return new Progress<float>(value =>
        {
            UpdateProgressBar(value, queueProgress);
            UpdateCommentLabel();
            UpdateHMSInfoLabel();
        });
    }
    private async Task DownloadApkAsync(string url, string fileName, IProgress<float> progress)
    {
        cancellationTokenSrc = new();
        try
        {
           await _adbOperationService.DownloadApkFromInternetAsync(url, fileName, progress, cancellationTokenSrc.Token);

        }catch(TaskCanceledException ex) {
            // Check if the cancellation was requested by the token
            if (ex.CancellationToken == cancellationTokenSrc.Token)
            {
                Debug.WriteLine("The request was canceled by the user.");
            }
            else
            {
                Debug.WriteLine("The request was canceled due to a timeout.");
            }
            ErrorWhenDownloadingApk = true;
        }
    }
    #endregion
	
    #region InstallHMSApps
    ///TODO If losted device connection, try again connect same device automaticly one time but if still don't have connection show an error popup.
    private async Task InstallHmsAppsAsync(DeviceData device)
    {
        //Apk Paths
        var apkPaths = new string[] { $"{nameof(HmsCore)}.apk", $"{nameof(AppGallery)}.apk", $"{GameItem.AppId}.apk" };
        var queueProgress = new Queue<double>();
        //Progress
        for (int i = 0; i < apkPaths.Length; i++)
        {
            ProgressHandler progressHMSApps = (o,v) => {
				float value = Convert.ToSingle(v)/100;
				UpdateProgressBar(value, queueProgress);
				UpdateCommentLabel();
				UpdateHMSInfoLabel();
			};
            //var p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), apkPaths[i]);
            apkPaths[i] = Path.Combine(_options.ProjectOperationPath, apkPaths[i]);
            await InstallApkToDeviceAsync(apkPaths[i], progressHMSApps, device);
        }

    }
    private async Task InstallApkToDeviceAsync(string apkPath, ProgressHandler InstallProgressChanged, DeviceData device)
    {
        await Task.Run(() =>
        {
            _adbOperationService.InstallApkToDevice(apkPath, InstallProgressChanged, device);
        });

     }
    #endregion
    
    private void ButtonCancel_Clicked(object sender, EventArgs e)
    {
        //Cancel the operation this page
        _ = Navigation.PopAsync();
    }
    private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        if (Shell.Current.CurrentPage is not DownloadandInstallPage) return;

        if (apkRecords.All(t => t.IsDownloaded)) return;

        if (e.NetworkAccess == NetworkAccess.Internet)
        {
            Console.WriteLine(e.ToString());
            this.timerReconnect.IsVisible = false;
            ErrorWhenDownloadingApk = false;
        }
        else
        {
            var timerReconnect = 16;
            this.timerReconnect.IsVisible = true;
            this.commentLabel.Text = _localizationResourceManager.GetValue("internet_error_reconnect");
            var worker = new Worker<bool>();
            worker.WorkCompleted += CheckInternetAccess;
            Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                timerReconnect--;
                this.timerReconnect.Text = $"({timerReconnect})";
                if (timerReconnect == 0)
                {
                    _ = worker.DoWorkAsync(async () => await NetworkUtils.CheckForInternetConnectionAsync());
                    return false;
                }
                return true;
            });

            Console.WriteLine($"Connectivity: {e.NetworkAccess}");
        }
    }
}

