using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
