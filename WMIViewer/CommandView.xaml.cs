using AOManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WMIViewer
{
	/// <summary>
	/// Interaction logic for CommandViewModelView.xaml
	/// </summary>
	public partial class CommandView : UserControl
	{
		public CommandView()
		{
			InitializeComponent();
			this.Unloaded += CommandView_Unloaded;
		}


		public WrapperCommandViewModel ViewModel
		{
			get
			{
				return (WrapperCommandViewModel)this.GetValue(ViewModelProperty);
			}
			set
			{
				this.SetValue(ViewModelProperty, value);
			}
		}

		public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
																								  typeof(WrapperCommandViewModel),
																								  typeof(CommandView),
																								  new PropertyMetadata(ViewModelChanged));

		private static void ViewModelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			CommandView view = obj as CommandView;
			view.root.DataContext = args.NewValue;
			view.UnSubscribe(args.OldValue as WrapperCommandViewModel);
			view.Subscribe();
			view.UpdateStates(false);
		}


		void CommandView_Unloaded(object sender, RoutedEventArgs e)
		{
			UnSubscribe(ViewModel);
		}

		private void UnSubscribe(WrapperCommandViewModel vm)
		{
			if(vm != null)
			{
				vm.PropertyChanged -= ViewModel_PropertyChanged;
			}
		}

		private void Subscribe()
		{
			ViewModel.PropertyChanged += ViewModel_PropertyChanged;
		}

		void ViewModel_Executing()
		{
			UpdateStates(true);
		}

		void ViewModel_Executed(CommandViewModel obj)
		{
			UpdateStates(true);
		}

		void ViewModel_Faulted(CommandViewModel arg1, Exception arg2, bool arg3)
		{
			UpdateStates(true);
		}

		void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			UpdateStates(true);
		}

		private void UpdateStates(bool transition)
		{
			VisualStateManager.GoToState(this, "On" + ViewModel.State.ToString(), transition);
		}

		
	}
}
