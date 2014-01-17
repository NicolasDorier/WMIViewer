using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace WMIViewer
{
	public class ViewModelCommandBinding : FrameworkElement
	{


		public ICommand ViewModel
		{
			get
			{
				return (ICommand)GetValue(ViewModelProperty);
			}
			set
			{
				SetValue(ViewModelProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ViewModelProperty =
			DependencyProperty.Register("ViewModel", typeof(ICommand), typeof(ViewModelCommandBinding), new PropertyMetadata(null, Refresh));


		private static void Refresh(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ViewModelCommandBinding)d).RefreshBinding();
		}


		CommandBinding _Binding;

		private void RefreshBinding()
		{
			if(_Binding != null)
			{
				Source.CommandBindings.Remove(_Binding);
				Command.CanExecuteChanged -= Command_CanExecuteChanged;
			}
			if(Command != null && ViewModel != null && Source != null)
			{
				_Binding = new CommandBinding();
				_Binding.Command = Command;
				_Binding.CanExecute += _Binding_CanExecute;
				_Binding.Executed += _Binding_Executed;

				Command.CanExecuteChanged += Command_CanExecuteChanged;
				Source.CommandBindings.Add(_Binding);
			}
		}

		void Command_CanExecuteChanged(object sender, EventArgs e)
		{
			CommandManager.InvalidateRequerySuggested();
		}

		void _Binding_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ViewModel.Execute(e.Parameter);
			e.Handled = true;
		}

		void _Binding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ViewModel.CanExecute(e.Parameter);
			e.Handled = true;
		}

		public RoutedCommand Command
		{
			get
			{
				return (RoutedCommand)GetValue(CommandProperty);
			}
			set
			{
				SetValue(CommandProperty, value);
			}
		}

		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.Register("Command", typeof(RoutedCommand), typeof(ViewModelCommandBinding), new PropertyMetadata(null, Refresh));




		public UIElement Source
		{
			get
			{
				return (UIElement)GetValue(SourceProperty);
			}
			set
			{
				SetValue(SourceProperty, value);
			}
		}

		// Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register("Source", typeof(UIElement), typeof(ViewModelCommandBinding), new PropertyMetadata(null, (s, a) =>
			{
				((ViewModelCommandBinding)s).SetDataContext();
				Refresh(s, a);
			}));

		private void SetDataContext()
		{
			this.SetBinding(ViewModelCommandBinding.DataContextProperty, new Binding()
			{
				Source = Source,
				Path = new PropertyPath("DataContext")
			});
		}
	}


	public class ViewModelCommandBindings : ObservableCollection<ViewModelCommandBinding>
	{
		public ViewModelCommandBindings()
		{
			this.CollectionChanged += ViewModelCommandBindings_CollectionChanged;
		}

		void ViewModelCommandBindings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if(e.NewItems != null)
			{
				foreach(ViewModelCommandBinding bind in e.NewItems)
				{
					bind.Source = Source;
				}
			}
		}
		UIElement _Source;
		public UIElement Source
		{
			get
			{
				return _Source;
			}

			set
			{
				_Source = value;
				foreach(var i in this)
					i.Source = value;
			}
		}

	}
	public class CommandBindingExtensions
	{


		public static ViewModelCommandBindings GetViewModelCommandBindings(DependencyObject obj)
		{
			return (ViewModelCommandBindings)obj.GetValue(ViewModelCommandBindingsProperty);
		}

		public static void SetViewModelCommandBindings(DependencyObject obj, ViewModelCommandBindings value)
		{
			obj.SetValue(ViewModelCommandBindingsProperty, value);
		}

		// Using a DependencyProperty as the backing store for ViewModelCommandBindings.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ViewModelCommandBindingsProperty =
			DependencyProperty.RegisterAttached("ViewModelCommandBindings", typeof(ViewModelCommandBindings), typeof(CommandBindingExtensions), new PropertyMetadata(null, (s, a) =>
			{
				((ViewModelCommandBindings)(a.NewValue)).Source = (UIElement)s;
			}));


	}
}
