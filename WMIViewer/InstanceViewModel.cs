using AOManager;
using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace WMIViewer
{
	public class ManagementPropertyData
	{
		public string Name
		{
			get;
			set;
		}
		public string Value
		{
			get;
			set;
		}

		public bool IsKey
		{
			get;
			set;
		}

		internal static ManagementPropertyData Create(PropertyData prop)
		{
			ManagementPropertyData data = new ManagementPropertyData();
			data.Name = prop.Name;
			data.Value = ToString(prop.Value);
			data.IsKey = prop.Qualifiers.OfType<QualifierData>().Any(q => q.Name == "key" && true.Equals(q.Value));
			return data;
		}

		private static string ToString(object v)
		{
			if(v == null)
				return null;

			if(v is Array)
			{
				StringBuilder builder = new StringBuilder();
				foreach(var el in (Array)v)
				{
					builder.Append("[" + ToString(el) + "]");
				}
				return builder.ToString();
			}
			try
			{
				return (string)Convert.ChangeType(v, typeof(string));
			}
			catch(InvalidCastException)
			{
				return v.GetType() + " was not convertible to string";
			}
		}


	}
	public class ManagementObjectData
	{
		public List<ManagementPropertyData> Properties
		{
			get;
			set;
		}
		internal static ManagementObjectData Create(ManagementObject mgmtObj)
		{
			ManagementObjectData data = new ManagementObjectData();
			data.Properties = new List<ManagementPropertyData>();
			foreach(var prop in mgmtObj.Properties)
			{
				data.Properties.Add(ManagementPropertyData.Create(prop));
			}
			data.Path = mgmtObj.Path.Path;

			try
			{
				data.InstancePath = data.Path.Substring(mgmtObj.ClassPath.Path.Length + 1);
			}
			catch(ArgumentOutOfRangeException)
			{
			}

			return data;
		}

		public string InstancePath
		{
			get;
			set;
		}
		public string Path
		{
			get;
			set;
		}

		public bool IsClass
		{
			get
			{
				return string.IsNullOrEmpty(InstancePath);
			}
		}
	}
	public class InstanceViewModel : NotifyPropertyChangedBase
	{
		private ClassViewModel _Parent;

		public InstanceViewModel(ClassViewModel _Parent, ManagementObjectData data)
		{
			this._Parent = _Parent;
			ManagementObject = data;
		}

		

		public class RefreshCommand : CommandViewModel<ManagementObjectData>
		{
			InstanceViewModel _Parent;
			public RefreshCommand(InstanceViewModel parent)
				: base(parent._Parent.Bus, parent)
			{
				_Parent = parent;
			}
			protected override void UpdateModelCore()
			{
				if(!Execution.IsFaulted && Execution.Result != null)
				{
					_Parent.ManagementObject = Execution.Result;
				}
			}

			protected override bool CanExecuteCore(object parameter)
			{
				return !_Parent.ManagementObject.IsClass;
			}

			protected override Task<ManagementObjectData> StartExecutionCore(object parameter)
			{
				return new Task<ManagementObjectData>(() =>
				{
					RunningMessage = "Refreshing instance " + _Parent.ManagementObject.Path;
					return ManagementObjectData.Create(_Parent.CreateManagementObject());
				});
			}
		}

		public RefreshCommand _Refresh;
		public RefreshCommand Refresh
		{
			get
			{
				if(_Refresh == null)
					_Refresh = new RefreshCommand(this);
				return _Refresh;
			}
		}

		private ManagementObjectData _ManagementObject;
		public ManagementObjectData ManagementObject
		{
			get
			{
				return _ManagementObject;
			}
			set
			{
				if(value != _ManagementObject)
				{
					_ManagementObject = value;
					if(ManagementObject.IsClass)
					{
						Query = _Parent.Query;
					}
					else
					{
						Query = "[wmi]\"" + Helper.Escape(value.Path).Replace("\\\\" + Environment.MachineName, "\\\\.") + "\"";
					}
					Properties = new SelectableObservableCollection<ManagementPropertyViewModel>(value.Properties.Select(p => new ManagementPropertyViewModel(this, p)));
					OnPropertyChanged(() => this.ManagementObject);
				}
			}
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

		private SelectableObservableCollection<ManagementPropertyViewModel> _Properties = new SelectableObservableCollection<ManagementPropertyViewModel>();
		public SelectableObservableCollection<ManagementPropertyViewModel> Properties
		{
			get
			{
				return _Properties;
			}
			set
			{
				if(value != _Properties)
				{
					_Properties = value;
					OnPropertyChanged(() => this.Properties);
				}
			}
		}

		internal System.Management.ManagementObject CreateManagementObject()
		{
			return new ManagementObject(this.ManagementObject.Path);
		}
	}
}
