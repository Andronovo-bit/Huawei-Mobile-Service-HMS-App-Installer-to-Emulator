using HuaweiHMSInstaller.Helper;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Popup;

namespace HuaweiHMSInstaller.Pages;

public partial class ThanksPage : ContentPage
{
    private SfPopup _sfPopup;
	public ThanksPage()
	{
		InitializeComponent();
    }
    private void OnFinishButtonClicked(object sender, EventArgs e)
    {
        //Create popup and push it to the navigation stack
        //Initialize the popup
        var popup = new SfPopup();
        _sfPopup = popup;
        // popup transparent background
        //popup.Background = Color.FromRgba(0, 0, 0, 0.5);
        popup.AutoSizeMode = PopupAutoSizeMode.None;
        popup.OverlayMode = PopupOverlayMode.Blur;

        popup.PopupStyle = new PopupStyle 
        { 
            HeaderTextAlignment = TextAlignment.Center,
            Stroke = Colors.Gray,
            StrokeThickness = 1,
            MessageBackground = Colors.White,
            FooterBackground = Colors.White,
            CornerRadius = 10,
            BlurIntensity= PopupBlurIntensity.ExtraDark,
            HasShadow = true,
            OverlayColor = Color.FromRgba(0, 0, 0, 0.5),
        };

        popup.HeaderTemplate = new DataTemplate(() =>
        {
            var headerLabel = new Label();
            headerLabel.Text = "HUAWEI";
            headerLabel.FontFamily = "Arial";
            headerLabel.FontSize = 20;
            headerLabel.FontAttributes = FontAttributes.Bold;
            headerLabel.TextColor = Color.FromArgb("#ed1c24");
            headerLabel.HorizontalOptions = LayoutOptions.Center;
            headerLabel.VerticalOptions = LayoutOptions.Center;

            return headerLabel;
        });

        //Create a label for the popup content
        var label = new Label
        {
            Text = "Thank you for choosing us!",
            TextColor = Color.FromArgb("#000000"),
            FontSize = 20,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        //add label 3, 2, 1, 0 animation
        Dispatcher.StartTimer(TimeSpan.FromSeconds(2), () =>
        {
            label.Behaviors.Add(new CountDownBehavior(true));

            return false;
        });

        //Set the label as the popup content
        popup.ContentTemplate = new DataTemplate(() => label);

        this.stackLayout.Children.Add(popup);
        popup.Show();
    }

    private void CloseButton_Clicked(object sender, EventArgs e)
    {
        //Pop the popup from the navigation stack
        _sfPopup.Dismiss();
        _sfPopup.IsOpen = false;
    }
}

