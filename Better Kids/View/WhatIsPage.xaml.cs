using Better_Kids.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
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
	public sealed partial class WhatIsPage : Page
	{
		private CameraView cv = new CameraView();
		MyFrame myframe;

		public WhatIsPage()
		{
			this.InitializeComponent();
			NavigationCacheMode = NavigationCacheMode.Disabled;
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			myframe = (MyFrame)e.Parameter;

			previewControl.Source = await cv.InitializeCameraAsync(false);
			previewControl.FlowDirection = FlowDirection.LeftToRight;
			await cv.Start();

			Application.Current.Suspending += Application_Suspending;
			Application.Current.Resuming += Application_Resuming;
		}

		private async void Application_Resuming(object sender, object o)
		{
			previewControl.Source = await cv.InitializeCameraAsync(false);
			previewControl.FlowDirection = FlowDirection.RightToLeft;
			await cv.Start();
		}

		protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
			previewControl.Source = null;
			await cv.UninitializeCameraAsync();

			Application.Current.Suspending -= Application_Suspending;
			Application.Current.Resuming -= Application_Resuming;
		}

		private async void Application_Suspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();

			previewControl.Source = null;
			await cv.UninitializeCameraAsync();

			deferral.Complete();
		}

		private async Task play_string(string str)
		{
			// The media object for controlling and playing audio.
			MediaElement mediaElement = new MediaElement();

			// The object for controlling the speech synthesis engine (voice).
			var synth = new SpeechSynthesizer();

			// Generate the audio stream from plain text.
			SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(str);

			// Send the stream to the media object.
			mediaElement.SetSource(stream, stream.ContentType);
			mediaElement.Play();
		}

		private async Task listenfor_string(string str)
		{
			// Create an instance of SpeechRecognizer.
			var speechRecognizer = new SpeechRecognizer();

			// Compile the dictation grammar by default.
			await speechRecognizer.CompileConstraintsAsync();

			// Start recognition.
			SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeWithUIAsync();

			// Do something with the recognition result.
			string reply;
			if (speechRecognitionResult.Text.Contains(str) || str.Contains(speechRecognitionResult.Text))
				reply = "说的不错哦";
			else
				reply = "多听听再试试吧";

			await play_string(reply);
			var messageDialog = new Windows.UI.Popups.MessageDialog(reply, "Better Kids");
			await messageDialog.ShowAsync();
		}

		private async void textblock1_Tapped(object sender, TappedRoutedEventArgs e)
		{
			await play_string((string)textblock1.Text);
		}

		private async void textblock2_Tapped(object sender, TappedRoutedEventArgs e)
		{
			await play_string((string)textblock2.Text);
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			myframe.ToInkPage((string)textblock2.Text);
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			button.IsEnabled = false;
			var result = await cv.Click2();
			textblock1.Text = result.Item1;
			textblock2.Text = result.Item2;
			button.IsEnabled = true;
		}
	}
}
