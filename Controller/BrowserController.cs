using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tool_Reg_OKX.Helper;
using Tool_Reg_OKX.Models;
using Tool_Reg_OKX.ViewModel;

namespace Tool_Reg_OKX.Controller
{
	public class BrowserController
	{
		ChromeDriver driver;
		AccountModel account;
		//public string createdProfileId;
		private GPMLoginAPI api;
		public BrowserController(AccountModel account)
		{
			this.account = account;
		}
		public BrowserController()
		{

		}
		public async Task<ChromeDriver> OpenChromeGpm(string apiGpm,
			string createdProfileId,
			string name, string groupID, string useragent = "",
			double scale = 0.7, string proxy = "",
			bool hideBrowser = false,
			string position = "0,0")
		{
			//this.createdProfileId = "";

			api = new GPMLoginAPI(apiGpm);

		CreateProfile:
			if (createdProfileId == "")
			{
				var createdResult = api.Create(name, isNoiseCanvas: true);

				if (createdResult != null)
				{
					var status = Convert.ToBoolean(createdResult["status"]);
					if (status)
					{
						createdProfileId = Convert.ToString(createdResult["profile_id"]);
						account.C_GPMID = createdProfileId;
						//FunctionHelper.EditValueColumn(account, "C_GPMID", createdProfileId, true);
					}

				}

				//this.createdProfileId = createdProfileId;
				//Console.WriteLine("Created profile ID: " + createdProfileId);
			}

			//if (string.IsNullOrEmpty(position))
			//{
			//	position = GetNewPosition(800, 800, scale);
			//}

			//string extensionPath = Path.GetFullPath("CapMonsterExtension");
			//if (!Directory.Exists(extensionPath))
			//{
			//	// Log or handle the error that extension path does not exist
			//	//throw new Exception("Extension folder not found: " + extensionPath);
			//	MessageBox.Show("Ko tìm thấy đường dẫn");
			//}


			var arg = $"--window-position={position} --window-size=800,800 --force-device-scale-factor={scale}";

			if (useragent != "")
			{
				arg += $" --user-agent=\"{useragent}\"";
			}

			if (hideBrowser)
			{
				arg += $" --headless";
			}

			await api.UpdateProfile(createdProfileId, groupID: groupID, proxy: proxy.Replace("http://", ""));

			var startedResult = api.Start(createdProfileId, null, arg);

			var browserLocation = Convert.ToString(startedResult["browser_location"]);
			var seleniumRemoteDebugAddress = Convert.ToString(startedResult["selenium_remote_debug_address"]);
			var gpmDriverPath = Convert.ToString(startedResult["selenium_driver_location"]);

			//if (gpmDriverPath == "")
			//{
			//	//createdProfileId = "";
			//	//goto CreateProfile;
			//	return null;
			//}

			var gpmDriverFileInfo = new FileInfo(gpmDriverPath);

			var service = ChromeDriverService.CreateDefaultService(gpmDriverFileInfo.DirectoryName, gpmDriverFileInfo.Name);
			service.HideCommandPromptWindow = true;
			var options = new ChromeOptions
			{
				BinaryLocation = browserLocation,
				DebuggerAddress = seleniumRemoteDebugAddress
			};

			driver = new ChromeDriver(service, options);

			return driver;
		}

		public static string GetNewPosition(int w, int h, double scale = 1)
		{
			var current_size = "";

			lock (MainViewModel.lockChrome)
			{
				current_size = $"{MainViewModel.CurrentWidth},{MainViewModel.CurrentHeight}";

				MainViewModel.CurrentWidth += w;

				if (MainViewModel.CurrentWidth + w >= SystemParameters.PrimaryScreenWidth / scale)
				{
					MainViewModel.CurrentWidth = 0;
					MainViewModel.CurrentHeight += h;
				}

				if (MainViewModel.CurrentHeight + h >= SystemParameters.PrimaryScreenHeight / scale)
				{
					MainViewModel.CurrentWidth = 0;
					MainViewModel.CurrentHeight = 0;
				}
			}

			return current_size;
		}
		public void CloseChrome()
		{
			try
			{
				var windowHandles = driver.WindowHandles;
				foreach (var handle in windowHandles)
				{
					driver.SwitchTo().Window(handle);
					driver.Close();
				}

				driver.Quit();
			}
			catch { }
			try
			{
				api.Stop(account.C_GPMID);
			}
			catch
			{

			}
		}
	}
}
