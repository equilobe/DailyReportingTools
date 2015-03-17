﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Web
{
	public class RegisterModel
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string ConfirmPassword { get; set; }
	}

	public class LoginModel
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public bool RememberMe { get; set; }
	}

	public class AuthenticationResponse
	{
		public bool IsValid { get; set; }
		public string Message { get; set; }
		public List<string> ErrorList { get; set; }
		public bool HasErrors { get { return ErrorList.Count > 0; } }
	}
}
