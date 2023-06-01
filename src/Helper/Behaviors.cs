using LocalizationResourceManager.Maui;

namespace HuaweiHMSInstaller.Helper
{
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

    internal class CountDownBehavior : Behavior
    {
        private Label _label;
        private int _count = 3;
        private bool IsClosedEnv {get;set;}
        private readonly ILocalizationResourceManager _localizationResourceManager;


        public CountDownBehavior(bool isClosedEnv)
        {
            IsClosedEnv = isClosedEnv;
            _localizationResourceManager = Services.ServiceProvider.GetService<ILocalizationResourceManager>();
        }
        
        protected override void OnAttachedTo(BindableObject bindable)
        {
            base.OnAttachedTo(bindable);
            _label = bindable as Label;
            Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>

            {
                _label.Text = _count.ToString();
                _count--;
                if (_count == -1 && IsClosedEnv)
                {
                    _label.Text = _localizationResourceManager.GetValue("bye_bye");
                    // close application after 1 second use dispatcher
                    Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
                    {
                        System.Environment.Exit(0);
                        return false;
                    });

                    return false;
                }
                return true;
            });
        }
    }
}
