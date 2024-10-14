using OpenQA.Selenium;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Tool_Reg_OKX.Models;

namespace Tool_Reg_OKX.Helper
{
	public class WWProxyHelper
	{
		public const string _baseUrl = "https://wwproxy.com/api/client/";
		//public const string _provinceId = "1";

		public static async Task<string> GetNewProxy(string apiKey)
		{
			string proxy = "";
			var options = new RestClientOptions(_baseUrl)
			{
				MaxTimeout = -1 // No timeout
			};

			using (var restClient = new RestClient(options))
			{
				string respone = "";

				var request = new RestRequest($"proxy/available?key={apiKey}&provinceId=-1", Method.Get);
				

				for (int i = 0; i < 10; i++)
				{
					RestResponse response = await restClient.ExecuteAsync(request);
					respone = response.Content == null ? "" : response.Content;

					var status = RegexHelper.GetValueFromGroup("\"status\":\"(.*?)\",", respone);

					if (status == "OK")
					{
						proxy = RegexHelper.GetValueFromGroup("\"proxy\":\"(.*?)\",", respone);

						return proxy;
					}
					else
					{
						//var pr = GetCurrentProxy(apiKey);
						return await GetCurrentProxy(apiKey);
					}

				}

				return "";
			}
		}
		public static async Task<string> GetCurrentProxy(string apiKey)
		{
			string proxy = "";
			var options = new RestClientOptions(_baseUrl)
			{
				MaxTimeout = -1 // No timeout
			};

			using (var restClient = new RestClient(options))
			{
				string respone = "";

				var request = new RestRequest($"proxy/current?key={apiKey}", Method.Get);
				request.AddHeader("Content-Type", "application/json");

				for(int i = 0; i < 5; i++)
				{
					RestResponse response = await restClient.ExecuteAsync(request);
					respone = response.Content == null ? "" : response.Content;
					var status = RegexHelper.GetValueFromGroup("\"status\":\"(.*?)\",", respone);
					if (status == "OK")
					{
						proxy = RegexHelper.GetValueFromGroup("\"proxy\":\"(.*?)\",", respone);

						return proxy;
					}
					Thread.Sleep(1000);
				}
				return "";
			}
		}
	}
}
