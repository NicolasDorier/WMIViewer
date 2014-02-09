using AOManager;
using NicolasDorier;
using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using NicolasDorier.Reactive;
using System.Reactive;
using System.Reactive.Linq;

namespace WMIViewer
{
	public class NamespaceViewModel : NotifyPropertyChangedBase
	{
		public NamespaceViewModel()
		{
			Bus.Commmands.ItemChanged()
						 .Where(i => i.Added)
						 .Subscribe(c =>
						 {
							 CurrentCommand = new WrapperCommandViewModel(c.Item);
						 });
			Classes.ItemPropertyChanged(o => o.Selected)
				   .Subscribe(c =>
				   {
					   if(c.NewValue != null)
						   c.NewValue.Refresh.Execute();
				   });
			Namespaces.ItemPropertyChanged(o => o.Selected)
					  .Subscribe(o => Namespace = "\\\\localhost\\root\\" + o.NewValue);
			Namespaces.Selected = "CIMV2";
			new RefreshNamespaceCommand(this).Execute();
		}

		public class RefreshNamespaceCommand : CommandViewModel<List<string>>
		{
			private NamespaceViewModel namespaceViewModel;

			public RefreshNamespaceCommand(NamespaceViewModel namespaceViewModel)
				: base(new CommandBusViewModel())
			{
				this.namespaceViewModel = namespaceViewModel;
			}
			protected override Task<List<string>> StartExecutionCore(object parameter)
			{
				return new Task<List<string>>(() =>
				{
					List<string> names = new List<string>();
					ManagementScope ms = new ManagementScope();

					//Provides a wrapper for building paths to WMI objects
					ManagementPath path = new ManagementPath("\\\\localhost\\root");
					ms.Path = path;
					ms.Connect();

					ObjectQuery wql = new ObjectQuery("select * from __Namespace");
					ManagementObjectSearcher searcher =
						new ManagementObjectSearcher(ms, wql);
					ManagementObjectCollection oc = searcher.Get();

					foreach(var result in oc)
					{
						names.Add((string)result["Name"]);
					}
					return names;
				});
			}

			protected override void UpdateModelCore()
			{
				if(!Execution.IsFaulted && Execution.Result != null)
				{
					CollectionExtensions.Synchronize(
						namespaceViewModel.Namespaces,
						Execution.Result,
						o => o,
						o => o,
						o => o);
				}
			}
		}

		private SelectableObservableCollection<string> _Namespaces = new SelectableObservableCollection<string>();
		public SelectableObservableCollection<string> Namespaces
		{
			get
			{
				return _Namespaces;
			}
		}


		private WrapperCommandViewModel _CurrentCommand;
		public WrapperCommandViewModel CurrentCommand
		{
			get
			{
				return _CurrentCommand;
			}
			set
			{
				if(value != _CurrentCommand)
				{
					_CurrentCommand = value;
					OnPropertyChanged(() => this.CurrentCommand);
				}
			}
		}
		CommandBusViewModel _Bus;
		public CommandBusViewModel Bus
		{
			get
			{
				if(_Bus == null)
				{
					_Bus = new CommandBusViewModel();
				}
				return _Bus;
			}
		}

		ManagementClass CreateClass()
		{
			return new ManagementClass(Namespace);
		}

		public class RefreshCommand : CommandViewModel
		{
			private NamespaceViewModel _Parent;

			public RefreshCommand(NamespaceViewModel parent)
				: base(parent.Bus, parent)
			{
				this._Parent = parent;
			}

			protected override Task StartExecution(object parameter)
			{
				var classesByPath = _Parent.Classes.ToDictionary(c => c.ManagementClass.Path);
				return new Task(() =>
				{
					RunningMessage = "Enumerating classes in " + _Parent.Namespace;
					foreach(var mgmt in _Parent.CreateClass()
						.GetSubclasses(new EnumerationOptions()
						{
							EnumerateDeep = true
						})
						.OfType<ManagementClass>().ToList())
					{
						var data = ManagementClassData.Create(mgmt);
						this.OnDispatcher(() =>
						{
							ClassViewModel vm = null;
							if(!classesByPath.TryGetValue(data.Path, out vm))
							{
								vm = new ClassViewModel(_Parent, data);
								_Parent.Classes.Add(vm);
							}
							vm.ManagementClass = data;
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
		private readonly SelectableObservableCollection<ClassViewModel> _Classes = new SelectableObservableCollection<ClassViewModel>();
		public SelectableObservableCollection<ClassViewModel> Classes
		{
			get
			{
				return _Classes;
			}
		}


		private string _Namespace;
		public string Namespace
		{
			get
			{
				return _Namespace;
			}
			set
			{
				if(value != _Namespace)
				{
					_Namespace = value;
					Classes.Clear();
					Refresh.Execute();
					OnPropertyChanged(() => this.Namespace);
				}
			}
		}
	}
}
