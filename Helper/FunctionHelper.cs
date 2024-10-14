using OtpNet;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tool_Reg_OKX.Models;

namespace Tool_Reg_OKX.Helper
{
	public class FunctionHelper
	{
		private static readonly string lowercase = "abcdefghijklmnopqrstuvwxyz";
		private static readonly string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		private static readonly string digits = "0123456789";
		private static readonly string specialChars = "!@#$%";
		private static readonly string allChars = lowercase + uppercase + digits + specialChars;

		public static string GenerateRandomPassword(int minLength = 8, int maxLength = 32)
		{
			if (minLength < 8 || maxLength > 32 || minLength > maxLength)
			{
				throw new ArgumentException("Invalid password length range.");
			}

			// Randomize password length within the specified range
			Random random = new Random();
			int passwordLength = random.Next(minLength, maxLength + 1);

			// Ensure the password contains at least one character from each category
			StringBuilder password = new StringBuilder();
			password.Append(lowercase[random.Next(lowercase.Length)]);
			password.Append(uppercase[random.Next(uppercase.Length)]);
			password.Append(digits[random.Next(digits.Length)]);
			password.Append(specialChars[random.Next(specialChars.Length)]);

			// Fill the remaining password length with random characters from all categories
			for (int i = password.Length; i < passwordLength; i++)
			{
				password.Append(allChars[random.Next(allChars.Length)]);
			}

			// Shuffle the password to ensure randomness
			return new string(password.ToString().OrderBy(c => random.Next()).ToArray());
		}
		public static string GenerateRandomString(int length)
		{
			Random random = new Random();
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			char[] randomString = new char[length];

			for (int i = 0; i < length; i++)
			{
				randomString[i] = chars[random.Next(chars.Length)];
			}

			return new string(randomString);
		}

		public static string GenerateRandomEmail()
		{
			// Danh sách họ và tên đầy đủ
			List<string> fullNames = new List<string>
		{
			"nguyen van an", "tran thi anh", "le minh bao", "pham quoc binh",
			"hoang thi canh", "vu duy chau", "vo quang cuong", "truong thi dai",
			"dang van dung", "bui thi giang", "do khanh ha", "ho thi hanh",
			"nguyen thi hoang", "pham van hung", "le thi huyen", "tran minh khoa",
			"vu van khoi", "do thi kim", "hoang lan anh", "nguyen thi linh",
			"pham duc long", "vu hoang minh", "bui thi nam", "le khanh nga",
			"nguyen minh phuc", "pham thi phuong", "hoang quang tam", "do thi tien",
			"le thanh trang", "nguyen van trinh", "pham thi tuan", "vu hoang tung"
		};

			// Tạo đối tượng Random
			Random random = new Random();

			// Chọn ngẫu nhiên một tên đầy đủ từ danh sách
			string randomFullName = fullNames[random.Next(fullNames.Count)];

			// Loại bỏ khoảng trắng trong tên để phù hợp với email
			string formattedName = randomFullName.Replace(" ", "");

			// Tạo số ngẫu nhiên để đảm bảo email là duy nhất
			int randomNumber = random.Next(100, 999);

			// Ghép tên đã được format với số và domain email
			string email = formattedName + randomNumber.ToString() + $"{FunctionHelper.GenerateRandomString(2)}{random.Next(10, 100)}_{FunctionHelper.GenerateRandomString(3)}{random.Next(10, 1000)}@dragonvu.com";

			return email;
		}
		public static string ConvertTwoFA(string token)
		{
			for (var i = 0; i < 5; i++)
			{
				try
				{
					var totp = new Totp(Base32Encoding.ToBytes(token));
					var code = totp.ComputeTotp();
					if (code != "")
					{
						return code;
					}
				}
				catch
				{
					//
				}
			}

			return "";
		}

		public static async Task<string> GetCodeNumber(string Email)
		{
			var options = new RestClientOptions()
			{
				MaxTimeout = -1,
				
			};

			using (var restClient = new RestClient(options))
			{
				string respone = "";
				var requestUrl = $"http://mail.builuc1998.com/?type=getcode&key=123456&mail={Email}"; // URL đầy đủ
																									  // Sử dụng URL hoàn chỉnh trong request
				var request = new RestRequest(requestUrl, Method.Get);

				for (int i = 0; i < 60; i++)
				{
					RestResponse response = await restClient.ExecuteAsync(request);
					respone = response.Content == null ? "" : response.Content;

					var code = RegexHelper.GetValueFromGroup("\"code\":\"(.*?)\"", respone);

					if (code != "")
					{
						return code;
					}
					Thread.Sleep(1000);
				}

				return "";
			}
		}
	}
}
