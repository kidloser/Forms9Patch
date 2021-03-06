﻿using System;
using Xamarin.Forms;
namespace Forms9Patch
{
    /// <summary>
    /// Activity indicator full page overlay.
    /// </summary>
    public class ActivityIndicatorPopup : ModalPopup
    {
        #region Properties
        /// <summary>
        /// The color property.
        /// </summary>
        public static readonly BindableProperty ColorProperty = BindableProperty.Create("Color", typeof(Color), typeof(ActivityIndicatorPopup), Color.Blue, propertyChanged: (b, o, n) =>
        {
            if (b is ActivityIndicatorPopup indicatorPopup)
                indicatorPopup._indicator.Color = indicatorPopup.Color;
        });
        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        #endregion


        #region Fields
        ActivityIndicator _indicator = new ActivityIndicator
        {
            BackgroundColor = Color.Transparent,
            Color = Color.Blue,

        };
        #endregion


        #region Factory
        /// <summary>
        /// Create this instance.
        /// </summary>
        public static ActivityIndicatorPopup Create()
        {
            var indicator = new ActivityIndicatorPopup();
            indicator.IsVisible = true;
            return indicator;
        }
        #endregion


        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Forms9Patch.ActivityIndicatorPopup"/> class.
        /// </summary>
        public ActivityIndicatorPopup()
        {
            if (Device.RuntimePlatform == Device.UWP)
                _indicator.WidthRequest = 300;
            Content = _indicator;

            CancelOnPageOverlayTouch = false;
            BackgroundColor = Color.FromRgba(0, 0, 0, 1);
            HorizontalOptions = LayoutOptions.Center;
            VerticalOptions = LayoutOptions.Center;
            _indicator.IsVisible = true;

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Forms9Patch.ActivityIndicatorPopup"/> class.
        /// </summary>
        /// <param name="target">Target.</param>
        [Obsolete]
        public ActivityIndicatorPopup(VisualElement target) : base(target)
        {
            throw new NotSupportedException(P42.Utils.ReflectionExtensions.CallerMemberName() + " is obsolete.");
        }
        #endregion


        #region Property Change Management
        /// <summary>
        /// Ons the property changed.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected override void OnPropertyChanged(string propertyName = null)
        {
            if (!P42.Utils.Environment.IsOnMainThread)
            {
                Device.BeginInvokeOnMainThread(() => OnPropertyChanged(propertyName));
                return;
            }

            base.OnPropertyChanged(propertyName);

            if (propertyName == IsVisibleProperty.PropertyName)
                Activate();
            else if (propertyName == ScaleProperty.PropertyName)
                _indicator.Scale = Scale;
        }

        void Activate()
        {
            if (P42.Utils.Environment.IsOnMainThread)
            {
                _indicator.IsVisible = IsVisible;
                _indicator.IsRunning = IsVisible;
            }
            else
                Device.BeginInvokeOnMainThread(Activate);
        }
        #endregion
    }
}
