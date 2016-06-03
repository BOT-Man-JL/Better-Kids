using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
	public sealed partial class InkPage : Page
	{
		public InkPage()
		{
			this.InitializeComponent();

			inkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen;

			var attr = new InkDrawingAttributes();
			attr.Color = Colors.Black;
			attr.PenTip = PenTipShape.Circle;
			attr.Size = new Size(10, 10);

			inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(attr);
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			text.Text = (string)e.Parameter;

			var attr = new InkDrawingAttributes();
			attr.Color = Colors.Black;
			attr.PenTip = PenTipShape.Circle;

			int stoke_w = 10;
			int ratio = (int)Math.Sqrt((text.Text.Length - 1) / 15);
			while (ratio > 0)
			{
				text.FontSize /= 2;
				stoke_w /= 2;

				ratio--;
			}

			attr.Size = new Size(stoke_w, stoke_w);
			inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(attr);
		}
	}
}
