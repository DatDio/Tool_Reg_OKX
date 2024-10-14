using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tool_Reg_OKX.Models
{
    public class AccountModel : INotifyPropertyChanged

	{
        public ChromeDriver driver { get; set; } = null;
		private string _email="";
		public string Email
		{
			get => _email;
			set
			{
				_email = value;
				OnPropertyChanged(nameof(Email));
			}
		}

		private string _passWord="";
		public string PassWord
		{
			get => _passWord;
			set
			{
				_passWord = value;
				OnPropertyChanged(nameof(PassWord));
			}
		}

		private string _status = "";
		public string Status
		{
			get => _status;
			set
			{
				_status = value;
				OnPropertyChanged(nameof(Status));
			}
		}

		private string _c_2FA="";
		public string C_2FA
		{
			get => _c_2FA;
			set
			{
				_c_2FA = value;
				OnPropertyChanged(nameof(C_2FA));
			}
		}

		private string _referralCode = "";
		public string ReferralCode
		{
			get => _referralCode;
			set
			{
				_referralCode = value;
				OnPropertyChanged(nameof(ReferralCode));
			}
		}

		private string _linkVery = "";
		public string LinkVery
		{
			get => _linkVery;
			set
			{
				_linkVery = value;
				OnPropertyChanged(nameof(LinkVery));
			}
		}

		private string _proxy="";
		public string Proxy
		{
			get => _proxy;
			set
			{
				_proxy = value;
				OnPropertyChanged(nameof(Proxy));
			}
		}

		private string _c_GPMID = "";
		public string C_GPMID
		{
			get => _c_GPMID;
			set
			{
				_c_GPMID = value;
				OnPropertyChanged(nameof(C_GPMID));
			}
		}

		private string _apiKeyCapMonter = "";
		public string ApiKeyCapMonter
		{
			get => _apiKeyCapMonter;
			set
			{
				_apiKeyCapMonter = value;
				OnPropertyChanged(nameof(ApiKeyCapMonter));
			}
		}

		private string _viOTPApikey="";
		public string ViOTPApikey
		{
			get => _viOTPApikey;
			set
			{
				_viOTPApikey = value;
				OnPropertyChanged(nameof(ViOTPApikey));
			}
		}

		private string _phoneNumber = "";
		public string PhoneNumber
		{
			get => _phoneNumber;
			set
			{
				_phoneNumber = value;
				OnPropertyChanged(nameof(PhoneNumber));
			}
		}

		private string _rePhoneNumber="";
		public string RePhoneNumber
		{
			get => _rePhoneNumber;
			set
			{
				_rePhoneNumber = value;
				OnPropertyChanged(nameof(RePhoneNumber));
			}
		}

		private string _viOTPRequestId = "";
		public string ViOTPRequestId
		{
			get => _viOTPRequestId;
			set
			{
				_viOTPRequestId = value;
				OnPropertyChanged(nameof(ViOTPRequestId));
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
