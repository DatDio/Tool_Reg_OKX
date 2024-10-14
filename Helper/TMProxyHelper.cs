using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tool_Reg_OKX.Helper
{
	public class TMProxyHelper
	{
		public static async Task<string> GetNewProxy(string apiKey)
		{
			string proxy = "";

			using (var restClient = new RestClient())
			{
				string respone = "";

				var request = new RestRequest($"https://tmproxy.com/api/proxy/get-new-proxy", Method.Post);

				var body = @"{
						" + "\n" +
						$@"  ""api_key"": ""{apiKey}""
						" + "\n" +
						@"}";
				request.AddStringBody(body, DataFormat.Json);

				for (int i = 0; i < 10; i++)
				{
					RestResponse response = await restClient.ExecuteAsync(request);
					respone = response.Content == null ? "" : response.Content;

					proxy = RegexHelper.GetValueFromGroup("\"https\":\"(.*?)\"", respone);

					if (proxy != "")
					{
						//proxy = RegexHelper.GetValueFromGroup("\"proxy\":\"(.*?)\",", respone);

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
			using (var restClient = new RestClient())
			{
				string respone = "";

				var request = new RestRequest($"https://tmproxy.com/api/proxy/get-current-proxy", Method.Post);
				request.AddHeader("Content-Type", "application/json");
				var body = @"{
						" + "\n" +
						$@"  ""api_key"": ""{apiKey}""
						" + "\n" +
						@"}";
				request.AddStringBody(body, DataFormat.Json);
				for (int i = 0; i < 5; i++)
				{
					RestResponse response = await restClient.ExecuteAsync(request);
					respone = response.Content == null ? "" : response.Content;
					proxy = RegexHelper.GetValueFromGroup("\"https\":\"(.*?)\",", respone);
					if (proxy != "")
					{
						//proxy = RegexHelper.GetValueFromGroup("\"proxy\":\"(.*?)\",", respone);

						return proxy;
					}
					Thread.Sleep(1000);
				}
				return "";
			}
		}
	}
}
