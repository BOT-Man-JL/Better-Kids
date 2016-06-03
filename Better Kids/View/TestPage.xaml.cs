using Better_Kids.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Better_Kids.View
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class TestPage : Page
	{
		CameraView cv = new CameraView();

		public TestPage()
		{
			this.InitializeComponent();

			NavigationCacheMode = NavigationCacheMode.Disabled;

			Application.Current.Suspending += Application_Suspending;
			Application.Current.Resuming += Application_Resuming;

			inkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			PreviewControl.Source = await cv.InitializeCameraAsync(true);
			PreviewControl.FlowDirection = FlowDirection.RightToLeft;

			await cv.Start();
		}

		protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			PreviewControl.Source = null;
			await cv.UninitializeCameraAsync();
		}

		private async void Application_Suspending(object sender, SuspendingEventArgs e)
		{
			if (Frame.CurrentSourcePageType == typeof(MainPage))
			{
				var deferral = e.SuspendingOperation.GetDeferral();

				PreviewControl.Source = null;
				await cv.UninitializeCameraAsync();

				deferral.Complete();
			}
		}

		private async void Application_Resuming(object sender, object o)
		{
			if (Frame.CurrentSourcePageType == typeof(MainPage))
			{
				PreviewControl.Source = await cv.InitializeCameraAsync(true);
				PreviewControl.FlowDirection = FlowDirection.RightToLeft;

				await cv.Start();
			}
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			image.Source = Emotion.GetImageFromWeb("happiness");
		}

		private async void button_Click2(object sender, RoutedEventArgs e)
		{
			var result = await cv.Click();
			if (result != null)
				textBlock.Text = $"{result.Item1} - {result.Item2}";
			else
				textBlock.Text = "No Face.";
		}

		private async void button_Click3(object sender, RoutedEventArgs e)
		{
			// Create an instance of SpeechRecognizer.
			var speechRecognizer = new Windows.Media.SpeechRecognition.SpeechRecognizer();

			// Compile the dictation grammar by default.
			await speechRecognizer.CompileConstraintsAsync();

			// Start recognition.
			Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();

			// Do something with the recognition result.
			var messageDialog = new Windows.UI.Popups.MessageDialog(speechRecognitionResult.Text, "Text spoken");
			await messageDialog.ShowAsync();
		}

		private async void button_Click4(object sender, RoutedEventArgs e)
		{
			await cv.Click2();
		}

		private async void button_Click5(object sender, RoutedEventArgs e)
		{
			var container = new InkRecognizerContainer();

			var result = await container.RecognizeAsync(inkCanvas.InkPresenter.StrokeContainer, InkRecognitionTarget.All);

			textBlock.Text = result[0].GetTextCandidates()[0];
		}
	}
}
