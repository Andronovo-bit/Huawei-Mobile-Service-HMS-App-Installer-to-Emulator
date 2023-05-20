using HuaweiHMSInstaller.Pages;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Core;
using Syncfusion.Maui.Popup;

namespace HuaweiHMSInstaller;

public partial class MainPage : ContentPage
{
    private SfPopup _sfPopup;

    public MainPage()
	{
		InitializeComponent();
	}

	private void OnInstallButtonClicked(object sender, EventArgs e)
    {
        //Initialize the popup
        var popup = new SfPopup();
        _sfPopup = popup;

        // Create a radial gradient brush with two colors
        //var brush = new RadialGradientBrush
        //{
        //    Center = new Point(0.5, 0.5), // The center of the gradient
        //    Radius = 0.5, // The radius of the gradient
        //    GradientStops = new GradientStopCollection // The colors and positions of the gradient
        //    {
        //        new GradientStop(Colors.Red, 0), // Red at the center
        //        new GradientStop(Colors.Yellow, 1) // Yellow at the edge
        //    }
        //};

        // Create a PathF object that defines the check mark shape
        var pathF = new PathF();
        pathF.MoveTo(5, 25); // Move to the start point
        pathF.LineTo(25, 40); // Draw a line to the first point
        pathF.LineTo(45, 10); // Draw a line to the second point

        // Create a PathGeometry object from the PathF object

        PathFigureCollection pathFigureCollection = new PathFigureCollection
        {
             new PathFigure
            {
                StartPoint = new Point(10, 50),
                Segments = new PathSegmentCollection
                {
                    new LineSegment { Point = new Point(40, 80) },
                    new LineSegment { Point = new Point(90, 20) }
                }
            }
        };
        var pathGeometry = new PathGeometry(pathFigureCollection);

        // Create a check mark path shape
        var checkMark = new Microsoft.Maui.Controls.Shapes.Path
        {
            Stroke = new SolidColorBrush(Colors.Green),
            StrokeThickness = 2,
            Data = pathGeometry,
            WidthRequest = 100,
            HeightRequest = 100
        };

        // Add the shapes to a grid layout
        var grid = new Grid();
        grid.Children.Add(checkMark);

        // Create an animation that scales the circle and rotates the check mark
        var animationTickMark = new Animation
        {
            { 0, 0.5, new Animation(v => checkMark.Scale = v, 1, 1.2) },
            { 0.5, 1, new Animation(v => checkMark.Scale = v, 1.2, 1) },
        };

        // Create a linear gradient brush
        LinearGradientBrush brush = new LinearGradientBrush
        {
            // Set the start and end points of the gradient axis
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1)
        };

        // Add two gradient stops with blue and green colors
        brush.GradientStops.Add(new GradientStop { Color = Colors.Blue, Offset = 0.0f });
        brush.GradientStops.Add(new GradientStop { Color = Colors.Green, Offset = 1.0f });

        //create gradient color stroke circle
        var circle = new Ellipse
        {
            WidthRequest = 100,
            HeightRequest = 100,
            Stroke = brush,
            StrokeThickness = 10,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };


        // popup transparent background
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


        // Add a label as the second object
        //Create a label for the popup content
        var label = new Label
        {
            Text = "Checking your internet connection",
            TextColor = Color.FromArgb("#000000"),
            FontSize = 20,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        stackLayout.Children.Add(label);

        //label add ... animation
        var dotAnimationBehavior = new DotAnimationBehavior();
        label.Behaviors.Add(dotAnimationBehavior);

        //var animLabel = new Label();
        //var animation = new Animation(v => animLabel.BackgroundColor = Color.FromHsla(v, 1, 0.5), 0, 1); // Create an animation that changes the hue of the label
        //animation.Commit(animLabel, "ColorAnimation", 16, 1000, Easing.Linear, null, () => true); // Start the animation with a repeat action
        //stackLayout.Children.Add(animLabel);

        var animation = new Animation(v => circle.Rotation = v, 0, 360);
        animation.Commit(circle, "CircleAnimation", 16, 1000, Easing.Linear, null, () => true);
        stackLayout.Children.Add(circle);

        Dispatcher.StartTimer(TimeSpan.FromSeconds(4), () =>
        {
            label.Text = "Internet Connection OK";
            stackLayout.Children.Remove(circle);
            animationTickMark.Commit(this, "ConfirmationAnimation", length: 1000);
            stackLayout.Children.Add(grid);

            Dispatcher.StartTimer(TimeSpan.FromSeconds(2), () =>
            {
                popup.IsOpen = false;
                popup.Dismiss();
                this.stackLayout.Children.Remove(popup);
                Application.Current.MainPage.Navigation.PushModalAsync(new DownloadandInstallPage(), true);
                return false;
            });
            return false;
        });
        //Set the label as the popup content
        popup.ContentTemplate = new DataTemplate(() =>
        {
            return stackLayout;
        });
        this.stackLayout.Children.Add(popup);
        popup.Show();


    }

}
internal class DotAnimationBehavior : Behavior
{
    private Label _label;
    private string _text;

    protected override void OnAttachedTo(BindableObject bindable)
    {
        base.OnAttachedTo(bindable);
        _label = bindable as Label;
        _text = _label.Text;
        Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            if (_label.Text.EndsWith("..."))
            {
                _label.Text = _label.Text.Substring(0, _label.Text.Length - 3);
            }
            else
            {
                _label.Text += ".";
            }
            return true;
        });
    }

    protected override void OnDetachingFrom(BindableObject bindable)
    {
        base.OnDetachingFrom(bindable);
        _label.Text = _text;
    }
}