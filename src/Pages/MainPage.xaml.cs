using HuaweiHMSInstaller.Helper;
using HuaweiHMSInstaller.Pages;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Popup;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace HuaweiHMSInstaller;

public partial class MainPage : ContentPage
{
    private SfPopup _sfPopup;
    private Button footerButton = new ();

    public MainPage()
    {
        InitializeComponent();
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
            Text = "Checking your internet connection",
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

                label.Text = "Internet Connection OK";
                stackLayout.Children.Remove(circle);
                grid.Children.Add(checkMark);
                Dispatcher.StartTimer(TimeSpan.FromSeconds(2), () =>
                {
                    if (!popup.IsOpen) return false;
                    popup.IsOpen = false;
                    popup.Dismiss();
                    this.stackLayout.Children.Remove(popup);
                    Application.Current.MainPage.Navigation.PushModalAsync(new DownloadandInstallPage(), true);
                    return false;
                });
                return false;
            }
            else
            {
                label.Text = "Internet Connection NOT OK. Try Again";
                stackLayout.Children.Remove(circle);
                grid.Children.Add(wrongMark);
                // Add a button as the third object
                popup.ShowFooter = true;
                footerButton.Clicked += OnButtonClicked;

                return false;

            }

        });

        //Set the label as the popup content
        this.stackLayout.Children.Add(popup);

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
        popup.OverlayMode = PopupOverlayMode.Blur;
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
            headerLabel.Text = "Check Internet";
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
                Text = "Retry",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Color.FromArgb("#ed1c24"),
                TextColor = Color.FromArgb("#ffffff"),
                CornerRadius = 10,
                WidthRequest = 100,
                HeightRequest = 50,
                Margin = new Thickness(0, 0, 0, 10)
                //Style = new Style(typeof(Button))
                //{
                //    Setters =
                //    {
                //        new Setter { Property = Button.BorderWidthProperty, Value = 1 },
                //        new Setter { Property = Button.BorderColorProperty, Value = Color.FromArgb("#ed1c24") },
                //        new Setter { Property = Button.FontSizeProperty, Value = 20 },
                //        new Setter { Property = Button.FontFamilyProperty, Value = "Arial" },
                //        new Setter { Property = Button.FontAttributesProperty, Value = FontAttributes.Bold },
                //        new Setter { Property = Button.HorizontalOptionsProperty, Value = LayoutOptions.Center },
                //        new Setter { Property = Button.VerticalOptionsProperty, Value = LayoutOptions.Center },
                //        new Setter { Property = Button.WidthRequestProperty, Value = 100 },
                //        new Setter { Property = Button.HeightRequestProperty, Value = 50 },
                //        new Setter { Property = Button.MarginProperty, Value = new Thickness(0, 0, 0, 10) },
                //        new Setter { Property = Button.BackgroundColorProperty, Value = Color.FromArgb("#ed1c24") },
                //        new Setter { Property = Button.TextColorProperty, Value = Color.FromArgb("#ffffff") },
                //        new Setter { Property = Button.IsVisibleProperty, Value = false },
                //    }
                //}
            };
            return footerButton;
        });
    }
}
