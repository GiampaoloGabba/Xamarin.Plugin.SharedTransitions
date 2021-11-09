﻿using System;
using Android.Content;
using Plugin.SharedTransitions.Platforms.Android;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Page), typeof(SharedTransitionPageRenderer))]
namespace Plugin.SharedTransitions.Platforms.Android
{
	public class SharedTransitionPageRenderer : PageRenderer
	{
		public SharedTransitionPageRenderer(Context context) : base(context)
		{
			
		}
		
		public SharedTransitionPageRenderer(IntPtr a, JniHandleOwnership b) : base (a, b))
	        {
		
	        }

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (Element != null)
				{
					if (Application.Current.MainPage is ISharedTransitionContainer shellPage)
					{
						shellPage.TransitionMap.RemoveFromPage(Element);
					}

					if (Element.Parent is ISharedTransitionContainer navPage)
					{
						navPage.TransitionMap.RemoveFromPage(Element);
					}
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e);
			}

			base.Dispose(disposing);
		}
	}
}
