using AdvancedSharpAdbClient;
using HuaweiHMSInstaller.Helper;
using HuaweiHMSInstaller.Models;
using HuaweiHMSInstaller.Services;
using LocalizationResourceManager.Maui;
using Microsoft.Extensions.Options;
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

	private readonly string InstallGame;
    private readonly string GameName;
    private readonly SearchListItem GameItem;


    #endregion

    private readonly IAdbOperationService _adbOperationService;
	private readonly GlobalOptions _options;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    public DownloadandInstallPage(SearchListItem item)
	{
        _adbOperationService = ServiceProvider.GetService<IAdbOperationService>();
        _localizationResourceManager = ServiceProvider.GetService<ILocalizationResourceManager>();
        _options = ServiceProvider.GetService<IOptions<GlobalOptions>>().Value;

        GameItem = item;
        InstallGame = $"https://appgallery.cloud.huawei.com/appdl/{GameItem.AppId}?";
        GameName = GameItem.Name;

        InitializeComponent();
        Init();

    }

    private void Init()
    {

        //this.mainGrid.BackgroundColor = Color.FromRgba(0, 0, 0, 0.50);

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
        this.commentLabel.Text = "Device(Emulator) configuration is in progress";

        var adbServerCheck = await AdbServerOperationAsync();

		if (adbServerCheck)
		{
			var adbDevices = await _adbOperationService.GetDevices();
			if (adbDevices is null or { Count: 0 })
			{
				await AlertForNotHaveAdbDevices();
			}else{
				await DownloadHMSApps();
                await InstallHmsAppsAsync(adbDevices.First());
                await FakeProgressAsync();
                await Task.Delay(2000);
                await Application.Current.MainPage.Navigation.PushModalAsync(new ThanksPage(), true);

            }
		}
	}
	private async Task<bool> AdbServerOperationAsync()
	{
		IProgress<float> progressAdb = new Progress<float>(value =>
		{
			var realValue = (value / thresholds.Length)*2;
			// If the progress exceeds the current threshold, increment the message index

			// Update the progress bar value
			// Get the index of the message that corresponds to the current progress value
			globalProgress = realValue;
			this.progressBar.Progress = realValue;
			if (globalProgress >= thresholds[index] / 100f)
			{
				index++;
			}
			if (globalProgress >= thresholdsHmsInfo[indexHmsInfo] / 100f)
			{
				indexHmsInfo++;
			}
			// Update the message
			// Update the comment label text based on the current progress and message index
			commentBuilder.Clear();
			commentBuilder.Append(AdbProgressMessages.ElementAt(index).Key);
			this.commentLabel.Text = commentBuilder.ToString();

			hmsInfoBuilder.Clear();
			hmsInfoBuilder.Append(hmsInfoMessage[indexHmsInfo]);
			this.HMSInfoLabel.Text = hmsInfoBuilder.ToString();
		});
		var adbServerCheck = _adbOperationService.CheckAdbServer();
        try
        {
            if (!adbServerCheck)
            {
                var hasAdbFolderFile = AdbFolderFileCheckOperation();
                if (!hasAdbFolderFile)
                {
                    await _adbOperationService.DownloadAdbFromInternetAsync(progressAdb);
                    hasAdbFolderFile = AdbFolderFileCheckOperation();
                }
                if (hasAdbFolderFile)
                {
                    var status = AdbServer.Instance.StartServer(adbPath, true);
                    if (status == StartServerResult.Started || status == StartServerResult.AlreadyRunning)
                    {
                        adbServerCheck = true;
                        await _adbOperationService.CreateAdbClient();
                    }
                }
            }
            else
            {
                await _adbOperationService.CreateAdbClient();
                AdbProgressMessages[AdbMessagesConst.DownloadingADBDriver] = false;
                AdbProgressMessages[AdbMessagesConst.InstallingADBDriver] = false;
                ThresholdOperation();
            }
        }
        catch (Exception ex)
        {
            WorkingProcessAndPort.KillProcess("adb");
            await AdbServerOperationAsync();
        }

		return adbServerCheck;
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
			await DownloadAndInstallOperationAsync();
		}
		else
		{
			await Application.Current.MainPage.Navigation.PopModalAsync();
		}
	}
    
    private bool AdbFolderFileCheckOperation()
    {
        var hasAdbFolder = Directory.Exists(adbFolderPath);
        var hasAdbFile = File.Exists(adbPath);

        if (hasAdbFolder && hasAdbFile) {
            var adbFolderFileCount = Directory.GetFiles(Path.Combine(adbFolderPath, "platform-tools"))?.Length; //must be 15
            var adbFileSize = new FileInfo(adbPath).Length;
            if(adbFolderFileCount == 15 && adbFileSize == AdbFileSize) return true;
        }
        return false;

    }
    #region Common
    private void ThresholdOperation()
    {
        var keys = AdbProgressMessages.Where(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToList();
        var count = keys.Count;
        var factor = 100 / count;
        thresholds = keys.Select(x => keys.IndexOf(x) * factor + factor).ToArray();
        thresholds[^1] = Math.Max(thresholds[^1], 101);
    }
    private void UpdateProgressBar(float value, Queue<double> queueProgress)
    {
        var realValue = Math.Round(value / thresholds.Length, 3);
        if(queueProgress.Count != 0)
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
    #endregion

    #region DownloadHMSApps
    private async Task DownloadHMSApps()
    {
        Directory.CreateDirectory(_options.ProjectOperationPath);
        var apkUrls = new string[] { HmsCore, AppGallery, InstallGame };
        var constantMessage = new string[] { AdbMessagesConst.DownloadingHMSCore, AdbMessagesConst.DownloadingHMSAppGallery, AdbMessagesConst.DownloadingGame };
        var apkNames = new string[] { $"{nameof(HmsCore)}.apk", $"{nameof(AppGallery)}.apk", $"{GameItem.AppId}.apk" };
        var queueProgress = new Queue<double>();
        for (int i = 0; i < apkUrls.Length; i++)
        {
			var exist = File.Exists(Path.Combine(_options.ProjectOperationPath, apkNames[i]));
			if (exist)
			{	
				AdbProgressMessages[constantMessage[i]] = false;
				ThresholdOperation();
                continue;
			}

            IProgress<float> progressHMSApps = new Progress<float>(value =>
            {
                if (i < apkUrls.Length)
                {
                    UpdateProgressBar(value,queueProgress);
                    UpdateCommentLabel();
                    UpdateHMSInfoLabel();
                }
            });

            await DownloadApk(apkUrls[i], apkNames[i], progressHMSApps);
        }
    }
    private async Task DownloadApk(string url, string fileName, IProgress<float> progress)
    {
        await _adbOperationService.DownloadApkFromInternetAsync(url, fileName, progress);
    }
    #endregion
	
    #region InstallHMSApps
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
            await InstallApkToDevice(apkPaths[i], progressHMSApps, device);
        }

    }
    private async Task InstallApkToDevice(string apkPath, ProgressHandler InstallProgressChanged, DeviceData device)
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
        Navigation.PopAsync();
    }
}

