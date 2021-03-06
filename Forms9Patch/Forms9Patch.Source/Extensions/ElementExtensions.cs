﻿using System;
using Xamarin.Forms;
using P42.Utils;

namespace Forms9Patch
{
	/// <summary>
	/// Visual element extensions.
	/// </summary>
	public static class ElementExtensions
	{
		/// <summary>
		/// the element's hosting page.
		/// </summary>
		/// <returns>The page.</returns>
		/// <param name="element">Element.</param>
		public static Page HostingPage(this Element element)
		{
			if (element == null)
				return null;
			var page = element as Page;
			while (page == null && element.Parent != null)
			{
				element = element.Parent;
				page = element as Page;
			}
			Page lastSoloPage = page;
			/*
			if (page != null)
			{
				// ok we have a page ... is it the right kind?
				while (page != null && page.Parent != null)
				{
					if (!(page is MasterDetailPage ))
						lastSoloPage = page;
					page = page.Parent as Page;
				}
			}
			*/
			return lastSoloPage;
		}

		/// <summary>
		/// The topmost page
		/// </summary>
		/// <returns>The page.</returns>
		/// <param name="element">Element.</param>
		public static Page TopPage(this Element element)
		{
			while (element.Parent != null)
				element = element.Parent;
			return HostingPage(element);
		}

		/// <summary>
		/// Fine a ancestor of a specific type for the specified element.
		/// </summary>
		/// <param name="element">Element.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Parent<T>(this Element element) where T : Element
		{
			if (element == null)
				return null;
			while (element.Parent != null)
			{
				element = element.Parent;
				if (element.GetType() == typeof(T))
					return (T)element;
			}
			return null;
		}

        /// <summary>
        /// Fine an ancestor of this element of a type that is inherited from T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        public static T ParentInheritedFrom<T>(this Element element) where T : Element
        {
            if (element == null)
                return null;
            while (element.Parent != null)
            {
                element = element.Parent;
                if (element is T)
                    return (T)element;
            }
            return null;
        }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool IsInNativeLayout(this VisualElement e)
        {
            var isInNativeLayout = (bool)e.GetPropertyValue("IsInNativeLayout");
            return isInNativeLayout;
        }

        public static void SetIsInNativeLayout(this VisualElement e, bool value)
        {
            e.SetPropertyValue("IsInNativeLayout", value);
        }
	}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
