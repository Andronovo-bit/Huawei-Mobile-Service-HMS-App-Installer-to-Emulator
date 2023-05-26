using AdvancedSharpAdbClient;
using HuaweiHMSInstaller.Helper;
using HuaweiHMSInstaller.Models;
using HuaweiHMSInstaller.Services;
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
	// Define an array of messages for each progress range
	private readonly Dictionary<string, bool> AdbProgressMessages = AdbMessagesConst.Messages;
	private readonly string[] hmsInfoMessage = HmsInfoMessagesConst.hmsInfoMessage;
	// Define an array of progress thresholds for each message
	private int[] thresholds;
	private readonly double[] thresholdsHmsInfo = new double[] { 20, 40, 60, 80, 101 };
	// Initialize variables to keep track of the current message index
	private int index = 0;
	private int indexHmsInfo = 0;
	private double globalProgress = 0;
	// Use StringBuilder for string concatenation
	private StringBuilder commentBuilder = new ();
	private StringBuilder hmsInfoBuilder = new ();

	private string adbPath;
	//HMS Links
	private const string AppGallery = "https://appgallery.cloud.huawei.com/appdl/C27162?";
	private const string HmsCore = "https://appgallery.cloud.huawei.com/appdl/C10132067?";
	private const string InstallGame = "https://appgallery.cloud.huawei.com/appdl/C107971673?";

    #endregion

    private readonly IAdbOperationService adbOperationService;
	private readonly GlobalOptions _options;

	public DownloadandInstallPage()
	{
		adbOperationService = ServiceProvider.GetService<IAdbOperationService>();
		_options = ServiceProvider.GetService<IOptions<GlobalOptions>>().Value;

		InitializeComponent();

		ThresholdOperation();

        adbPath = Path.Combine(_options.ProjectOperationPath, AdbFolder, "platform-tools", "adb.exe");

		Dispatcher.DispatchAsync(async () =>
		{
			await DownloadAndInstallOperationAsync();
		});
	}

	private async Task DownloadAndInstallOperationAsync(){

		//label add ... animation use behavior
		this.dotAnimation.Behaviors.Add(new DotAnimationBehavior());

		var adbServerCheck = await AdbServerOperationAsync();

		if (adbServerCheck)
		{
			var adbDevices = await adbOperationService.GetDevices();
			if (adbDevices is null or { Count: 0 })
			{
				await AlertForNotHaveAdbDevices();
				//return false;
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

		var adbServerCheck = adbOperationService.CheckAdbServer();

		if (!adbServerCheck)
		{
			await adbOperationService.DownloadAdbFromInternetAsync(progressAdb);
			var checkDownload = File.Exists(adbPath);
			if (checkDownload)
			{
				var status = AdbServer.Instance.StartServer(adbPath, true);
				if(status == StartServerResult.Started ||status == StartServerResult.AlreadyRunning)
				{
					adbServerCheck = true;
                }
			}

		}
		else
		{
            AdbProgressMessages[AdbMessagesConst.DownloadingADBDriver] = false;
            AdbProgressMessages[AdbMessagesConst.InstallingADBDriver] = false;
            ThresholdOperation();
        }

		return adbServerCheck;

 
	}
	private async Task AlertForNotHaveAdbDevices()
	{
		await Task.Delay(1000);

		var result = await DisplayAlert("Error", "Please connect your device or emulator and try again", "Retry", "Cancel", FlowDirection.LeftToRight);

		if (result)
		{
			await DownloadAndInstallOperationAsync();
		}
		else
		{
			await Application.Current.MainPage.Navigation.PopModalAsync();
		}
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
        if (globalProgress >= thresholds[index] / 100f)
        {
            index++;
        }
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
            commentBuilder.Append(adbProgressMsg[index]);
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
        //var progressValues = new double[] { 0.10, 0.20, 0.30 };
        var apkUrls = new string[] { HmsCore, AppGallery, InstallGame };
        var constantMessage = new string[] { AdbMessagesConst.DownloadingHMSCore, AdbMessagesConst.DownloadingHMSAppGallery, AdbMessagesConst.DownloadingGame };
        var apkNames = new string[] { $"{nameof(HmsCore)}.apk", $"{nameof(AppGallery)}.apk", $"{nameof(InstallGame)}.apk" };
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
        await adbOperationService.DownloadApkFromInternetAsync(url, fileName, progress);
    }
    #endregion
	
    #region InstallHMSApps
    private async Task InstallHmsAppsAsync(DeviceData device)
    {
        //Apk Paths
        var apkPaths = new string[] { $"{nameof(HmsCore)}.apk", $"{nameof(AppGallery)}.apk", $"{nameof(InstallGame)}.apk" };
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
			adbOperationService.InstallApkToDevice(apkPath, InstallProgressChanged, device);
		});

    }
    #endregion


}

