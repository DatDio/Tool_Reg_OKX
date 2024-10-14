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
	public class ViOTPHelper
	{
		public const string _baseUrl = "https://api.viotp.com/request/getv2?";
		public const string _OkxServiceID = "681";

		public static async Task<bool> RentPhoneNumber(AccountModel accountModel)
		{
			var options = new RestClientOptions()
			{
				MaxTimeout = -1 // No timeout
			};

			using (var restClient = new RestClient(options))
			{
				string respone = "", phoneNumber = "", rePhoneNumber = "", message = "";

				var request = new RestRequest($"https://api.viotp.com/request/getv2?token={accountModel.ViOTPApikey}&serviceId={_OkxServiceID}&network=VIETTEL", Method.Get);
				//request.AddHeader("Content-Type", "application/json");

				RestResponse response = await restClient.ExecuteAsync(request);
				respone = response.Content == null ? "" : response.Content;
				if (respone == "")
				{
					return false;
				}
				var status_code = RegexHelper.GetValueFromGroup("\"status_code\":(.*?),\"", respone);

				if (status_code != "200")
				{
					message = RegexHelper.GetValueFromGroup("\"message\":\"(.*?)\",", respone);
					accountModel.Status = message;
					return false;
				}
				else
				{
					accountModel.PhoneNumber = RegexHelper.GetValueFromGroup("\"phone_number\":\"(.*?)\",", respone);
					accountModel.RePhoneNumber = RegexHelper.GetValueFromGroup("\"re_phone_number\":\"(.*?)\",", respone);
					accountModel.ViOTPRequestId = RegexHelper.GetValueFromGroup("\"request_id\":(.*?),\"", respone);
				}
				return true;
			}
		}
		public static async Task<bool> ReRentPhoneNumber(AccountModel accountModel)
		{
			using (var restClient = new RestClient())
			{
				string respone = "", phoneNumber = "", rePhoneNumber = "", message = "";
				var url = $"https://api.viotp.com/request/getv2?token={accountModel.ViOTPApikey}&serviceId={_OkxServiceID}&number={accountModel.RePhoneNumber}";
				var request = new RestRequest(url, Method.Get);
				//request.AddHeader("Content-Type", "application/json");

				for (int i = 0; i < 60; i++)
				{
					RestResponse response = await restClient.ExecuteAsync(request);
					respone = response.Content == null ? "" : response.Content;
					if (respone == "")
					{
						return false;
					}
					var status_code = RegexHelper.GetValueFromGroup("\"status_code\":(.*?),\"", respone);

					if (status_code != "200")
					{
						message = RegexHelper.GetValueFromGroup("\"message\":\"(.*?)\",", respone);
						accountModel.Status = message;
						if (message == "Số dư quý khách không đủ !")
						{
							return false;
						}
						Thread.Sleep(1000);
					}
					else
					{
						accountModel.ViOTPRequestId = RegexHelper.GetValueFromGroup("\"request_id\":(.*?),\"", respone);

						return true;
					}
				}

				return false;
			}
		}


		public static async Task<string> GetCodeNumber(AccountModel accountModel)
		{

			using (var restClient = new RestClient())
			{
				string respone = "";

				var request = new RestRequest($"https://api.viotp.com/session/getv2?requestId={accountModel.ViOTPRequestId}&token={accountModel.ViOTPApikey}", Method.Get);
				//request.AddHeader("Content-Type", "application/json");

				for (int i = 0; i < 60; i++)
				{
					RestResponse response = await restClient.ExecuteAsync(request);
					respone = response.Content == null ? "" : response.Content;

					var Status = RegexHelper.GetValueFromGroup("\"Status\":(.*?),\"", respone);

					if (Status == "1")
					{
						var code = RegexHelper.GetValueFromGroup("\"Code\":\"(.*?)\",", respone);

						return code;
					}
					else if (Status == "2")
					{
						var reRentPhoneExpired = await ReRentPhoneNumber(accountModel);
						if (!reRentPhoneExpired)
						{
							return "";
						}
					}
					Thread.Sleep(1000);
				}

				return "";
			}
		}
	}
}
