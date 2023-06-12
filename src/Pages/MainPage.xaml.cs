using HuaweiHMSInstaller.Helper;
using HuaweiHMSInstaller.Models;
using HuaweiHMSInstaller.Pages;
using HuaweiHMSInstaller.Services;
using LocalizationResourceManager.Maui;
using Microsoft.Extensions.Options;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Networking;
using Syncfusion.Maui.Popup;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Path = Microsoft.Maui.Controls.Shapes.Path;
using ServiceProvider = HuaweiHMSInstaller.Services.ServiceProvider;

namespace HuaweiHMSInstaller;
public partial class MainPage : ContentPage
{
    private SfPopup _sfPopup;
    private Button footerButton = new ();
    private readonly IAppGalleryService _appGalleryService;
    private ObservableCollection<SearchListItem> FilteredItems = new();

    private SearchListItem SelectedItem { get; set; }
    private Frame SearchListFrame;
    private readonly ILocalizationResourceManager _localizationResourceManager;
    private readonly GlobalOptions _options;
    public MainPage()
    {
        Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

        _appGalleryService = ServiceProvider.GetService<IAppGalleryService>();
        _localizationResourceManager = ServiceProvider.GetService<ILocalizationResourceManager>();
        _options = ServiceProvider.GetService<IOptions<GlobalOptions>>().Value;
        InitializeComponent();
        Init();
    }
    private void Init()
    {
        this.langPicker.SelectedItem = _localizationResourceManager.CurrentCulture.TwoLetterISOLanguageName.ToUpper();
        this.searchBar.BackgroundColor = Color.FromRgba(255, 255, 255, 0.1);
        this.SearchListFrameGrid.BackgroundColor = Color.FromRgba(255, 255, 255, 0.05);
        this.VersionNum.Text = $"{_localizationResourceManager.GetValue("version")}: {_options.VersionNumber}";

        CheckInternetAndAppGalleryService(AfterEventCheckInternetAndHuaweiServiceInit);
    }
    #region Internet&Huawei server check operation
    private bool CheckInternetConnectionInit()
    {

        var current = Connectivity.NetworkAccess;
        if (current == NetworkAccess.Internet) return true;

        return false;
    }
    private void CheckHuaweiService(Worker<bool>.WorkCompletedEventHandler func) =>
        CheckService(func, _appGalleryService.CheckAppGalleryServiceAsync);
    private void CheckHuaweiCloudService(Worker<bool>.WorkCompletedEventHandler func) =>
        CheckService(func, _appGalleryService.CheckAppGalleryCloudServiceAsync);
    private void CheckService(Worker<bool>.WorkCompletedEventHandler func, Func<Task<bool>> check)
    {
        var worker = new Worker<bool>();
        worker.WorkCompleted += func;
        _ = worker.DoWorkAsync(async () => await check());
    }
    private void CheckInternetAndAppGalleryService(Worker<bool>.WorkCompletedEventHandler func)
    {
        var internetState = CheckInternetConnectionInit();
        if (internetState)
        {
            CheckHuaweiService(func);
        }
        else
        {
            Dispatcher.StartTimer(TimeSpan.FromSeconds(2), () =>
            {
                this.sponsorGameLoader.IsVisible = false;
                this.sponsorGameNotInternetorHuaweiService.IsVisible = true;
                this.sponsorGameNotInternetorHuaweiServiceLabel.Text = _localizationResourceManager.GetValue("internet_connection_error");
                this.sponsorGameStackLayout.IsVisible = false;
                InstallButtonEnable(false);
                return false;
            });
        }
    }
    private void AfterEventCheckInternetAndHuaweiServiceInit(object sender, bool result)
    {
        if (result)
        {
            Dispatcher.StartTimer(TimeSpan.FromSeconds(2), () =>
            {
                _ = GetSponsorGameInfo();
                return false;
            });

        }
        else
        {
            CheckHuaweiService(SposorGameCheck);
        }
    }
    private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        if(e.NetworkAccess == NetworkAccess.Internet)
        {
            this.sponsorGameNotInternetorHuaweiService.IsVisible = false;
            this.sponsorGameLoader.IsVisible = true;
            Init();
        }
        else
        {
            this.sponsorGameNotInternetorHuaweiService.IsVisible = true;
            this.sponsorGameNotInternetorHuaweiServiceLabel.Text = _localizationResourceManager.GetValue("internet_connection_error");
            this.sponsorGameStackLayout.IsVisible = false;
            InstallButtonEnable(false);
        }
    }
    private void SposorGameCheck(object sender, bool result)
    {
        if (!result)
        {
            Dispatcher.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                this.sponsorGameLoader.IsVisible = false;
                this.sponsorGameNotInternetorHuaweiService.IsVisible = true;
                this.sponsorGameNotInternetorHuaweiServiceLabel.Text = _localizationResourceManager.GetValue("huawei_server_unreachable");
                InstallButtonEnable(false);
                return false;
            });
        }

    }
    private void Button_Retry_Clicked(object sender, EventArgs e)
    {
        this.sponsorGameNotInternetorHuaweiService.IsVisible = false;
        this.sponsorGameLoader.IsVisible = true;
        Init();
    }
    #endregion

    private async Task GetSponsorGameInfo()
    {
        var gameAppId = _options.SponsorGameAppId;
        if(gameAppId != null)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
            var result = await _appGalleryService.GetAppDetail(gameAppId);
            var selectedGame = new SearchListItem();

            var imgSource = new UriImageSource
            {
                Uri = new Uri(result.layoutData.FirstOrDefault()?.dataList.FirstOrDefault()?.icoUri),
                CacheValidity = new TimeSpan(10, 0, 0, 0)
            };
            selectedGame.ImageUrl = imgSource.Uri.ToString();
            selectedGame.AppId = gameAppId;
            this.selectedGamePicture.Source = imgSource;
            selectedGame.Name = this.selectedGameName.Text = ti.ToTitleCase(result.layoutData.FirstOrDefault()?.dataList.FirstOrDefault()?.name);
            this.selectedGameSize.Text = $"{_localizationResourceManager.GetValue("size")}: {result.layoutData[1]?.dataList.FirstOrDefault()?.sizeDesc}";
            this.selectedGameDownloadNum.Text = result.layoutData.FirstOrDefault()?.dataList.FirstOrDefault()?.intro;
            this.sponsorGameShortDescription.Text = result.layoutData[1]?.dataList.FirstOrDefault()?.editorDescribe;
            var gameStar = result.layoutData.FirstOrDefault()?.dataList.FirstOrDefault()?.starDesc;


            this.sponsorGameLoader.IsVisible = false;
            this.sponsorGameStackLayout.IsVisible = true;
            SelectedItem = selectedGame;
            InstallButtonEnable(true);
        }
    }
    private async void OnInstallButtonClicked(object sender, EventArgs e)
    {
       await CreateCheckingInternetConnectionPopup();
    }
    private void OnButtonClicked(object sender, EventArgs e)
    {
        _sfPopup.IsOpen = false;
        _sfPopup.Dismiss();
       _ = CreateCheckingInternetConnectionPopup(); 
    }
    private async Task CreateCheckingInternetConnectionPopup()
    {
        //Initialize the popup
        var popup = new SfPopup();
        var grid = new Grid();

        _sfPopup = popup;

        PopupOperation(ref popup);

        CheckAndWrongMarkCreation(out Path checkMark, out Path wrongMark);

        CreateCircle(out Ellipse circle);

        // Create an animation that scales the circle and rotates the check mark
        var animationMark = CreateAnimationMark(checkMark);

        // Use a stack layout to arrange multiple objects
        var stackLayout = new StackLayout
        {
            Orientation = StackOrientation.Vertical,
            Spacing = 10
        };

        // Add an image as the first object
        var image = new Image
        {
            Source = "logo.png",
            HorizontalOptions = LayoutOptions.Center
        };
        stackLayout.Children.Add(image);

        //Create a label for the popup content
        var label = CreateLabel(_localizationResourceManager.GetValue("check_internet_connection"));

        stackLayout.Children.Add(label);

        var animationCircle = CreateAnimationCircle(circle);
        stackLayout.Children.Add(circle);

        stackLayout.Children.Add(grid);

        //get popup size

        var resultCheckingInternetConnection = await NetworkUtils.CheckForInternetConnectionAsync();

        void NotInternetAndHuweiServiceState(string langKey)
        {
            label.FontSize = 19;
            label.Margin = new Thickness(0, 0, 0, 10);
            label.Text = _localizationResourceManager.GetValue(langKey);
            stackLayout.Children.Remove(circle);
            grid.Children.Add(wrongMark);
            // Add a button as the third object
            popup.ShowFooter = true;
            footerButton.Clicked += OnButtonClicked;
        }

        void CheckHuaweiCloudServicekCallBack(object sender, bool result)
        {
            Dispatcher.StartTimer(TimeSpan.FromSeconds(3), () =>
            {
                if (result)
                {
                    animationMark.Commit(this, "ConfirmationAnimation", length: 1000);
                    label.Text = _localizationResourceManager.GetValue("internet_connection_ok");
                    stackLayout.Children.Remove(circle);
                    grid.Children.Add(checkMark);
                    Dispatcher.StartTimer(TimeSpan.FromSeconds(2), () =>
                    {
                        if (!popup.IsOpen) return false;
                        popup.IsOpen = false;
                        popup.Dismiss();
                        stackLayout.Children.Remove(popup);
                        Application.Current.MainPage.Navigation.PushModalAsync(new DownloadandInstallPage(SelectedItem), true);
                        return false;
                    });
                }
                else
                {
                    NotInternetAndHuweiServiceState("huawei_server_unreachable");
                }
                return false;
            });

        }

        if (resultCheckingInternetConnection)
        {
            CheckHuaweiCloudService(CheckHuaweiCloudServicekCallBack);
        }
        else
        {
            NotInternetAndHuweiServiceState("internet_connection_not_ok");
        }

        //Set the label as the popup content
        this.MainContentViewArea.Children.Add(popup);

        popup.ContentTemplate = new DataTemplate(() =>
        {
            return stackLayout;
        });

        popup.Show();

    }
    // Create an animation that scales the circle and rotates the check mark
    private Animation CreateAnimationMark(Path checkMark)
    {
        var animationMark = new Animation
        {
            { 0, 0.5, new (v => checkMark.Scale = v, 1, 1.2) },
            { 0.5, 1, new (v => checkMark.Scale = v, 1.2, 1) },
        };
        return animationMark;
    }
    //Create a label for the popup content
    private Label CreateLabel(string text)
    {
        var label = new Label
        {
            Text = text,
            TextColor = Color.FromArgb("#000000"),
            FontSize = 20,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        //label add ... animation
        var dotAnimationBehavior = new DotAnimationBehavior();
        label.Behaviors.Add(dotAnimationBehavior);
        return label;
    }
    private Animation CreateAnimationCircle(Ellipse circle)
    {
        var animationCircle = new Animation(v => circle.Rotation = v, 0, 360);
        animationCircle.Commit(circle, "CircleAnimation", 16, 1000, Easing.Linear, null, () => true);
        return animationCircle;
    }
    private void CreateCircle(out Ellipse circle)
    {
        // Create a linear gradient brush
        var brush = new LinearGradientBrush
        {
            // Set the start and end points of the gradient axis
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1)
        };

        // Add two gradient stops with blue and green colors
        brush.GradientStops.Add(new GradientStop { Color = Colors.Blue, Offset = 0.0f });
        brush.GradientStops.Add(new GradientStop { Color = Colors.Green, Offset = 1.0f });

        //create gradient color stroke circle
        circle = new Ellipse
        {
            WidthRequest = 100,
            HeightRequest = 100,
            Stroke = brush,
            StrokeThickness = 10,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
    }
    private void CheckAndWrongMarkCreation(out Path checkMark, out Path wrongMark)
    {
        // Create a check mark path shape
        CreatePathLineSegment(out checkMark, 
                                new Point(10, 50), new List<Point> { new Point(40, 80), new Point(90, 20) }, 
                                Colors.Green);
        CreatePathLineSegment(out wrongMark, 
                                new Point(10, 10), new List<Point> { new Point(90, 90), new Point(90, 10), new Point(10, 90) },
                                Colors.Red);

    }
    private void CreatePathLineSegment(out Path path, Point startPoint, List<Point> points, Color color)
    {
        PathSegmentCollection pathSegmentCollection = new PathSegmentCollection();
        foreach (var point in points)
        {
            pathSegmentCollection.Add(new LineSegment { Point = point });
        }
        var pathFigure = new PathFigure { StartPoint = startPoint, Segments = pathSegmentCollection };
        var pathGeometry = new PathGeometry { Figures = new PathFigureCollection { pathFigure } };
        path = new Path
        {
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2,
            Data = pathGeometry,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            WidthRequest = 100,
            HeightRequest = 100
        };
    }
    private void PopupOperation(ref SfPopup popup){
        popup.AutoSizeMode = PopupAutoSizeMode.Height;
        popup.OverlayMode = PopupOverlayMode.Transparent;
        popup.PopupStyle = new PopupStyle
        {
            HeaderTextAlignment = TextAlignment.Center,
            Stroke = Colors.Gray,
            StrokeThickness = 1,
            MessageBackground = Colors.White,
            FooterBackground = Colors.White,
            CornerRadius = 10,
            BlurIntensity = PopupBlurIntensity.ExtraDark,
            HasShadow = true
        };
        popup.HeaderTemplate = new DataTemplate(() =>
        {
            var headerLabel = new Label();
            headerLabel.Text =_localizationResourceManager.GetValue("check_internet");
            headerLabel.MaxLines = 2;
            headerLabel.FontFamily = "Arial";
            headerLabel.FontSize = 20;
            headerLabel.FontAttributes = FontAttributes.Bold;
            headerLabel.TextColor = Color.FromArgb("#ed1c24");
            headerLabel.HorizontalOptions = LayoutOptions.Center;
            headerLabel.VerticalOptions = LayoutOptions.Center;

            return headerLabel;
        });
        popup.FooterTemplate = new DataTemplate(() =>
        {
            footerButton = new Button
            {
                Text = _localizationResourceManager.GetValue("retry"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Color.FromArgb("#ed1c24"),
                TextColor = Color.FromArgb("#ffffff"),
                CornerRadius = 10,
                WidthRequest = 150,
                HeightRequest = 50,
                Margin = new Thickness(0, 0, 0, 10)
            };
            return footerButton;
        });
    }
    private async void SearchBarPressedAsync(object sender, EventArgs e)
    {
        if (SearchListFrame != null)
        {
            SearchListFrame.IsVisible = false;
        }
        // Get the search query from the SearchBar
        var query = searchBar.Text;

        selectedItemLabel.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(query))
        {
            // Hide the ListView when the search query is null or empty
            InstallButtonEnable(false);
            var label = new Label();
            label.Margin = new Thickness(10);
            label.HorizontalOptions = LayoutOptions.Center;
            label.VerticalOptions = LayoutOptions.Center;
            label.Text = $"{_localizationResourceManager.GetValue("please_input_seach_keyword")}!!!";
            searchBarStackLayout.Children.Add(label);

            Dispatcher.StartTimer(TimeSpan.FromSeconds(3), () =>
            {
                searchBarStackLayout.Children.Remove(label);
                return false;
            });

            return;
        }

        // Clear the filtered items collection
        FilteredItems.Clear();

        SearchLoader.IsVisible = true;

        var searchResult = await _appGalleryService.SearchAppGalleryApp(query);

        var items = searchResult.layoutData.SelectMany(t => t.dataList.Select(x =>
                                                            new SearchListItem
                                                            {
                                                                Name = x.name,
                                                                ImageUrl = x.icon,
                                                                AppId = x.appid
                                                            })).ToList();

        // Filter the items by the query and add them to the filtered items collection
        foreach (var item in items)
        {
            FilteredItems.Add(item);
        }

        if(FilteredItems.Count > 1)
            CreateListView();
        else
        {
            SearchLoader.IsVisible = false;
            // Hide the ListView when the search query is null or empty
            InstallButtonEnable(false);
            var label = new Label();
            label.TextColor = Colors.WhiteSmoke;
            label.Margin = new Thickness(10);
            label.HorizontalOptions = LayoutOptions.Center;
            label.VerticalOptions = LayoutOptions.Center;
            label.Text = $"{_localizationResourceManager.GetValue("no_search_result_found")}!!!";
            searchBarStackLayout.Children.Add(label);

            Dispatcher.StartTimer(TimeSpan.FromSeconds(3), () =>
            {
                searchBarStackLayout.Children.Remove(label);
                return false;
            });

            return;
            
        }


    }
    private void CreateListView()
    {
        SearchListFrameGrid.Children.Clear();

        SearchListFrame = new Frame
        {
            BorderColor = Colors.White,
            CornerRadius = 10,
            BackgroundColor = Colors.Transparent,
            IsVisible = false,// Hide the Frame initially
            Margin = new Thickness(0, 10, 0, 0)
        };

        // Create a ListView to display the filtered items
        var listView = new ListView
        {
            ItemsSource = FilteredItems,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Default,
            MaximumHeightRequest = 200,
            MinimumHeightRequest = 200,

        };

        // Handle the ItemTapped event to update the Label text
        listView.ItemTapped += (sender, e) =>
        {

            // Get the tapped item from the ItemTappedEventArgs
            var item = e.Item as SearchListItem;

            // Set the Label text to the tapped item
            selectedItemLabel.Text = $"{_localizationResourceManager.GetValue("you_selected")}: {item.Name}";

            SelectedItem = item;

            if(selectedItemLabel != null )
            {
                InstallButtonEnable(true);

                var animationMark = new Animation
                {
                    { 0, 0.5, new (v => ButtonInstall.Scale = v, 1, 1.1) },
                    { 0.5, 1, new (v => ButtonInstall.Scale = v, 1.1, 1) },
                };

                animationMark.Commit(this, "ConfirmationAnimation", length: 1000);
            }
        };

        var dataTemplate = new DataTemplate(() =>
        {
            // Create an Image object and bind its Source property to the ImageUrl property of the data model
            var image = new Image();
            image.MaximumWidthRequest = 35;
            image.Margin = new Thickness(5,5,10,5);
            image.SetBinding(Image.SourceProperty, "ImageUrl");

            // Create a Label object and bind its Text property to the Name property of the data model
            var label = new Label();
            label.VerticalOptions = LayoutOptions.Center;
            label.HorizontalOptions = LayoutOptions.Center;
            label.TextColor = Colors.White;
            label.Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Colors.Black),
                Opacity = 1f,
                Offset = new Point(25, 25),
                Radius =20
            };
            label.SetBinding(Label.TextProperty, "Name");

            // Create a StackLayout object to arrange the image and label horizontally
            var stackLayoutListItem = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children =
                        {
                            image,
                            label
                        },
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                MinimumWidthRequest = 300
            };

            // Return a ViewCell object that contains the StackLayout
            return new ViewCell { View = stackLayoutListItem };
        });
        
       listView.ItemTemplate = dataTemplate;

        // Show the ListView with the filtered items

        Dispatcher.StartTimer(TimeSpan.FromSeconds(3), () =>
        {
            SearchLoader.IsVisible = false;
            SearchListFrame.Content = listView;
            SearchListFrame.IsVisible = true;
            SearchListFrameGrid.Add(SearchListFrame);
            return false;
        });

    }
    public void InstallButtonEnable(bool enable)
    {
        ButtonInstall.IsEnabled = enable;
        ButtonInstall.Opacity = enable ? 1 : 0.5;
    }
    public void SearchBar_TextChanged(object sender, TextChangedEventArgs e){

        if(string.IsNullOrWhiteSpace(e.NewTextValue) && SearchListFrame != null){
            SearchListFrame.IsVisible = false;
        }
    }
    private void langPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = sender as Picker;

        _localizationResourceManager.CurrentCulture = new CultureInfo(picker.SelectedItem.ToString());

        this.VersionNum.Text = $"{_localizationResourceManager.GetValue("version")}: {_options.VersionNumber}";

        this.sponsorGameStackLayout.IsVisible = false ;
        this.sponsorGameNotInternetorHuaweiService.IsVisible = false;
        this.sponsorGameLoader.IsVisible = true ;

        Init();

    }
    private void Button_ChangeGame_Clicked(object sender, EventArgs e)
    {
        this.selectedGameFrame.IsVisible = false;
        this.searchBar.IsVisible = true;
        InstallButtonEnable(false);
    }
}


