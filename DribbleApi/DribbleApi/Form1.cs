using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DribbleApi
{
	public partial class Form1 : Form
	{
		private static readonly string TokenUri = "https://dribbble.com/oauth/token";
		private static readonly string AuthorizationUri = "https://dribbble.com/oauth/authorize";
		private static readonly string ClientIdString = "ee9ff82e0021b503d34d0a3fe210f4d56eb16e0b1df438ba501bac33643b74c4";
		private static readonly string ClientSecretString = "7c213243b7671fca8a99ae62b701fbfee4256feb8f56bca85cfca305f2162d9b";
		public Form1()
		{
			InitializeComponent();
		}
		
		private void Form1_Load(object sender, EventArgs e)
		{
			button1.Enabled = false;
			webBrowser1.Navigated += OnNavigate;
			TcpListener tl = new TcpListener(8080);
			tl.Start();
			ThreadPool.QueueUserWorkItem(o =>
			{
				while (true)
				{
					var tc = tl.AcceptTcpClient();
					ThreadPool.QueueUserWorkItem(u =>
					{
						string Html = "<html><body><h1>It works!</h1></body></html>";
						string Str = "HTTP/1.1 200 OK\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\r\n\r\n" + Html;
						byte[] Buffer = Encoding.ASCII.GetBytes(Str);
						tc.GetStream().Write(Buffer, 0, Buffer.Length);
						tc.Close();
					});
				}
			});
			webBrowser1.Navigate("https://dribbble.com/oauth/authorize?client_id="+ClientIdString);
		}

		private string code;
		private string accessToken;

		string GetJsonResponse(string uri, string method)
		{
			HttpWebRequest request = WebRequest.CreateHttp(uri);

			request.Method = method;
			WebResponse resp = request.GetResponse();
			
			var reader = new StreamReader(resp.GetResponseStream());
			return reader.ReadToEnd();
		}
		void Authorize()
		{
			var uri =
				TokenUri +
				"?client_id=" + ClientIdString +
				"&client_secret=" + ClientSecretString +
				"&code=" + code;
			var obj = JObject.Parse(GetJsonResponse(uri, "POST"));
			accessToken = (string)obj["access_token"];
			button1.Enabled = true;
		}

		void GetBuckets()
		{
			button1.Enabled = false;
			string uri = "https://api.dribbble.com/v1/user/buckets?access_token="+accessToken;

			richTextBox1.Text= GetJsonResponse(uri, "GET");
			groupBox1.Visible = false;
			button1.Enabled = true;

		}
		void OnNavigate(object sender, EventArgs e)
		{
			if (webBrowser1.Url.Host == "localhost" && !webBrowser1.Url.Query.Contains("error"))
			{
				code = webBrowser1.Url.Query.Replace("?code=", "");
				Authorize();
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			GetBuckets();
		}
		
	}
}
