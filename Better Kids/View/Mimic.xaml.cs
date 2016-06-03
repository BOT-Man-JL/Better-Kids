using Better_Kids.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Better_Kids.View
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class Mimic : Page
	{
		private static string[] emotions = {
			"anger",
			"contempt",
			"disgust",
			"fear",
			"happiness",
			"surprise" };

		private static string[] emotionstrs = {
			"生气",
			"轻蔑",
			"恶心",
			"害怕",
			"高兴",
			"惊讶" };

		private MyFrame myframe;
		private int emotionIndex;
		private CameraView cv = new CameraView();
		//private bool fEnd = false;

		public Mimic()
		{
			this.InitializeComponent();
			NavigationCacheMode = NavigationCacheMode.Disabled;
		}

		private void LoadImage()
		{
			Random rand = new Random(DateTime.Now.Millisecond + DateTime.Now.Minute);
			emotionIndex = rand.Next(emotions.Count());
			image.Source = Emotion.GetImageFromWeb(emotions[emotionIndex]);
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			myframe = (MyFrame)e.Parameter;

			LoadImage();

			previewControl.Source = await cv.InitializeCameraAsync(true);
			previewControl.FlowDirection = FlowDirection.RightToLeft;
			await cv.Start();

			Application.Current.Suspending += Application_Suspending;
			Application.Current.Resuming += Application_Resuming;
		}

		private async void Application_Resuming(object sender, object o)
		{
			LoadImage();

			previewControl.Source = await cv.InitializeCameraAsync(true);
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

		private async void button_Click(object sender, RoutedEventArgs e)
		{
			button.IsEnabled = false;
			var result = await cv.Click();
			if (result != null)
			{
				if (result.Item2 > .5 && result.Item1 == emotions[emotionIndex])
				{
					myframe.SetTree(0.3);
					image.Source = new BitmapImage(new Uri("ms-appx:///Assets/right.png"));
					await myframe.Prompt("模仿得真像~");
				}
				else
				{
					myframe.SetTree(-0.2);
					image.Source = new BitmapImage(new Uri("ms-appx:///Assets/wrong.png"));
					await myframe.Prompt("不是这个表情哦~");
				}
				LoadImage();
			}
			else
			{
				await myframe.Prompt("没看清你的脸哦~");
			}
			button.IsEnabled = true;
		}
	}
}
