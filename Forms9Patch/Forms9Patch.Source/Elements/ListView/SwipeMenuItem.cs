﻿using System;
using Xamarin.Forms;

namespace Forms9Patch
{
    /// <summary>
    /// Describes a Swipe action button that appears when a cell is swiped 
    /// </summary>
    public class SwipeMenuItem : BindableObject, IMenuItem
    {
        #region Properties

        #region Key property
        /// <summary>
        /// The key property backing store.
        /// </summary>
        public static readonly BindableProperty KeyProperty = BindableProperty.Create("Key", typeof(string), typeof(SwipeMenuItem), default(string));
        /// <summary>
        /// Gets or sets the key - used to identify a SwipeMenuEvent.  Will return Text if set to null.
        /// </summary>
        /// <value>The key.</value>
        public string Key
        {
            get => (string)GetValue(KeyProperty) ?? (string)GetValue(TextProperty);
            set => SetValue(KeyProperty, value);
        }
        #endregion Key property

        #region IconImage property
        /// <summary>
        /// backing store for IconImage property
        /// </summary>
        public static readonly BindableProperty IconImageProperty = BindableProperty.Create("IconImage", typeof(Forms9Patch.Image), typeof(SwipeMenuItem), default(Forms9Patch.Image));
        /// <summary>
        /// Gets/Sets the IconImage property
        /// </summary>
        public Forms9Patch.Image IconImage
        {
            get => (Forms9Patch.Image)GetValue(IconImageProperty);
            set => SetValue(IconImageProperty, value);
        }
        #endregion IconImage property

        #region IconText Property
        /// <summary>
        /// The icon text property backing store.
        /// </summary>
        public static readonly BindableProperty IconTextProperty = BindableProperty.Create("IconText", typeof(string), typeof(SwipeMenuItem), default(string));
        /// <summary>
        /// Gets or sets the icon text - an alternative to IconImageSource.
        /// </summary>
        /// <value>The icon text.</value>
        public string IconText
        {
            get => (string)GetValue(IconTextProperty);
            set => SetValue(IconTextProperty, value);
        }
        #endregion IconText property

        #region Text property
        /// <summary>
        /// The text property backing store.
        /// </summary>
        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(SwipeMenuItem), default(string));
        /// <summary>
        /// Gets or sets the text (can be in HTML format.
        /// </summary>
        /// <value>The html text.</value>
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        #endregion Text Property

        #region HtmlText property
        /// <summary>
        /// backing store for HtmlText property
        /// </summary>
        public static readonly BindableProperty HtmlTextProperty = BindableProperty.Create("HtmlText", typeof(string), typeof(SwipeMenuItem), default(string));
        /// <summary>
        /// Gets/Sets the HtmlText property
        /// </summary>
        public string HtmlText
        {
            get => (string)GetValue(HtmlTextProperty);
            set => SetValue(HtmlTextProperty, value);
        }
        #endregion HtmlText property

        #region TextColor property
        /// <summary>
        /// The text color property.
        /// </summary>
        public static readonly BindableProperty TextColorProperty = BindableProperty.Create("TextColor", typeof(Color), typeof(SwipeMenuItem), Color.White);
        /// <summary>
        /// Gets or sets the color of the text and icon (image).
        /// </summary>
        /// <value>The color of the text.</value>
        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }
        #endregion TextColor property

        #region SwipeExecutable Property
        /// <summary>
        /// The swipe executable property backing store.
        /// </summary>
        public static readonly BindableProperty SwipeExecutableProperty = BindableProperty.Create("SwipeExecutable", typeof(bool), typeof(SwipeMenuItem), default(bool));
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:Forms9Patch.SwipeAction"/> swipe executable.
        /// Only used if this SwipeAction is at the top of the stack of SwipeActions.
        /// </summary>
        /// <value><c>true</c> if swipe executable; otherwise, <c>false</c>.</value>
        public bool IsTriggeredOnFullSwipe
        {
            get => (bool)GetValue(SwipeExecutableProperty);
            set => SetValue(SwipeExecutableProperty, value);
        }
        #endregion SwipeExecutable region

        #region BackgroundColor property
        /// <summary>
        /// The background color property backing store.
        /// </summary>
        public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create("BackgroundColor", typeof(Color), typeof(SwipeMenuItem), default(Color));
        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>The color of the background.</value>
        public Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }
        #endregion BackgroundColor property

        /*
		/// <summary>
		/// The action property backing store.
		/// </summary>
		public static readonly BindableProperty ActionProperty = BindableProperty.Create("Action", typeof(Action<object,object>), typeof(SwipeMenuItem), default(Action<object,object>));
		/// <summary>
		/// Gets or sets the action to be performed if activated.
		/// </summary>
		/// <value>The action.</value>
		public Action<object,object> Action
		{
			get { return (Action<object,object>)GetValue(ActionProperty); }
			set { SetValue(ActionProperty, value); }
		}
		*/

        #endregion


        #region Change management
        /// <summary>
        /// BindableProperty has changed.
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

            if (propertyName == IconTextProperty.PropertyName && IconTextProperty != null)
                IconImage = null;
            else if (propertyName == IconImageProperty.PropertyName && IconImage?.Source != null)
                IconText = null;
            else if (propertyName == TextProperty.PropertyName && Text != null)
                HtmlText = null;
            else if (propertyName == HtmlTextProperty.PropertyName && HtmlText != null)
                Text = null;
        }
        #endregion
    }
}
