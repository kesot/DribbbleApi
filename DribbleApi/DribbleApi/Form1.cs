using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
			//webBrowser1.Navigate("http://localhost:123/?code=06cef3a5d7699f4989317f10000c192c69a9c20372d305c3ca67fde3a85d0473");
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
						// Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
						string Str = "HTTP/1.1 200 OK\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\r\n\r\n" + Html;
						// Приведем строку к виду массива байт
						byte[] Buffer = Encoding.ASCII.GetBytes(Str);
						// Отправим его клиенту
						//while ((tc.GetStream().ReadByte()) != -1) ;
						tc.GetStream().Write(Buffer, 0, Buffer.Length);
						// Закроем соединение
						tc.Close();
					});
				}
			});
			webBrowser1.Navigate("https://dribbble.com/oauth/authorize?client_id="+ClientIdString);
			
		}

		private string code;
		void Authorize()
		{
			var uri =
				TokenUri +
				"?client_id=" + ClientIdString +
				"&client_secret=" + ClientSecretString +
				"&code=" + code;
			HttpWebRequest request =
				WebRequest.CreateHttp(uri);

			//request.Headers.Add("Authorization", "DirectCrm key=\"zO3d06YKthe9R9h7UIFb\" customerId=\"\" sessionKey=\"\"");
			request.Method = "POST";
			WebResponse resp = request.GetResponse();
			
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
			textBox1.Text = webBrowser1.Url.ToString();
		}
		
	}
}
