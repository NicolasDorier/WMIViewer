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
	public class ManagementClassData
	{
		public string Name
		{
			get;
			set;
		}

		internal static ManagementClassData Create(ManagementClass mgmt)
		{
			var data = new ManagementClassData();
			data.Path = mgmt.Path.Path;
			data.Name = mgmt.ClassPath.ClassName;
			return data;
		}

		public string Path
		{
			get;
			set;
		}
	}
	public class ClassViewModel : NotifyPropertyChangedBase
	{
		private readonly SelectableObservableCollection<InstanceViewModel> _Instances = new SelectableObservableCollection<InstanceViewModel>();
		private ManagementClassData _ManagementClass;
		private NamespaceViewModel _Parent;

		public CommandBusViewModel Bus
		{
			get
			{
				return _Parent.Bus;
			}
		}

		public SelectableObservableCollection<InstanceViewModel> Instances
		{
			get
			{
				return _Instances;
			}
		}


		public ClassViewModel(NamespaceViewModel _Parent, ManagementClassData mgmtClass)
		{
			this._Parent = _Parent;
			this.ManagementClass = mgmtClass;
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

		public class RefreshCommand : CommandViewModel
		{
			private ClassViewModel _Parent;

			public RefreshCommand(ClassViewModel parent)
				: base(parent._Parent.Bus, parent)
			{
				this._Parent = parent;
			}
			protected override Task StartExecution(object parameter)
			{
				var instancesByPath = this._Parent.Instances.ToDictionary(i => i.ManagementObject.Path);
				return new Task(() =>
				{
					RunningMessage = "Refreshing instance list of " + this._Parent.ManagementClass.Name;
					var mgmtClass = _Parent.CreateManagementClass();

					var data = ManagementObjectData.Create(mgmtClass);
					OnDispatcher(() =>
					{
						_Parent.Instances.Selected = new InstanceViewModel(_Parent, data);
					});
					foreach(ManagementObject instance in mgmtClass.GetInstances())
					{
						var localData = ManagementObjectData.Create(instance);
						OnDispatcher(() =>
						{
							InstanceViewModel vm = null;
							if(!instancesByPath.TryGetValue(localData.Path, out vm))
							{
								vm = new InstanceViewModel(_Parent, localData);
								_Parent.Instances.Add(vm);

							}
							vm.ManagementObject = localData;
						});
					}
				});
			}
		}
		private RefreshCommand _Refresh;
		public RefreshCommand Refresh
		{
			get
			{
				if(_Refresh == null)
					_Refresh = new RefreshCommand(this);
				return _Refresh;
			}
		}

		public ManagementClassData ManagementClass
		{
			get
			{
				return _ManagementClass;
			}
			set
			{
				if(value != _ManagementClass)
				{
					_ManagementClass = value;
					Query = "Get-WmiObject \"" + value.Name + "\"";
					OnPropertyChanged(() => this.ManagementClass);
				}
			}
		}

		public ManagementClass CreateManagementClass()
		{
			return new ManagementClass(ManagementClass.Path);
		}
	}
}
