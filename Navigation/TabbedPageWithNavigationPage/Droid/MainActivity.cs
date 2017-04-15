using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace TabbedPageWithNavigationPage.Droid
{
	[Activity (Label = "TabbedPageWithNavigation",  
        //Icon = "@android:color/transparent",
        MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			global::Xamarin.Forms.Forms.Init (this, bundle);

            ActionBar.SetIcon(Android.Resource.Color.Transparent);

			LoadApplication (new App ());
		}
	}
}

