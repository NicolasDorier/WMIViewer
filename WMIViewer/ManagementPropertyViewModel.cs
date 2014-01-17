using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMIViewer
{
	public class ManagementPropertyViewModel : ProxyViewModel<ManagementPropertyData>
	{
		public ManagementPropertyViewModel(InstanceViewModel instance, ManagementPropertyData prop):base(prop)
		{
			_Query = "(" + instance.Query + ")." + prop.Name;
		}

		private string _Query;
		public string Query
		{
			get
			{
				return _Query;
			}
			set
			{
				if(value != _Query)
				{
					_Query = value;
					OnPropertyChanged(() => this.Query);
				}
			}
		}
	}
}
