using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tool_Reg_OKX.Helper;
using Tool_Reg_OKX.Models;

namespace Tool_Reg_OKX.Controller
{
	public class OKXController
	{
		AccountModel account;
		Random random;
		public string url;
		public OKXController(AccountModel account)
		{
			this.account = account;
			random = new Random();
		}
		public async Task<ResultModel> RegOkx()
		{
			#region Nhập ApiKey capMonter
			account.Status = "Đến trang extension...";
			try
			{
				account.driver.Url = "chrome-extension://pabjfbciaedomjjfelfafejkppknjleh/popup.html";
			}
			catch
			{
				return ResultModel.Fail;
			}
			Thread.Sleep(2000);
			account.Status = "Nhập ApiKey";
			if (!SeleniumHelper.SendKeys(account.driver, By.Id("client-key-input"), account.ApiKeyCapMonter))
			{
				return ResultModel.Fail;
			}
			Thread.Sleep(2000);
			account.Status = "Bấm addFund";
			if (!SeleniumHelper.Click(account.driver, By.Id("client-key-save-btn")))
			{
				return ResultModel.Fail;
			}
			Thread.Sleep(4000);
			if (SeleniumHelper.GetTextElement(account.driver, By.Id("client-balance-or-error-text")).Contains("Empty key"))
			{
				return ResultModel.Fail;
			}
			#endregion


			int countPerForm = 0;
		reStart:
			countPerForm++;
			string url = "", codeMail = "", codeNumber = "";
			account.Status = "Đến trang đăng kí ...";

			try
			{
				account.driver.Url = "https://www.okx.com/vi/account/register?action=header_register_btn";
			}
			catch
			{
				return ResultModel.Fail;
			}

			Thread.Sleep(2000);
			account.Status = "Chọn quốc gia Việt Nam ...";
			for (int i = 1; i <= 3; i++)
			{
				if (SeleniumHelper.Click(account.driver, By.ClassName("login-input-suffix")))
				{
					break;
				}
				if (i >= 3)
				{
					return ResultModel.Fail;
				}
			}

			Thread.Sleep(3000);
			if (!SeleniumHelper.SendKeys(account.driver, By.ClassName("login-select-search-ellipsis"), "Việt Nam"))
			{
				return ResultModel.Fail;
			}
			Thread.Sleep(random.Next(1000, 4000));
			if (!SeleniumHelper.WaitElement(account.driver, By.ClassName("optionItem-displayName")))
			{
				return ResultModel.Fail;
			}
			//var element = account.driver.FindElement(By.ClassName("optionItem-displayName"));
			var text = SeleniumHelper.GetTextElement(account.driver, By.ClassName("optionItem-displayName"));
			if (text != "Việt Nam")
			{
				return ResultModel.Fail;
			}

			Thread.Sleep(2000);
			account.Status = "Chọn checkbox...";
			if (!SeleniumHelper.Click(account.driver, By.ClassName("optionItem-displayName")))
			{
				return ResultModel.Fail;
			}
			Thread.Sleep(1000);
			if (!SeleniumHelper.Click(account.driver, By.ClassName("login-checkbox-input")))
			{
				return ResultModel.Fail;
			}
			account.Status = "Bấm submit ...";
			Thread.Sleep(1000);
			if (!SeleniumHelper.Click(account.driver, By.Id("register-country-submit")))
			{
				return ResultModel.Fail;
			}
			Thread.Sleep(1000);
			if (!SeleniumHelper.Click(account.driver, By.CssSelector("button[data-testid=\"okd-dialog-confirm-btn\"]")))
			{
				return ResultModel.Fail;
			}
			account.Status = "Nhập Email ...";
			Thread.Sleep(random.Next(1000, 4000));
			if (!SeleniumHelper.SendKeys(account.driver, By.Id("email-autocomplete-input"), account.Email))
			{
				return ResultModel.Fail;
			}
			Thread.Sleep(2000);

			//Mã giới thiệu
			if (!String.IsNullOrEmpty(account.ReferralCode))
			{
				account.Status = "Nhập mã giới thiệu ...";
				if (!SeleniumHelper.SendKeys(account.driver, By.Id("invite-code-input"), account.ReferralCode))
				{
					return ResultModel.Fail;
				}
			}

			Thread.Sleep(1000);
			account.Status = "Đang check error ...";
			if (SeleniumHelper.WaitElement(account.driver, By.ClassName("login-input-error"), 5))
			{
				return ResultModel.Fail;
			}

			//Bấm submit tối đa 3 lần nếu gặp captcha
			int countReStartSubmit = 0;
		reStartSubmit:
			countReStartSubmit++;
			account.Status = $"Bấm submit lần {countReStartSubmit}...";
			if (!SeleniumHelper.Click(account.driver, By.Id("register-submit-btn")))
			{
				return ResultModel.Fail;
			}
			account.Status = $"Check captcha lần {countReStartSubmit}...";
			if (SeleniumHelper.WaitElement(account.driver, By.ClassName("login-dialog-title-block"), 5))
			{
				account.Status = $"Đợi giải captcha lần {countReStartSubmit}...";
				for (int i = 0; i < 3; i++)
				{
					if (SeleniumHelper.WaitElementHidden(account.driver, By.ClassName("login-dialog-title-block"), 5))
					{
						break;
					}
				}
				//return ResultModel.Fail;
			}
			account.Status = $"Check erorr lần {countReStartSubmit}...";
			if (!SeleniumHelper.WaitElementHidden(account.driver, By.Id("register-submit-btn")))
			{
				if (countReStartSubmit == 3)
				{
					return ResultModel.Fail;
				}
				goto reStartSubmit;
			}
			//Lấy code mail
			account.Status = "Đang đợi code email...";
			codeMail = await FunctionHelper.GetCodeNumber(account.Email);
			if (codeMail == "")
			{
				return ResultModel.Fail;
			}
			account.Status = "Nhập code email ...";
			Thread.Sleep(2000);
			if (!SeleniumHelper.SendKeys(account.driver, By.CssSelector("input[type=\"text\"]"), codeMail))
			{
				return ResultModel.Fail;
			}
			if (!SeleniumHelper.WaitElement(account.driver, By.ClassName("login-label-text")))
			{
				return ResultModel.Fail;
			}
			account.Status = "Đang thuê số ViOTP ...";
			//var inputelement = account.driver.FindElements(By.CssSelector("input[type=\"text\"]"));
			if (account.PhoneNumber == "")
			{
				var rentPhoneStatus = await ViOTPHelper.RentPhoneNumber(account);
				if (!rentPhoneStatus)
				{
					account.Status = "Thuê số thất bại ...";
					return ResultModel.Fail;
				}
			}

			account.Status = "Nhập số ...";
			if (!SeleniumHelper.SendKeys(account.driver, By.ClassName("login-input-input"), account.PhoneNumber))
			{
				return ResultModel.Fail;
			}
			account.Status = "Bấm submit ...";
			Thread.Sleep(3000);
			if (!SeleniumHelper.Click(account.driver, By.Id("register-submit-btn")))
			{
				return ResultModel.Fail;
			}

			Thread.Sleep(2000);
			for (int i = 1; i <= 2; i++)
			{
				account.Status = $"Đang lấy code number lần {1}...";
				codeNumber = await ViOTPHelper.GetCodeNumber(account);

				if (codeNumber != "")
				{
					break;
				}

				if (i == 2)
				{
					return ResultModel.Fail;
				}
				//Bấm gửi lại code
				account.Status = "Bấm gửi lại code ...";
				if (!SeleniumHelper.Click(account.driver, By.ClassName("login-hyperlink-no-hover-underline")))
				{
					return ResultModel.Fail;
				}
				if (SeleniumHelper.WaitElement(account.driver, By.ClassName("geetest_container"), 8))
				{
					account.Status = "Có captcha";
					for (int j = 0; j < 3; j++)
					{
						if (SeleniumHelper.WaitElement(account.driver, By.ClassName("geetest_container"), 5))
						{
							Thread.Sleep(5000);
						}
					}
					if (!SeleniumHelper.WaitElementHidden(account.driver, By.ClassName("geetest_container"), 8))
					{
						return ResultModel.Fail;
					}
				}

			}

			account.Status = "Đang nhập code ...";
			if (!SeleniumHelper.SendKeys(account.driver, By.ClassName("login-input-input"), codeNumber))
			{
				return ResultModel.Fail;
			}
			account.Status = "Đang nhập password ...";
			Thread.Sleep(2000);
			account.PassWord = FunctionHelper.GenerateRandomPassword(9, 12);
			if (!SeleniumHelper.SendKeys(account.driver, By.ClassName("login-input-input"), account.PassWord))
			{
				return ResultModel.Fail;
			}
			account.Status = "Bấm đăng kí ...";
			url = account.driver.Url;
			Thread.Sleep(2000);
			if (!SeleniumHelper.Click(account.driver, By.Id("register-submit")))
			{
				return ResultModel.Fail;
			}
			if (SeleniumHelper.WaitElement(account.driver, By.ClassName("login-notification-title-box"), 8))
			{
				account.Status = "Fail, Ip bị chặn!";
				if (!SeleniumHelper.Click(account.driver, By.Id("register-submit")))
				{
					return ResultModel.Fail;
				}
				if (SeleniumHelper.WaitElement(account.driver, By.ClassName("login-notification-title-box"), 8))
				{
					return ResultModel.Fail;
				}
			}
			account.Status = "Đang đợi url change ...";
			if (!SeleniumHelper.UrlChange(account.driver, url, 10))
			{
				return ResultModel.Fail;
			}
			#region Get LinkVery
			account.Status = "Đến trang lấy link very ...";
			try
			{
				account.driver.Url = "https://www.okx.com/vi/account/kyc/personal/overview";
			}
			catch
			{
				return ResultModel.Fail;
			}
			Thread.Sleep(2000);
			url = account.driver.Url;
			if (!SeleniumHelper.Click(account.driver, By.ClassName("compliance-btn")))
			{
				return ResultModel.Fail;
			}

			if (!SeleniumHelper.UrlChange(account.driver, url, 10))
			{
				return ResultModel.Fail;
			}
			Thread.Sleep(2000);
			if (!SeleniumHelper.Click(account.driver, By.ClassName("compliance-select-inner-box"), count: 1))
			{
				return ResultModel.Fail;
			}
			Thread.Sleep(2000);
			if (!SeleniumHelper.WaitElement(account.driver, By.ClassName("CustomSelectOption_kyc-custom-option-label__VbKlR"), 8))
			{
				if (!SeleniumHelper.Click(account.driver, By.ClassName("compliance-select-inner-box"), count: 1))
				{
					return ResultModel.Fail;
				}

			}
			if (!SeleniumHelper.Click(account.driver, By.ClassName("CustomSelectOption_kyc-custom-option-label__VbKlR")))
			{
				return ResultModel.Fail;
			}
			//Bấm tiếp theo chỗ very
			account.Status = "Đang tiếp theo ...";
			Thread.Sleep(2000);
			if (!SeleniumHelper.Click(account.driver, By.ClassName("compliance-btn")))
			{
				return ResultModel.Fail;
			}
			//Bấm sao chép liên kết
			account.Status = "Bấm sao chép liên kết ...";
			Thread.Sleep(2000);
			if (!SeleniumHelper.Click(account.driver, By.ClassName("styles_link__A5gGI")))
			{
				return ResultModel.Fail;
			}
			//account.LinkVery = "https://www.okx.com/vi/kyc-verify?sessionId=72e908f2d0c44c85a7a2cde895ac0061&fromQRcode=true&sdkTraceId=627102467771389266&source_code=kyc_main_flow&isPassport=false&selfieOnly=false&autoClosePage=true";
			string clipboardText = string.Empty;
			Application.Current.Dispatcher.Invoke(() =>
			{
				clipboardText = Clipboard.GetText();
			});
			if (String.IsNullOrEmpty(clipboardText))
			{
				return ResultModel.Fail;
			}
			// Sau đó gán giá trị cho account.LinkVery
			account.LinkVery = clipboardText;
			Thread.Sleep(2000);
			#endregion

			var resultGet2FA = await Get2FA();
			if (resultGet2FA == ResultModel.Get2FaFail)
			{
				return ResultModel.Get2FaFail;
			}
			else if (resultGet2FA == ResultModel.RunOutOfBalance)
			{
				return ResultModel.RunOutOfBalance;
			}

			return ResultModel.Success;
		}

		public async Task<ResultModel> Get2FA()
		{
			int countPerForm = 0;
			string C_2FA = "", codeNumber = "";
		#region Get 2FA
		reStart:
			countPerForm++;
			account.Status = "Đến trang lấy 2FA ...";
			try
			{
				account.driver.Url = "https://www.okx.com/vi/account/users/google/set";
			}
			catch
			{
				return ResultModel.Get2FaFail;
			}
			Thread.Sleep(2000);
			account.Status = "Bấm tiếp ...";
			for (int i = 1; i <= 3; i++)
			{
				if (SeleniumHelper.Click(account.driver, By.ClassName("btn-content")))
				{
					break;
				}
				if (i >= 3)
				{
					return ResultModel.Get2FaFail;
				}
			}

			Thread.Sleep(random.Next(1000, 4000));

			if (!SeleniumHelper.WaitElement(account.driver, By.ClassName("style_backup-code-val__h5jHO"), 8))
			{
				if (!SeleniumHelper.Click(account.driver, By.ClassName("btn-content")))
				{
					return ResultModel.Get2FaFail;
				}
				if (!SeleniumHelper.WaitElement(account.driver, By.ClassName("style_backup-code-val__h5jHO"), 5))
				{
					return ResultModel.Get2FaFail;
				}
			}
			C_2FA = SeleniumHelper.GetTextElement(account.driver, By.ClassName("style_backup-code-val__h5jHO"));
			if (string.IsNullOrEmpty(C_2FA))
			{
				return ResultModel.Get2FaFail;
			}
			Thread.Sleep(random.Next(1000, 4000));
			if (!SeleniumHelper.Click(account.driver, By.ClassName("btn-content")))
			{
				return ResultModel.Get2FaFail;
			}

			//Bấm gửi mã 
			account.Status = "Bấm gửi mã ...";
			Thread.Sleep(random.Next(1000, 4000));
			if (!SeleniumHelper.Click(account.driver, By.ClassName("accountuix-input-suffix")))
			{
				return ResultModel.Get2FaFail;
			}
			//accountuix-dialog-title-container
			//
			account.Status = "Đang check xem có captcha";
			if (SeleniumHelper.WaitElement(account.driver, By.ClassName("accountuix-dialog-title-container"), 5))
			{
				account.Status = "Đang đợi giải captcha";
				for (int i = 0; i < 3; i++)
				{
					if (!SeleniumHelper.WaitElementHidden(account.driver, By.ClassName("accountuix-dialog-title-container"), 5))
					{
						Thread.Sleep(5000);
					}
				}
				if (!SeleniumHelper.WaitElementHidden(account.driver, By.ClassName("accountuix-dialog-title-container"), 5))
				{
					return ResultModel.Get2FaFail;
				}

				//if (countPerForm == 3)
				//{
				//	return ResultModel.Get2FaFail;
				//}
				//goto reStart;
			}
			account.Status = "Đang thuê lại số ...";
			var reRentPhone = await ViOTPHelper.ReRentPhoneNumber(account);
			if (!reRentPhone)
			{
				if (account.Status == "Số dư quý khách không đủ !")
				{
					return ResultModel.RunOutOfBalance;
				}
				return ResultModel.Get2FaFail;
			}

			for (int i = 1; i <= 2; i++)
			{
				account.Status = $"Đang đợi code number lần {i}";
				codeNumber = await ViOTPHelper.GetCodeNumber(account);
				if (codeNumber != "")
				{
					break;
				}
				//Bấm gửi lại mã
				if (!SeleniumHelper.Click(account.driver, By.ClassName("accountuix-input-suffix")))
				{
					return ResultModel.Get2FaFail;
				}
				if (SeleniumHelper.WaitElement(account.driver, By.ClassName("accountuix-dialog-title-container"), 5))
				{

				}
				if (SeleniumHelper.WaitElement(account.driver, By.ClassName("accountuix-form-item-control-explain-error"), 8))
				{
					return ResultModel.Get2FaFail;
				}
				if (i == 2)
				{
					return ResultModel.Get2FaFail;
				}
			}

			var code2FA = FunctionHelper.ConvertTwoFA(C_2FA);
			if (code2FA == "")
			{
				return ResultModel.Get2FaFail;
			}
			account.Status = "Nhập code ...";
			if (!SeleniumHelper.SendKeys(account.driver, By.ClassName("accountuix-input-input"), codeNumber))
			{
				return ResultModel.Fail;
			}
			Thread.Sleep(random.Next(1000, 4000));
			if (!SeleniumHelper.SendKeys(account.driver, By.ClassName("accountuix-input-input"), code2FA, count: 1))
			{
				return ResultModel.Fail;
			}
			Thread.Sleep(random.Next(1000, 4000));
			url = account.driver.Url;
			account.Status = "Bấm xác nhận ...";
			if (!SeleniumHelper.Click(account.driver, By.ClassName("btn-content")))
			{
				return ResultModel.Get2FaFail;
			}
			if (!SeleniumHelper.WaitElementHidden(account.driver, By.ClassName("btn-content"), 5))
			{
				if (SeleniumHelper.WaitElement(account.driver, By.ClassName("btn-disabled"), 5))
				{
					if (!SeleniumHelper.Click(account.driver, By.ClassName("accountuix-input-input")))
					{
						return ResultModel.Get2FaFail;
					}
					if (!SeleniumHelper.Click(account.driver, By.ClassName("accountuix-input-input"), count: 1))
					{
						return ResultModel.Get2FaFail;
					}
					if (!SeleniumHelper.GetEnableElement(account.driver, By.ClassName("btn-content")))
					{
						return ResultModel.Get2FaFail;
					}
					if (!SeleniumHelper.Click(account.driver, By.ClassName("btn-content")))
					{
						return ResultModel.Get2FaFail;
					}
				}
			}

			if (SeleniumHelper.WaitElement(account.driver, By.ClassName("accountuix-form-item-control-explain-error"), 8))
			{
				return ResultModel.Get2FaFail;
			}
			account.Status = "Đang đợi url change ...";
			if (!SeleniumHelper.UrlChange(account.driver, url, 10))
			{
				return ResultModel.Get2FaFail;
			}
			account.C_2FA = C_2FA;
			#endregion
			return ResultModel.Success;
		}
	}
}
