using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using Windows.Graphics.Display;

namespace Better_Kids.Model
{
	class Key_Manager
	{
		// Bing Image
		public const string bing_url = "https://bingapis.azure-api.net/api/v5/images/search";
		public const string bing_key = "d9c59f6186204b4b8c12d4c26e6e84e4";
		public const int bing_cBuffer = 20;

		// Vision
		public const string vision_url = "https://api.projectoxford.ai/vision/v1.0/describe";
		public const string vision_key = "bffac94c45ce4b8ba345446223b2fc6c";

		// Emotion
		public const string emotion_url = "https://api.projectoxford.ai/emotion/v1.0/recognize";
		public const string emotion_key = "b1c3c671c6a545459048cf19f0a131ec";
		public const double emotion_threshold = .6;

		// Linguistics
		//const string text_url = "https://api.projectoxford.ai/linguistics/v1.0/analyze";
		//const string subscription_key2 = "930485c13ba34038852ed9fddcc375a9";
		//const string text_uuid = "4fa79af1-f22c-408d-98bb-b7d7aeef7f04";

		// Youdao Translation
		public const string youdao_url = "http://fanyi.youdao.com/openapi.do";
		public const string youdao_key = "1460273110";
		public const string youdao_keyfrom = "Better-Kids";
	}

	class Oxford_GetImage
	{
		private class JSON_TMP
		{
			public class JSON_TMP2
			{
				public string thumbnailUrl { get; set; }
			}

			public JSON_TMP2[] value { get; set; }
		}

		private static string[] emotions = {
			"anger",
			"contempt",
			"disgust",
			"fear",
			"happiness",
			"neutral",
			"sadness",
			"surprise" };

		private static List<List<string>> emotionurls = new List<List<string>>();

		public static async Task RetrieveUrls()
		{
			List<HttpClient> clients = new List<HttpClient>();
			List<Task<HttpResponseMessage>> tasks = new List<Task<HttpResponseMessage>>();

			foreach (var emo in emotions)
			{
				var client = new HttpClient();
				client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", $"{Key_Manager.bing_key}");

				clients.Add(client);

				var type = emo + " face";
				emotionurls.Add(new List<string>());

				tasks.Add(client.GetAsync($"{Key_Manager.bing_url}?q={type}&count={Key_Manager.bing_cBuffer}"));

				// Avoid over 5 request in a second
				await Task.Delay(1000);
			}

			try
			{
				int i = 0;
				foreach (var task in tasks)
				{
					var response = await task;
					response.EnsureSuccessStatusCode();
					var result = await response.Content.ReadAsAsync<JSON_TMP>();

					foreach (var str in result.value)
						emotionurls[i].Add(str.thumbnailUrl);
					i++;
				}
			}
			catch (Exception e)
			{
				throw new Exception("Retrieve Emotion Images Url Failed: " + e.Message);
			}

			foreach (var client in clients)
				client.Dispose();
		}

		public static string GetImageUrl(string type)
		{
			Random rand = new Random(DateTime.Now.Millisecond + DateTime.Now.Minute);

			for (int i = 0; i < emotions.Count(); i++)
				if (emotions[i] == type)
					return emotionurls[i][rand.Next(emotionurls[i].Count)];
			return null;
		}
	}

	class Emotion
	{
		public static BitmapImage GetImageFromWeb(string type)
		{
			try
			{
				return new BitmapImage(new Uri(Oxford_GetImage.GetImageUrl(type)));
			}
			catch
			{
				return null;
			}
		}
	}

	class CameraView
	{
		private MediaCapture _mediaCapture;
		private bool _isInitialized;
		private readonly DisplayRequest _displayRequest = new DisplayRequest();

		public async Task<MediaCapture> InitializeCameraAsync(bool is_front_cam)
		{
			Debug.WriteLine("InitializeCameraAsync");

			DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

			// Get available devices for capturing pictures
			var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

			DeviceInformation desiredDevice;
			// Get the desired camera by panel
			if (is_front_cam)
			{
				desiredDevice = allVideoDevices.FirstOrDefault(
					x => x.EnclosureLocation != null &&
					x.EnclosureLocation.Panel == Panel.Front);
			}
			else
			{
				desiredDevice = allVideoDevices.FirstOrDefault(
					x => x.EnclosureLocation != null &&
					x.EnclosureLocation.Panel == Panel.Back);
			}

			// If there is no device mounted on the desired panel, return the first device found
			var cameraDevice = desiredDevice ?? allVideoDevices.FirstOrDefault();

			if (cameraDevice == null)
			{
				throw new Exception("找不到相机");
			}

			// Create MediaCapture and its settings
			if (_mediaCapture == null)
			{
				_mediaCapture = new MediaCapture();
				_mediaCapture.Failed += MediaCapture_Failed;

				var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };

				// Initialize MediaCapture
				try
				{
					await _mediaCapture.InitializeAsync(settings);
					_isInitialized = true;
				}
				catch (UnauthorizedAccessException)
				{
					throw new Exception("请在【设置-隐私】页面打开相机权限");
				}
			}
			else
				_isInitialized = true;

			// If initialization succeeded, start the preview
			if (_isInitialized)
			{
				//// Figure out where the camera is located
				//if (cameraDevice.EnclosureLocation == null || cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Unknown)
				//{
				//	// No information on the location of the camera, assume it's an external camera, not integrated on the device
				//	_externalCamera = true;
				//}
				//else
				//{
				//	// Camera is fixed on the device
				//	_externalCamera = false;

				//	// Only mirror the preview if the camera is on the front panel
				//	_mirroringPreview = (cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front);
				//}

				// Prevent the device from sleeping while the preview is running
				_displayRequest.RequestActive();

				// Set the preview source in the UI and mirror it if necessary
				return _mediaCapture;

				// Remark: always true
				//PreviewControl.FlowDirection = _mirroringPreview ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
			}
			else
				return null;
		}

		public async Task UninitializeCameraAsync()
		{
			DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;

			await _mediaCapture.StopPreviewAsync();
			_mediaCapture.Failed -= MediaCapture_Failed;
			_mediaCapture.Dispose();
			_mediaCapture = null;

			_displayRequest.RequestRelease();

			_isInitialized = false;
		}

		public async Task Start()
		{
			// Start the preview
			await _mediaCapture.StartPreviewAsync();
		}

		private void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
		{
			Debug.WriteLine("Camera Failed.");
		}

		private class JSON_TMP
		{
			public class Scores
			{
				public double angle { get; set; }
				public double contempt { get; set; }
				public double disgust { get; set; }
				public double fear { get; set; }
				public double happiness { get; set; }
				public double neutral { get; set; }
				public double sadness { get; set; }
				public double suprise { get; set; }
			}
			public Scores scores { get; set; }

			public KeyValuePair<string, double> get()
			{
				List<KeyValuePair<string, double>> array = new List<KeyValuePair<string, double>>();
				array.Add(new KeyValuePair<string, double>("angle", scores.angle));
				array.Add(new KeyValuePair<string, double>("contempt", scores.contempt));
				array.Add(new KeyValuePair<string, double>("disgust", scores.disgust));
				array.Add(new KeyValuePair<string, double>("fear", scores.fear));
				array.Add(new KeyValuePair<string, double>("happiness", scores.happiness));
				array.Add(new KeyValuePair<string, double>("neutral", scores.neutral));
				array.Add(new KeyValuePair<string, double>("sadness", scores.sadness));
				array.Add(new KeyValuePair<string, double>("suprise", scores.suprise));

				KeyValuePair<string, double> ret = array[0];
				foreach (var i in array)
				{
					if (i.Value > ret.Value)
						ret = i;
				}
				return ret;
			}
		}

		public async Task<Tuple<string, double>> Click()
		{
			var stream = new InMemoryRandomAccessStream();
			await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

			using (var client = new HttpClient())
			using (var inputStream = stream)
			{
				var decoder = await BitmapDecoder.CreateAsync(inputStream);
				var outputStream = new InMemoryRandomAccessStream();
				var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);
				await encoder.FlushAsync();

				client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", $"{Key_Manager.emotion_key}");

				byte[] buffer = new byte[outputStream.AsStream().Length];
				outputStream.AsStream().Read(buffer, 0, buffer.Length);
				var content = new ByteArrayContent(buffer);
				content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

				var http_result = await client.PostAsync($"{Key_Manager.emotion_url}", content);
				try
				{
					http_result.EnsureSuccessStatusCode();
					var json_result = await http_result.Content.ReadAsAsync<JSON_TMP[]>();

					var result = json_result[0].get();

					if (result.Value > Key_Manager.emotion_threshold)
						return new Tuple<string, double>(result.Key, result.Value);
					else
						return null;
				}
				catch (Exception e)
				{
					throw new Exception("Emotion API Http Request Failed: " + e.Message);
				}
			}
		}

		//private class JSON_TMP_2
		//{
		//	public class Desc
		//	{
		//		public class Capt
		//		{
		//			public string text { get; set; }
		//		}
		//		public Capt[] catptions { get; set; }
		//	}
		//	public Desc description { get; set; }
		//}

		public class JSON_TMP_2
		{
			public class Description
			{
				public class Caption
				{
					public string text { get; set; }
					public double confidence { get; set; }
				}

				public Caption[] captions { get; set; }
			}

			public Description description { get; set; }
		}

		//private class JSON_TMP_3
		//{
		//	public string[][] result { get; set; }
		//}

		private class JSON_TMP_4
		{
			public string[] translation { get; set; }
		}

		public async Task<Tuple<string, string>> Click2()
		{
			var stream = new InMemoryRandomAccessStream();
			await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

			string desc;

			using (var client = new HttpClient())
			using (var inputStream = stream)
			{
				var decoder = await BitmapDecoder.CreateAsync(inputStream);
				var outputStream = new InMemoryRandomAccessStream();
				var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);
				await encoder.FlushAsync();

				client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", $"{Key_Manager.vision_key}");

				byte[] buffer = new byte[outputStream.AsStream().Length];
				outputStream.AsStream().Read(buffer, 0, buffer.Length);
				var content = new ByteArrayContent(buffer);
				content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

				var http_result = await client.PostAsync($"{Key_Manager.vision_url}", content);
				try
				{
					http_result.EnsureSuccessStatusCode();
					var json_result = await http_result.Content.ReadAsAsync<JSON_TMP_2>();

					desc = json_result.description.captions[0].text;
				}
				catch (Exception e)
				{
					throw new Exception("Vision API Http Request Failed: " + e.Message);
				}
			}

			//string key_entity = "";

			//using (var client = new HttpClient())
			//{
			//	client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", $"{subscription_key2}");

			//	var content = new StringContent($"{{\"language\":\"en\",\"analyzerIds\":[\"{text_uuid}\"],\"text\":\"{desc}\"}}");
			//	content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

			//	var http_result = await client.PostAsync($"{text_url}", content);
			//	try
			//	{
			//		http_result.EnsureSuccessStatusCode();
			//		var json_result = await http_result.Content.ReadAsAsync<JSON_TMP_3[]>();

			//		var desc_cpy = desc.Split(new char[] { ' ', ',', '.' });
			//		int index = 0;

			//		for (var i = 0; i < json_result[0].result.Count(); i++)
			//			for (var j = 0; j < json_result[0].result[i].Count(); j++)
			//				if (json_result[0].result[i][j].Equals("NN"))
			//				{
			//					key_entity = desc_cpy[index];

			//					// Find the first entity
			//					break;
			//				}
			//				else
			//					index++;
			//	}
			//	catch (Exception e)
			//	{
			//		throw new Exception("Linguistics API Http Request Failed: " + e.Message);
			//	}
			//}

			//if (key_entity == "")
			//	return null;

			using (var client = new HttpClient())
			{
				// Use the whole line
				var key_entity = desc;

				var http_result = await client.GetAsync($"{Key_Manager.youdao_url}?keyfrom={Key_Manager.youdao_keyfrom}" +
					$"&key={Key_Manager.youdao_key}&type=data&doctype=json&version=1.1&q={key_entity}");
				try
				{
					http_result.EnsureSuccessStatusCode();
					var json_result = await http_result.Content.ReadAsAsync<JSON_TMP_4>();

					return new Tuple<string, string>(key_entity, json_result.translation[0]);
				}
				catch (Exception e)
				{
					throw new Exception("Translation API Http Request Failed: " + e.Message);
				}
			}
		}
	}
}