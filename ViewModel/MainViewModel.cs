using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tool_Reg_OKX.Controller;
using Tool_Reg_OKX.Helper;
using Tool_Reg_OKX.Models;
using Tool_Reg_OKX.Settings;

namespace Tool_Reg_OKX.ViewModel
{
	public class MainViewModel : BaseViewModel
	{
		private const string settingsFilePath = "appsettings.json";
		public ObservableCollection<AccountModel> Accounts { get; set; } = new ObservableCollection<AccountModel>();
		public ObservableCollection<GroupModel> Groups { get; set; }
		public bool IsRunning = false;
		public static int CurrentWidth, CurrentHeight;
		public static object lockChrome;
		public static object lockSave;
		public static object lockApiKeyWWProxy;
		public List<string> apiKeyListWWPRoxy;
		private string apiKeyViOTP, apiUrlGPM, groupId, apiKeyCapMonster;
		GPMLoginAPI GPMLoginAPI;
		//private int Success, Fail;

		public Random random = new Random();
		private int _success = 0;
		public int Success
		{
			get { return _success; }
			set
			{
				_success = value;
				OnPropertyChanged(nameof(Success));
			}
		}
		private string _selectedGroupID;
		public string SelectedGroupID
		{
			get => _selectedGroupID;
			set
			{
				_selectedGroupID = value;
				OnPropertyChanged(nameof(SelectedGroupID));
			}
		}

		private int _fail = 0;
		public int Fail
		{
			get { return _fail; }
			set
			{
				_fail = value;
				OnPropertyChanged(nameof(Fail));
			}
		}

		private double _scale;
		public double Scale
		{
			get { return _scale; }
			set
			{
				_scale = value;
				OnPropertyChanged(nameof(Scale));
			}
		}
		private int _thread;
		public int Thread
		{
			get { return _thread; }
			set
			{
				_thread = value;
				OnPropertyChanged(nameof(Thread));
			}
		}

		private int _threadRunning = 0;
		public int ThreadRunning
		{
			get { return _threadRunning; }
			set
			{
				_threadRunning = value;
				OnPropertyChanged(nameof(ThreadRunning));
			}
		}

		private string _apiKeyViOTP = "";
		public string ApiKeyViOTP
		{
			get { return _apiKeyViOTP; }
			set
			{
				_apiKeyViOTP = value;
				OnPropertyChanged(nameof(ApiKeyViOTP));
			}
		}

		private string _apiKeyCapMonster = "";
		public string ApiKeyCapMonster
		{
			get { return _apiKeyCapMonster; }
			set
			{
				_apiKeyCapMonster = value;
				OnPropertyChanged(nameof(ApiKeyCapMonster));
			}
		}

		private string _apiUrlGPM;
		public string ApiUrlGPM
		{
			get { return _apiUrlGPM; }
			set
			{
				_apiUrlGPM = value;
				OnPropertyChanged(nameof(ApiUrlGPM));
			}
		}

		private string _apiKeyWWProxy = "";
		public string ApiKeyWWProxy
		{
			get { return _apiKeyWWProxy; }
			set
			{
				_apiKeyWWProxy = value;
				OnPropertyChanged(nameof(ApiKeyWWProxy));
			}
		}

		private ObservableCollection<string> referralList;

		public ObservableCollection<string> ReferralList
		{
			get => referralList;
			set
			{
				referralList = value;
				//OnPropertyChanged();
				OnPropertyChanged(nameof(ReferralCount));  // Cập nhật khi danh sách thay đổi
			}
		}

		// Thuộc tính để binding với UI
		public int ReferralCount => ReferralList.Count;

		public ICommand StartCommand { get; set; }
		public ICommand StopCommand { get; set; }
		public ICommand OpenOutputCommand { get; set; }
		public ICommand OpenReferraltCommand { get; set; }
		public ICommand ReloadGroupCommand { get; set; }
		public MainViewModel()
		{
			LoadSettings();
			if (!Directory.Exists("Output"))
			{
				Directory.CreateDirectory("Output");
			}
			if (!Directory.Exists("Input"))
			{
				Directory.CreateDirectory("Input");
			}
			if (!File.Exists("Input/Referral.txt"))
			{
				File.Create("Input/Referral.txt").Dispose();
			}
			if (!File.Exists("Output/Output.txt"))
			{
				File.Create("Output/Output.txt").Dispose();
			}
			lockChrome = new object();
			lockApiKeyWWProxy = new object();
			lockSave = new object();
			ReferralList = new ObservableCollection<string>(File.ReadAllLines(Path.GetFullPath("Input/Referral.txt")));
			ReferralList.CollectionChanged += (sender, e) =>
			{
				OnPropertyChanged(nameof(ReferralCount));
			};
			GPMLoginAPI = new GPMLoginAPI(_apiUrlGPM);
			Groups = GPMLoginAPI.GetAllGroups();
			#region StartCommand
			StartCommand = new RelayCommand<object>(
				(a) =>
				{

					if (String.IsNullOrEmpty(_apiKeyWWProxy)
					|| String.IsNullOrEmpty(SelectedGroupID)
					|| String.IsNullOrEmpty(_apiKeyViOTP)
					|| String.IsNullOrEmpty(_apiUrlGPM) || IsRunning)
					{
						return false;
					}
					return true;
				},
				async (a) =>
				{
					SaveSettings();
					Accounts.Clear();
					// Xóa nội dung cũ của ReferralList mà không khởi tạo lại ObservableCollection
					ReferralList.Clear();

					// Đọc dữ liệu mới từ file và thêm vào ReferralList
					var referralData = File.ReadAllLines(Path.GetFullPath("Input/Referral.txt")).ToList();
					foreach (var referral in referralData)
					{
						ReferralList.Add(referral);
					}

					apiKeyListWWPRoxy = ApiKeyWWProxy
			.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
			.Select(apiKey => apiKey.Trim())
			.Where(apiKey => !string.IsNullOrWhiteSpace(apiKey))
			.ToList();
					apiKeyViOTP = _apiKeyViOTP;
					apiKeyCapMonster = _apiKeyCapMonster;
					ThreadRunning = _thread;
					IsRunning = true;
					CurrentWidth = 0;
					CurrentHeight = 0;
					Success = 0;
					Fail = 0;

					groupId = SelectedGroupID;

					List<Task> tasks = new List<Task>();

					for (int i = 0; i < _thread; i++)
					{
						Task task = Task.Run(() => OneThread());
						tasks.Add(task);
					}

					// Chờ tất cả các tác vụ hoàn thành (nếu cần)
					await Task.WhenAll(tasks);
					MessageBox.Show("Tool đã dừng");
					IsRunning = false;
				});
			#endregion

			#region StopCommand
			StopCommand = new RelayCommand<object>(
				(a) =>
				{
					if (IsRunning)
					{
						return true;
					}
					return false;
				},
				async (a) =>
				{
					IsRunning = false;
				});
			#endregion


			#region ReloadGroupCommand
			ReloadGroupCommand = new RelayCommand<object>(
				(a) =>
				{
					return true;
				},
				async (a) =>
				{
					Groups.Clear();
					var NewGroups = GPMLoginAPI.GetAllGroups();
					foreach (var group in NewGroups)
					{
						Groups.Add(group);
					}
				});
			#endregion
			#region OpenOutputCommand
			OpenOutputCommand = new RelayCommand<object>(
				(a) =>
				{
					return true;
				},
				async (a) =>
				{
					string folderPath = Path.GetFullPath("Output/Output.txt");
					try
					{
						// Mở thư mục sử dụng Process.Start
						Process.Start(new ProcessStartInfo
						{
							FileName = folderPath,
							UseShellExecute = true // Thiết lập UseShellExecute để mở thư mục
						});
					}
					catch (System.Exception ex)
					{
						MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				});
			#endregion

			#region OpenReferraltCommand
			OpenReferraltCommand = new RelayCommand<object>(
				(a) =>
				{
					return true;
				},
				async (a) =>
				{
					string folderPath = Path.GetFullPath("Input/Referral.txt");
					try
					{
						// Mở thư mục sử dụng Process.Start
						Process.Start(new ProcessStartInfo
						{
							FileName = folderPath,
							UseShellExecute = true // Thiết lập UseShellExecute để mở thư mục
						});
					}
					catch (System.Exception ex)
					{
						MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				});
			#endregion
		}
		private async Task OneThread()
		{
			string position = "", name = "", note = "";
			AccountModel account = null;
			BrowserController browserController = null;
			OKXController okxController = null;
			string apiKeyWWProxy = "";
			//GPMLoginAPI GPMLoginAPI = new GPMLoginAPI(_apiUrlGPM);
			lock (lockApiKeyWWProxy)
			{
				if (apiKeyListWWPRoxy.Count() == 0)
				{
					goto endThread;
				}
				apiKeyWWProxy = apiKeyListWWPRoxy[0];
				apiKeyListWWPRoxy.RemoveAt(0);
				//apiKeyListWWPRoxy.Add(apiKeyWWProxy);
			}
			lock (lockChrome)
			{
				position = BrowserController.GetNewPosition(800, 800, _scale);
			}
			while (IsRunning)
			{
				account = new AccountModel()
				{
					Email = FunctionHelper.GenerateRandomEmail(),
					ViOTPApikey = apiKeyViOTP,
					ApiKeyCapMonter = apiKeyCapMonster
				};
				lock (lockSave)
				{
					//if (ReferralList.Count == 0)
					//{
					//	IncrementFail();
					//	account.Status = "Hết referral code!";
					//	break;
					//}

					if (ReferralList.Count > 0)
					{
						account.ReferralCode = ReferralList[0];
						ReferralList.RemoveAt(0);
						File.WriteAllLines(Path.GetFullPath("Input/Referral.txt"), ReferralList);
					}
				}

				//account.Proxy = await TMProxyHelper.GetNewProxy(apiKeyWWProxy);
				account.Proxy = await WWProxyHelper.GetNewProxy(apiKeyWWProxy);
				if (account.Proxy == "")
				{
					account.Status = "Lấy proxy lỗi!";
					IncrementFail();
					lock (lockSave)
					{
						ReferralList.Insert(0, account.ReferralCode);
						File.WriteAllLines(Path.GetFullPath("Input/Referral.txt"), ReferralList);
					}
					goto finish;
				}
				Application.Current.Dispatcher.Invoke(() =>
				{
					Accounts.Insert(0, account);
				});
				browserController = new BrowserController(account);
				okxController = new OKXController(account);
				try
				{
					account.driver = await browserController.OpenChromeGpm(_apiUrlGPM, account.C_GPMID,
						account.Email, groupId, "", Scale,
						account.Proxy, position: position);
				}
				catch
				{
					account.Status = "Mở GPM lỗi!";
					IncrementFail();
					lock (lockSave)
					{
						ReferralList.Insert(0, account.ReferralCode);
						File.WriteAllLines(Path.GetFullPath("Input/Referral.txt"), ReferralList);
					}
					goto finish;
				}
				var resultStatus = await okxController.RegOkx();
				if (resultStatus == ResultModel.Success)
				{
					IncrementSuccess();
					lock (lockSave)
					{
						File.AppendAllText("Output/Output.txt", $"{account.Email}|{account.PassWord}|{account.C_2FA}|{account.LinkVery}" + "\n");
					}
					name = $"{account.Email}|{account.PassWord}|{account.C_2FA}";
					await GPMLoginAPI.UpdateProfile(account.C_GPMID, groupId, name, account.C_2FA);

				}
				else if (resultStatus == ResultModel.Fail)
				{
					IncrementFail();
					lock (lockSave)
					{
						if (!String.IsNullOrEmpty(account.ReferralCode))
						{
							ReferralList.Insert(0, account.ReferralCode);
							File.WriteAllLines(Path.GetFullPath("Input/Referral.txt"), ReferralList);
						}
					}
					GPMLoginAPI.Delete(account.C_GPMID);
				}
				else if (resultStatus == ResultModel.Get2FaFail)
				{
					IncrementFail();
					lock (lockSave)
					{
						if (!String.IsNullOrEmpty(account.ReferralCode))
						{
							ReferralList.Insert(0, account.ReferralCode);
							File.WriteAllLines(Path.GetFullPath("Input/Referral.txt"), ReferralList);
						}
					}
					GPMLoginAPI.Delete(account.C_GPMID);
					//File.AppendAllText("Output/OutputGet2FAFail.txt", $"{account.Email}|{account.PassWord}|{account.C_2FA}|{account.LinkVery}" + "\n");
				}
				else if (resultStatus == ResultModel.RunOutOfBalance)
				{
					IncrementFail();
					GPMLoginAPI.Delete(account.C_GPMID);
					lock (lockSave)
					{
						if (!String.IsNullOrEmpty(account.ReferralCode))
						{
							ReferralList.Insert(0, account.ReferralCode);
							File.WriteAllLines(Path.GetFullPath("Input/Referral.txt"), ReferralList);
						}
					}
					browserController.CloseChrome();
					break;
					//File.AppendAllText("Output/OutputGet2FAFail.txt", $"{account.Email}|{account.PassWord}|{account.C_2FA}|{account.LinkVery}" + "\n");
				}
			finish:
				browserController.CloseChrome();
				Application.Current.Dispatcher.Invoke(() =>
				{
					Accounts.Remove(account);
				});
			}
		endThread:
			DecrementThread();
		}

		private void SaveSettings()
		{
			var settings = new AppSettings
			{
				ApiKeyViOTP = ApiKeyViOTP,
				ApiUrlGPM = ApiUrlGPM,
				ApiKeyWWProxy = ApiKeyWWProxy,
				ApiKeyCapMonster = ApiKeyCapMonster,
				Scale = Scale,
				Thread = Thread,
			};
			var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
			File.WriteAllText(settingsFilePath, json);
		}
		private void LoadSettings()
		{
			if (File.Exists(settingsFilePath))
			{
				var json = File.ReadAllText(settingsFilePath);
				var settings = JsonConvert.DeserializeObject<AppSettings>(json);
				ApiKeyViOTP = settings.ApiKeyViOTP;
				ApiUrlGPM = settings.ApiUrlGPM;
				ApiKeyWWProxy = settings.ApiKeyWWProxy;
				ApiKeyCapMonster = settings.ApiKeyCapMonster;
				Scale = settings.Scale;
				Thread = settings.Thread;
			}
		}

		public void IncrementSuccess()
		{
			// Tăng giá trị của _success
			int newValue = Interlocked.Increment(ref _success);

			// Cập nhật giá trị Success
			Success = newValue; // Cập nhật trực tiếp thuộc tính
		}
		public void IncrementFail()
		{
			// Tăng giá trị của _success
			int newValue = Interlocked.Increment(ref _fail);

			// Cập nhật giá trị Success
			Fail = newValue; // Cập nhật trực tiếp thuộc tính
		}
		public void DecrementThread()
		{
			// Tăng giá trị của _success
			int newValue = Interlocked.Decrement(ref _threadRunning);

			// Cập nhật giá trị Success
			ThreadRunning = newValue; // Cập nhật trực tiếp thuộc tính
		}
	}
}
