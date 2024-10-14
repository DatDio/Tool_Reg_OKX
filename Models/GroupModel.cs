using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tool_Reg_OKX.Models
{
	public class GroupModel
	{
		private string _groupID;
		public string GroupID
		{
			get { return _groupID; }
			set
			{
				if (_groupID != value)
				{
					_groupID = value;
					OnPropertyChanged(nameof(GroupID));
				}
			}
		}
		private string _groupName;
		public string GroupName
		{
			get { return _groupName; }
			set
			{
				if (_groupName != value)
				{
					_groupName = value;
					OnPropertyChanged(nameof(GroupName));
				}
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
