using Better_Kids.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Better_Kids.View
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class Guess : Page
	{
		private static string[] emotions = {
			"anger",
			"contempt",
			"disgust",
			"fear",
			"happiness",
			"neutral",
			"sadness",
			"surprise" };

		private static string[] emotionstrs = {
			"生气",
			"轻蔑",
			"恶心",
			"害怕",
			"高兴",
			"自然",
			"悲伤",
			"惊讶" };

		private MyFrame myframe;
		private int correctBtn;

		public Guess()
		{
			this.InitializeComponent();
		}

		private void Reset()
		{
			image.Source = null;
			button1.Content = button2.Content = "???";
		}

		private void LoadImage()
		{
			Random rand = new Random(DateTime.Now.Millisecond + DateTime.Now.Minute);
			int correct = rand.Next(emotions.Count());
			int wrong = rand.Next(emotions.Count());
			if (correct == wrong)
			{
				wrong = (wrong + 1) % emotions.Count();
			}
			image.Source = Emotion.GetImageFromWeb(emotions[correct]);
			if (rand.NextDouble() < 0.5)
			{
				correctBtn = 1;
				button1.Content = emotionstrs[correct];
				button2.Content = emotionstrs[wrong];
			}
			else
			{
				correctBtn = 2;
				button2.Content = emotionstrs[correct];
				button1.Content = emotionstrs[wrong];
			}

		}

		private async void check(int i)
		{
			button1.IsEnabled = false;
			button2.IsEnabled = false;
			if (i == correctBtn)
			{
				myframe.SetTree(0.3);
				image.Source = new BitmapImage(new Uri("ms-appx:///Assets/right.png"));
				await myframe.Prompt("恭喜你，答对了~");
			}
			else
			{
				myframe.SetTree(-0.2);
				image.Source = new BitmapImage(new Uri("ms-appx:///Assets/wrong.png"));
				await myframe.Prompt("打错了，加油哦~");
			}
			LoadImage();
			button1.IsEnabled = true;
			button2.IsEnabled = true;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			myframe = (MyFrame)e.Parameter;
			LoadImage();
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			check(1);
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			check(2);
		}
	}
}
