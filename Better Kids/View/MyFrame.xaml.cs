using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
	public sealed partial class MyFrame : Page
	{
		private static double treeHeight = 10;

		public MyFrame()
		{
			this.InitializeComponent();
			SetTree(0);
			Timer.Width = (int)(mainFrame.ActualWidth);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			switch ((int)e.Parameter)
			{
			case 1:
				Title.Text = "猜猜图片中的情感";
				mainFrame.Navigate(typeof(Guess), this);
				break;
			case 2:
				Title.Text = "模仿图片中的表情";
				mainFrame.Navigate(typeof(Mimic), this);
				break;
			case 3:
				Title.Text = "拍下你想知道的东西";
				mainFrame.Navigate(typeof(WhatIsPage), this);
				break;
			}
		}

		private void GoBack(object sender, RoutedEventArgs e)
		{
			// Awake NavigeteFrom in Mimic/WhatIs Page
			mainFrame.Navigate(typeof(MainPage));

			Frame rootFrame = Window.Current.Content as Frame;
			rootFrame.Navigate(typeof(MainPage));
		}

		public void SetTree(double percent)
		{
			const double maxHeight = 400;
			double pct = treeHeight / maxHeight + percent;
			if (pct > 1) pct = 1;
			if (pct < 0) pct = 0;
			treeHeight = Mid.Height = pct * maxHeight;
		}

		public void ToInkPage(string str)
		{
			Title.Text = str;
			mainFrame.Navigate(typeof(InkPage), str);
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

		public async Task Prompt(string str)
		{
			var original_title = Title.Text;
			Title.Text = str;
			await play_string(str);
			await Task.Delay(1500);
			Title.Text = original_title;
		}
	}
}
