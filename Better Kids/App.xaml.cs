using Better_Kids.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.System.Profile;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Better_Kids
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
	{
		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
				Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
				Microsoft.ApplicationInsights.WindowsCollectors.Session);

			this.InitializeComponent();
			this.UnhandledException += App_UnhandledException;
		}

		protected override async void OnLaunched(LaunchActivatedEventArgs e)
		{
#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif
			await Oxford_GetImage.RetrieveUrls();

			Frame rootFrame = Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
			if (rootFrame == null)
			{
				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new Frame();

				rootFrame.NavigationFailed += OnNavigationFailed;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					//TODO: Load state from previously suspended application
				}

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;
			}

			// Set TitleBar Color and Mobile View Mode
			var view = ApplicationView.GetForCurrentView();
			if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
			{
				var bgColor = Color.FromArgb(127, 255, 214, 4);
				view.TitleBar.BackgroundColor = bgColor;
				view.TitleBar.ButtonBackgroundColor = bgColor;
			}
			else if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
			{
				// To enable Hiding StatusBar, you should Add Windows Mobile to References.
				//if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
				//  StatusBar.GetForCurrentView().HideAsync();
				view.TryEnterFullScreenMode();
				DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
			}

			if (e.PrelaunchActivated == false)
			{
				if (rootFrame.Content == null)
				{
					// When the navigation stack isn't restored navigate to the first page,
					// configuring the new page by passing required information as a navigation
					// parameter
					rootFrame.Navigate(typeof(MainPage), e.Arguments);
				}
				// Ensure the current window is active
				Window.Current.Activate();
			}
		}

		private async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			e.Handled = true;
			await new MessageDialog("出错啦:\r\n" + e.Message, "Better Kids :(").ShowAsync();
		}

		void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}
	}
}
