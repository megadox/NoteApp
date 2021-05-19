using EvernoteClone.Model;
using EvernoteClone.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace EvernoteClone.ViewModel
{
    public class LoginVM
    {
		private User user;

		public User User
		{
			get { return user; }
			set { user = value; }
		}

		public RegisterCommand RegisterCommand { get; set; }

		public LoginCommand LoginCommand { get; set; }

		public LoginVM()
		{
			RegisterCommand = new RegisterCommand(this);
			LoginCommand = new LoginCommand(this);
		}
	}
}
