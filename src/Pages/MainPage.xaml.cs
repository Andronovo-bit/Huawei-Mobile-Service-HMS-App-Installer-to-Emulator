using CommunityToolkit.Maui.Views;
using HuaweiHMSInstaller.Helper;
using HuaweiHMSInstaller.Models;
using HuaweiHMSInstaller.Pages;
using HuaweiHMSInstaller.Services;
using LocalizationResourceManager.Maui;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Popup;
using System.Collections.ObjectModel;
using Path = Microsoft.Maui.Controls.Shapes.Path;
using ServiceProvider = HuaweiHMSInstaller.Services.ServiceProvider;

namespace HuaweiHMSInstaller;

public partial class MainPage : ContentPage
{
    private SfPopup _sfPopup;
    private Button footerButton = new ();
    //private Label selectedItemLabel = new ();
    private readonly IAppGalleryService _appGalleryService;
    private ObservableCollection<SearchListItem> FilteredItems = new();

    private SearchListItem SelectedItem { get; set; }
    private Frame SearchListFrame;
    private readonly ILocalizationResourceManager _localizationResourceManager;

    public MainPage()
    {
        _appGalleryService = ServiceProvider.GetService<IAppGalleryService>();
        _localizationResourceManager = ServiceProvider.GetService<ILocalizationResourceManager>();
        InitializeComponent();
        Init();
    }

    private void Init()
    {
        this.langPicker.SelectedItem = _localizationResourceManager.CurrentCulture.TwoLetterISOLanguageName.ToUpper();
        this.searchBar.BackgroundColor = Color.FromRgba(255, 255, 255, 0.1);
        this.SearchListFrameGrid.BackgroundColor = Color.FromRgba(255, 255, 255, 0.05);
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

        //var popup2 = new Popup
        //{
        //    Content = new VerticalStackLayout
        //    {
        //        Children =
        //        {
        //            new Label
        //            {
        //                Text = "This is a very important message!"
        //            }
        //        }
        //    }


        //await this.ShowPopupAsync(popup2);

        
        //Initialize the popup
        var popup = new SfPopup();
        var grid = new Grid();

        _sfPopup = popup;

        PopupOperation(ref popup);

        CheckAndWrongMarkCreation(out Path checkMark, out Path wrongMark);

        CreateCircle(out Ellipse circle);

        // Create an animation that scales the circle and rotates the check mark
        var animationMark = new Animation
        {
            { 0, 0.5, new (v => checkMark.Scale = v, 1, 1.2) },
            { 0.5, 1, new (v => checkMark.Scale = v, 1.2, 1) },
        };

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
        var label = new Label
        {
            Text = _localizationResourceManager.GetValue("check_internet_connection"),
            TextColor = Color.FromArgb("#000000"),
            FontSize = 20,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        //label add ... animation
        var dotAnimationBehavior = new DotAnimationBehavior();
        label.Behaviors.Add(dotAnimationBehavior);

        stackLayout.Children.Add(label);

        var animationCircle = new Animation(v => circle.Rotation = v, 0, 360);
        animationCircle.Commit(circle, "CircleAnimation", 16, 1000, Easing.Linear, null, () => true);
        stackLayout.Children.Add(circle);

        stackLayout.Children.Add(grid);

        //get popup size

        var resultCheckingInternetConnection = await CheckInternetConnection.CheckForInternetConnectionAsync();
        
        Dispatcher.StartTimer(TimeSpan.FromSeconds(4), () =>
        {
            animationMark.Commit(this, "ConfirmationAnimation", length: 1000);
            //dotAnimationBehavior.BindingContext = null;
            if (resultCheckingInternetConnection)
            {

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
                return false;
            }
            else
            {
                label.Text = _localizationResourceManager.GetValue("internet_connection_not_ok");
                stackLayout.Children.Remove(circle);
                grid.Children.Add(wrongMark);
                // Add a button as the third object
                popup.ShowFooter = true;
                footerButton.Clicked += OnButtonClicked;

                return false;

            }

        });

        //Set the label as the popup content
        this.MainContentViewArea.Children.Add(popup);

        popup.ContentTemplate = new DataTemplate(() =>
        {
            return stackLayout;
        });
        popup.Show();
        
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
                WidthRequest = 100,
                HeightRequest = 50,
                Margin = new Thickness(0, 0, 0, 10)
            };
            return footerButton;
        });
    }
    private async void SearchBarPressedAsync(object sender, EventArgs e)
    {
        var appGalleryBase64 = "data:image/svg+xml;base64,PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiPz4KPHN2ZyB3aWR0aD0iNDBweCIgaGVpZ2h0PSI0MHB4IiB2aWV3Qm94PSIwIDAgNDAgNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayI+CiAgICA8IS0tIEdlbmVyYXRvcjogU2tldGNoIDY0ICg5MzUzNykgLSBodHRwczovL3NrZXRjaC5jb20gLS0+CiAgICA8dGl0bGU+aWNfYWc8L3RpdGxlPgogICAgPGRlc2M+Q3JlYXRlZCB3aXRoIFNrZXRjaC48L2Rlc2M+CiAgICA8ZGVmcz4KICAgICAgICA8bGluZWFyR3JhZGllbnQgeDE9IjUwJSIgeTE9IjAlIiB4Mj0iNTAlIiB5Mj0iMTAwJSIgaWQ9ImxpbmVhckdyYWRpZW50LTEiPgogICAgICAgICAgICA8c3RvcCBzdG9wLWNvbG9yPSIjRkI2MzYxIiBvZmZzZXQ9IjAlIj48L3N0b3A+CiAgICAgICAgICAgIDxzdG9wIHN0b3AtY29sb3I9IiNFRDNFNDUiIG9mZnNldD0iMTAwJSI+PC9zdG9wPgogICAgICAgIDwvbGluZWFyR3JhZGllbnQ+CiAgICA8L2RlZnM+CiAgICA8ZyBpZD0iaWNfYWciIHN0cm9rZT0ibm9uZSIgc3Ryb2tlLXdpZHRoPSIxIiBmaWxsPSJub25lIiBmaWxsLXJ1bGU9ImV2ZW5vZGQiPgogICAgICAgIDxnIGlkPSLnvJbnu4QiIHRyYW5zZm9ybT0idHJhbnNsYXRlKDIuMDAwMDAwLCAyLjAwMDAwMCkiPgogICAgICAgICAgICA8cGF0aCBkPSJNMTAuMTAwOTk5NSwwIEMyLjcwNTExMjc3LDAgMCwyLjcwNDY0MDk4IDAsMTAuMDk5MDI4NiBMMCwyNS45MDA5NzE0IEMwLDMzLjI5NTM1OSAyLjcwNTExMjc3LDM2IDEwLjEwMDk5OTUsMzYgTDI1Ljg5NDE4NiwzNiBDMzMuMjg5ODYzNCwzNiAzNiwzMy4yOTUzNTkgMzYsMjUuOTAwOTcxNCBMMzYsMTAuMDk5MDI4NiBDMzYsMi43MDQ2NDA5OCAzMy4yOTQ4ODcyLDAgMjUuODk5MDAwNSwwIEwxMC4xMDA5OTk1LDAgWiIgaWQ9IkZpbGwtMSIgZmlsbD0idXJsKCNsaW5lYXJHcmFkaWVudC0xKSI+PC9wYXRoPgogICAgICAgICAgICA8cGF0aCBkPSJNMTUuNzAzMDUxNSwyMC44NzkyNTEgTDE3LjE0ODMxOTIsMjAuODc5MjUxIEwxNi40MjMyMjYsMTkuMTkyOTYwNyBMMTUuNzAzMDUxNSwyMC44NzkyNTEgWiBNMTUuMzQ3MTU5OCwyMS43MjkwNTExIEwxNC45MTgzNTM2LDIyLjcxMDIxMjggTDEzLjk0MjExMDgsMjIuNzEwMjEyOCBMMTYuMDE4MTQ1OSwxOC4wMDAyODkzIEwxNi44NjE4Njk4LDE4LjAwMDI4OTMgTDE4LjkyOTUxNCwyMi43MTAyMTI4IEwxNy45MjcyMzAzLDIyLjcxMDIxMjggTDE3LjUwMzkyMTYsMjEuNzI5MDUxMSBMMTUuMzQ3MTU5OCwyMS43MjkwNTExIFogTTMxLjA1NjQ1MjksMjIuNzA2NzQwNyBMMzIsMjIuNzA2NzQwNyBMMzIsMTggTDMxLjA1NjQ1MjksMTggTDMxLjA1NjQ1MjksMjIuNzA2NzQwNyBaIE0yNy4zMDEwNzE2LDIwLjY4NDgxMjYgTDI5LjA0MDMxMTcsMjAuNjg0ODEyNiBMMjkuMDQwMzExNywxOS44MjY2MjE2IEwyNy4zMDEwNzE2LDE5LjgyNjYyMTYgTDI3LjMwMTA3MTYsMTguODYxOTUyNCBMMjkuODI1ODc3NiwxOC44NjE5NTI0IEwyOS44MjU4Nzc2LDE4LjAwMzQ3MjEgTDI2LjM1NzgxMzgsMTguMDAzNDcyMSBMMjYuMzU3ODEzOCwyMi43MDk5MjM0IEwyOS45MTY3MzEzLDIyLjcwOTkyMzQgTDI5LjkxNjczMTMsMjEuODUxNDQzMSBMMjcuMzAxMDcxNiwyMS44NTE0NDMxIEwyNy4zMDEwNzE2LDIwLjY4NDgxMjYgWiBNMjMuNTUyMDU1OSwyMS4yNDA5Mjk2IEwyMi40ODIzNTUzLDE4IEwyMS43MDE3MDgyLDE4IEwyMC42MzIwMDc1LDIxLjI0MDkyOTYgTDE5LjU5MDk1MTgsMTguMDAyNjA0MSBMMTguNTczMDQzNiwxOC4wMDI2MDQxIEwyMC4yMTU5MzI1LDIyLjcxMjgxNjkgTDIxLjAwNzI4NTIsMjIuNzEyODE2OSBMMjIuMDc4NzIxOSwxOS42MTg4NzM0IEwyMy4xNTAxNTg2LDIyLjcxMjgxNjkgTDIzLjk0ODQ1NTYsMjIuNzEyODE2OSBMMjUuNTg3MDA0NCwxOC4wMDI2MDQxIEwyNC41OTU0MjYzLDE4LjAwMjYwNDEgTDIzLjU1MjA1NTksMjEuMjQwOTI5NiBaIE0xMi41MDE3NjE5LDIwLjY5NzgzMyBDMTIuNTAxNzYxOSwyMS40NjQwMTMgMTIuMTIxMjc2LDIxLjg3MzQzMzIgMTEuNDMwMzI1MiwyMS44NzM0MzMyIEMxMC43MzU2MTI5LDIxLjg3MzQzMzIgMTAuMzUzMTAxNywyMS40NTIxNDk5IDEwLjM1MzEwMTcsMjAuNjY1MTM3MyBMMTAuMzUzMTAxNywxOC4wMDMxODI4IEw5LjM5NjgyMzQzLDE4LjAwMzE4MjggTDkuMzk2ODIzNDMsMjAuNjk3ODMzIEM5LjM5NjgyMzQzLDIyLjAyMzMxMjggMTAuMTMzNDkwNCwyMi43ODM0MTY1IDExLjQxNzMwNDgsMjIuNzgzNDE2NSBDMTIuNzEzODUwMiwyMi43ODM0MTY1IDEzLjQ1NzE3MjEsMjIuMDA4ODQ1NiAxMy40NTcxNzIxLDIwLjY1ODQ4MjQgTDEzLjQ1NzE3MjEsMTguMDAwMjg5MyBMMTIuNTAxNzYxOSwxOC4wMDAyODkzIEwxMi41MDE3NjE5LDIwLjY5NzgzMyBaIE03LjExNTM1NDgxLDE4LjAwMDI4OTMgTDguMDcxMDU0MzQsMTguMDAwMjg5MyBMOC4wNzEwNTQzNCwyMi43MTMxMDYyIEw3LjExNTM1NDgxLDIyLjcxMzEwNjIgTDcuMTE1MzU0ODEsMjAuNzk5MTAzIEw0Ljk1NjI3ODIyLDIwLjc5OTEwMyBMNC45NTYyNzgyMiwyMi43MTMxMDYyIEw0LDIyLjcxMzEwNjIgTDQsMTguMDAwMjg5MyBMNC45NTYyNzgyMiwxOC4wMDAyODkzIEw0Ljk1NjI3ODIyLDE5LjkwMTI3MjEgTDcuMTE1MzU0ODEsMTkuOTAxMjcyMSBMNy4xMTUzNTQ4MSwxOC4wMDAyODkzIFoiIGlkPSJGaWxsLTEiIGZpbGw9IiNGRkZGRkYiPjwvcGF0aD4KICAgICAgICAgICAgPHBhdGggZD0iTTE4LDEyIEMxNC42OTEyNjE2LDEyIDEyLDkuMzA4NDQ5MDcgMTIsNiBMMTIuODQ3NTExNiw2IEMxMi44NDc1MTE2LDguODQwODU2NDggMTUuMTU5MTQzNSwxMS4xNTIxOTkxIDE4LDExLjE1MjE5OTEgQzIwLjg0MDg1NjUsMTEuMTUyMTk5MSAyMy4xNTI0ODg0LDguODQwODU2NDggMjMuMTUyNDg4NCw2IEwyNCw2IEMyNCw5LjMwODQ0OTA3IDIxLjMwODQ0OTEsMTIgMTgsMTIiIGlkPSJGaWxsLTMiIGZpbGw9IiNGRkZGRkYiPjwvcGF0aD4KICAgICAgICA8L2c+CiAgICA8L2c+Cjwvc3ZnPg==";
        //Frame searchBarStackFrameItem = null;
           
        //foreach (var item in searchBarStackLayout.Children)
        //{
        //    if (item is Frame)
        //    {
        //        searchBarStackFrameItem = item as Frame;
        //        break;
        //    }
        //}
        if (SearchListFrame != null)
        {
            SearchListFrame.IsVisible = false;
           // searchBarStackLayout.Children.Remove(searchBarStackFrameItem);
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
                                                                ImageUrl = x.icon ?? appGalleryBase64,
                                                                AppId = x.appid
                                                            })).ToList();

        // Filter the items by the query and add them to the filtered items collection
        foreach (var item in items)
        {
            FilteredItems.Add(item);
        }

        CreateListView();
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

        //};

        //selectedItemLabel = new Label()
        //{
        //    HorizontalOptions = LayoutOptions.Center,
        //    VerticalOptions = LayoutOptions.Center,
        //    TextColor = Colors.White,
        //    FontSize = 10,
        //    Margin = new Thickness(0, 10, 0, 0),
        //    MaximumWidthRequest = 300,
        //    HorizontalTextAlignment = TextAlignment.Center,
        //    VerticalTextAlignment = TextAlignment.Center,
        //};

        // SearchListFrame.Content = null;

        // Create a ListView to display the filtered items
        var listView = new ListView
        {
            ItemsSource = FilteredItems,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Default,
            MaximumHeightRequest = 200,
            

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

        _localizationResourceManager.CurrentCulture = new System.Globalization.CultureInfo(picker.SelectedItem.ToString());
        
    }
}


