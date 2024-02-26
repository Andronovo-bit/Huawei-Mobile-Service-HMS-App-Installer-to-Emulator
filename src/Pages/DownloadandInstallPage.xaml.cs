using AdvancedSharpAdbClient;
using HuaweiHMSInstaller.Helper;
using HuaweiHMSInstaller.Integrations.Analytics;
using HuaweiHMSInstaller.Models;
using HuaweiHMSInstaller.Services;
using HuaweiHMSInstaller.ViewModels;
using LocalizationResourceManager.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using static AdvancedSharpAdbClient.DeviceCommands.PackageManager;
using ServiceProvider = HuaweiHMSInstaller.Services.ServiceProvider;

namespace HuaweiHMSInstaller.Pages;

public partial class DownloadandInstallPage : ContentPage, IQueryAttributable
{
    public SearchListItem SearchListItem { get; set; }

    #region Definations
    // Define an array of messages for each progress range
    private Dictionary<string, bool> AdbProgressMessages;
    private string[] hmsInfoMessage;
    // Define an array of progress thresholds for each message
    private int[] thresholds;
    private readonly double[] thresholdsHmsInfo = new double[] { 20, 40, 60, 70, 80 };
    private List<InstallApkModel> apkRecords;
    private Dictionary<string, bool> checkHmsOperation;

    // Initialize variables to keep track of the current message index
    private int currentThresholdIndex = 0;
    private int currentHmsInfoIndex = 0;
    private double globalProgress = 0;
    // Use StringBuilder for string concatenation
    private StringBuilder commentBuilder;
    private StringBuilder hmsInfoBuilder;

    //HMS Links
    private const string AppGallery = "https://appgallery.cloud.huawei.com/appdl/C27162?";
    private const string HmsCore = "https://appgallery.cloud.huawei.com/appdl/C10132067?";

    private string InstallGameUrl;
    private string GameName;
    private SearchListItem GameItem;

    private CancellationTokenSource cancellationTokenSrc;
    private bool ErrorWhenDownloadingApk;
    private List<DeviceData> adbDevices = Enumerable.Empty<DeviceData>().ToList();

    #endregion

    private readonly IAdbOperationService _adbOperationService;
    private readonly IAppGalleryService _appGalleryService;
    private readonly AppSettings _settings;
    private readonly ILocalizationResourceManager _localizationResourceManager;
    private readonly DownloadAndInstallPageViewModel _viewModel;
    private readonly AnalyticsSubject _analyticsSubject;



    public DownloadandInstallPage(
        DownloadAndInstallPageViewModel viewModel,
        AnalyticsSubject analyticsSubject,
        IConfiguration configuration)
    {
        Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

        InitializeComponent();
        _adbOperationService = ServiceProvider.GetService<IAdbOperationService>();
        _appGalleryService = ServiceProvider.GetService<IAppGalleryService>();
        _localizationResourceManager = ServiceProvider.GetService<ILocalizationResourceManager>();
        _settings = configuration.GetSection("Settings").Get<AppSettings>();
        _analyticsSubject = analyticsSubject;
        _viewModel = viewModel;
        this.NavigatedTo += (s, e) => Initialize();

    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("SearchListItem"))
        {
            SearchListItem = query["SearchListItem"] as SearchListItem;
        }
        OnPropertyChanged(nameof(SearchListItem));
    }
    private void Initialize()
    {
        // Assign the game item
        GameItem = SearchListItem;

        // Initialize variables
        ErrorWhenDownloadingApk = false;
        InstallGameUrl = $"https://appgallery.cloud.huawei.com/appdl/{GameItem.AppId}?";
        GameName = GameItem.Name;

        // Set up progress messages and handlers
        AdbProgressMessages = _viewModel.AdbProgressMessages;
        hmsInfoMessage = HmsInfoMessagesConst.InitializeMessages();
        _viewModel.OnProgressChanged = UpdateProgress;
        _viewModel.OnThresholdReached = PerformThresholdOperation;

        // Initialize progress-related variables
        thresholds = null;
        currentThresholdIndex = 0;
        currentHmsInfoIndex = 0;
        globalProgress = 0;
        this.progressBar.Progress = globalProgress;

        // Initialize builders and collections
        commentBuilder = new StringBuilder();
        hmsInfoBuilder = new StringBuilder();
        apkRecords = new List<InstallApkModel>();
        checkHmsOperation = new Dictionary<string, bool>();

        // Perform the initial threshold operation
        PerformThresholdOperation();

        // Start the download and install operation asynchronously
        Dispatcher.DispatchAsync(async () =>
        {
            await DownloadAndInstallOperationAsync();
        });
    }
    private async Task DownloadAndInstallOperationAsync()
    {

        //label add ... animation use behavior
        this.dotAnimation.Behaviors.Add(new DotAnimationBehavior());
        this.commentLabel.Text = _localizationResourceManager.GetValue("emulator_config_progress");

        var adbDevices = await _viewModel.AdbServerandDeviceCheckAsync();


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
                foreach (var device in adbDevices)
                {
                    await DownloadandInstallHmsAppsAndNavigateAsync(device);
                }
            }
            else
            {
                var msg = checkInternet && !checkHuaweiService ? _localizationResourceManager.GetValue("huawei_server_unreachable") : _localizationResourceManager.GetValue("internet_connection_error");
                await _analyticsSubject.NotifyAsync(msg);
                await AlertForNotHaveInternetAsync(msg);
            }
        }
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
                _viewModel.NavigateToThanksPage();
            }
        }
        catch (Exception ex)
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
        if (globalProgress >= thresholds[currentThresholdIndex] / 100f)
        {
            currentThresholdIndex++;
        }
        if (globalProgress >= thresholdsHmsInfo[currentHmsInfoIndex] / 100f)
        {
            currentHmsInfoIndex++;
        }

        // Update the comment label text based on the current message index
        commentBuilder.Clear();
        commentBuilder.Append(AdbProgressMessages.ElementAt(currentThresholdIndex).Key);
        this.commentLabel.Text = commentBuilder.ToString();

        // Update the HMS info label text based on the current message index
        hmsInfoBuilder.Clear();
        hmsInfoBuilder.Append(hmsInfoMessage[currentHmsInfoIndex]);
        this.HMSInfoLabel.Text = hmsInfoBuilder.ToString();
    }

    private async Task ResettingDownloadApkOperationAsync()
    {
        this.timerReconnect.IsVisible = false;
        var totalApp = AdbProgressMessages.Where(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.Count; //7   

        var files = Directory.GetFiles(_settings.ProjectOperationPath);

        foreach (var record in apkRecords)
        {
            var file = files.FirstOrDefault(x => x.Contains(record.Name));
            if (file != null)
            {
                await CheckApkFileSizeAsync(record);
            }

        }

        var lastInstalled = apkRecords.LastOrDefault(t => t.IsDownloaded);
        //chage AdProgressMessages value for last installed app
        var installedApp = apkRecords.Count(t => t.IsDownloaded); //2

        if (installedApp == 0) return;

        currentThresholdIndex = 0;
        currentHmsInfoIndex = 0;

        //var realValue = thresholds[installedApp - 1] / 100.0;

        // Update the global progress and the progress bar value
        globalProgress = 0;
        this.progressBar.Progress = 0;
    }

    #region Common
    private void PerformThresholdOperation()
    {
        var keys = AdbProgressMessages.Where(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
        var count = keys.Count;
        var factor = 100 / count;
        thresholds = keys.Select(x => keys.IndexOf(x) * factor + factor).ToArray();
        thresholds[^1] = Math.Max(thresholds[^1], 101);
        // all keys add in with false value checkHmsOperation
        checkHmsOperation = keys.ToDictionary(x => x, x => false);
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

        if (thresholds.Length == currentThresholdIndex) return;

        if (globalProgress >= thresholds[currentThresholdIndex] / 100f)
        {
            currentThresholdIndex++;
        }
        if (thresholdsHmsInfo.Length == currentHmsInfoIndex) return;
        if (globalProgress >= thresholdsHmsInfo[currentHmsInfoIndex] / 100f)
        {
            currentHmsInfoIndex++;
        }
        queueProgress.Enqueue(Math.Round(value / thresholds.Length, 3));

    }
    private void UpdateCommentLabel()
    {
        // Update the message
        // Update the comment label text based on the current progress and message index
        var commentString = _viewModel.UpdateCommentLabel(AdbProgressMessages, currentThresholdIndex, GameName);
        Dispatcher.Dispatch(delegate
        {
            this.commentLabel.Text = commentString;
        });
    }
    private void UpdateHMSInfoLabel()
    {
        int index = currentHmsInfoIndex < hmsInfoMessage.Length ? currentHmsInfoIndex : (currentHmsInfoIndex != 0 ? currentHmsInfoIndex - 1 : 0);
        string message = hmsInfoMessage[index];

        Dispatcher.Dispatch(delegate
        {
            this.HMSInfoLabel.Text = message;
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
    private async Task AlertForNotHaveAdbDevices()
    {
        await Task.Delay(1000);
        await _analyticsSubject.NotifyAsync("No Adb Devices");
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
        Directory.CreateDirectory(_settings.ProjectOperationPath);
        apkRecords = new List<InstallApkModel>
        {
            new($"{nameof(HmsCore)}.apk", HmsCore, AdbMessagesConst.DownloadingHMSCore),
            new($"{nameof(AppGallery)}.apk", AppGallery, AdbMessagesConst.DownloadingHMSAppGallery),
            new($"{GameItem.AppId}.apk", InstallGameUrl, AdbMessagesConst.DownloadingGame)
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


        PerformThresholdOperation();

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
        var filePath = Path.Combine(_settings.ProjectOperationPath, model.Name);
        var exist = File.Exists(filePath);
        if (exist)
        {
            FileInfo fileInfo = new(filePath);
            var checkFileSize = await _viewModel.CheckFileSize(fileInfo, model.DownloadUrl);
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
        var filePath = Path.Combine(_settings.ProjectOperationPath, model.Name);
        var exist = File.Exists(filePath);
        if (exist)
        {
            AdbProgressMessages[model.Description] = false;
            PerformThresholdOperation();
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

        }
        catch (TaskCanceledException ex)
        {
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
            ProgressHandler progressHMSApps = (o, v) =>
            {
                float value = Convert.ToSingle(v) / 100;
                UpdateProgressBar(value, queueProgress);
                UpdateCommentLabel();
                UpdateHMSInfoLabel();
            };
            //var p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), apkPaths[i]);
            apkPaths[i] = Path.Combine(_settings.ProjectOperationPath, apkPaths[i]);
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

