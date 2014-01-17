using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Reactive;
using System.Windows.Input;
using System.Reactive.Linq;
using System.Windows.Media;
using System.Threading;

namespace WMIViewer
{
	//http://www.thomaslevesque.com/2009/08/04/wpf-automatically-sort-a-gridview-continued/
	public class GridViewSort
	{
		interface IViewSource
		{
			SortDescriptionCollection SortDescriptions
			{
				get;
			}
			event Predicate<object> Filter;
		}
		public class SelectorAdapter : IViewSource
		{
			Selector _Selector;
			public SelectorAdapter(Selector selector)
			{
				_Selector = selector;
			}
			#region IViewSource Members

			public SortDescriptionCollection SortDescriptions
			{
				get
				{
					return _Selector.Items.SortDescriptions;
				}
			}

			public event Predicate<object> Filter
			{
				add
				{
					_Selector.Items.Filter = value;
				}
				remove
				{
					_Selector.Items.Filter = null;
				}
			}

			#endregion
		}
		public class CollectionViewSourceAdapter : IViewSource
		{
			CollectionViewSource src;
			public CollectionViewSourceAdapter(CollectionViewSource src)
			{
				this.src = src;
			}
			#region IViewSource Members

			public SortDescriptionCollection SortDescriptions
			{
				get
				{
					return src.SortDescriptions;
				}
			}

			Dictionary<Predicate<object>, FilterEventHandler> _Handlers = new Dictionary<Predicate<object>, FilterEventHandler>();
			public event Predicate<object> Filter
			{
				add
				{
					FilterEventHandler handler = (s,e)=>{e.Accepted = value(e.Item);};
					_Handlers.Add(value, handler);
					src.Filter += handler;
				}
				remove
				{
					FilterEventHandler handler = null;
					_Handlers.TryGetValue(value, out  handler);
					if(handler != null)
					{
						src.Filter -= handler;
						_Handlers.Remove(value);
					}
				}
			}

			#endregion
		}

		public static string GetSortBy(DependencyObject obj)
		{
			return (string)obj.GetValue(SortByProperty);
		}

		public static void SetSortBy(DependencyObject obj, string value)
		{
			obj.SetValue(SortByProperty, value);
		}

		public static readonly DependencyProperty SortByProperty =
			DependencyProperty.RegisterAttached("SortBy", typeof(string), typeof(GridViewSort), new PropertyMetadata(OnSortByChanged));

		static void OnSortByChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			IViewSource viewSource = GetViewSource(source);
			var sortCollection = viewSource.SortDescriptions;
			sortCollection.Clear();
			if(args.NewValue != null)
				sortCollection.Add(new SortDescription((string)args.NewValue, ListSortDirection.Ascending));
		}

		private static IViewSource GetViewSource(DependencyObject source)
		{
			IViewSource result = null;
			CollectionViewSource sender = source as CollectionViewSource;
			if(sender != null)
				result = new CollectionViewSourceAdapter(sender);
			else
			{
				Selector sender2 = source as Selector;
				if(sender2 != null)
					result = new SelectorAdapter(sender2);
			}
			return result;
		}

		#region Public attached properties

		public static ICommand GetCommand(DependencyObject obj)
		{
			return (ICommand)obj.GetValue(CommandProperty);
		}

		public static void SetCommand(DependencyObject obj, ICommand value)
		{
			obj.SetValue(CommandProperty, value);
		}

		// Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.RegisterAttached(
				"Command",
				typeof(ICommand),
				typeof(GridViewSort),
				new UIPropertyMetadata(
					null,
					(o, e) =>
					{
						ItemsControl listView = o as ItemsControl;
						if(listView != null)
						{
							if(!GetAutoSort(listView)) // Don't change click handler if AutoSort enabled
							{
								if(e.OldValue != null && e.NewValue == null)
								{
									listView.RemoveHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
								}
								if(e.OldValue == null && e.NewValue != null)
								{
									listView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
								}
							}
						}
					}
				)
			);

		public static bool GetAutoSort(DependencyObject obj)
		{
			return (bool)obj.GetValue(AutoSortProperty);
		}

		public static void SetAutoSort(DependencyObject obj, bool value)
		{
			obj.SetValue(AutoSortProperty, value);
		}

		// Using a DependencyProperty as the backing store for AutoSort.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AutoSortProperty =
			DependencyProperty.RegisterAttached(
				"AutoSort",
				typeof(bool),
				typeof(GridViewSort),
				new UIPropertyMetadata(
					false,
					(o, e) =>
					{
						ListView listView = o as ListView;
						if(listView != null)
						{
							if(GetCommand(listView) == null) // Don't change click handler if a command is set
							{
								bool oldValue = (bool)e.OldValue;
								bool newValue = (bool)e.NewValue;
								if(oldValue && !newValue)
								{
									listView.RemoveHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
								}
								if(!oldValue && newValue)
								{
									listView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
								}
							}
						}
					}
				)
			);

		public static string GetPropertyName(DependencyObject obj)
		{
			return (string)obj.GetValue(PropertyNameProperty);
		}

		public static void SetPropertyName(DependencyObject obj, string value)
		{
			obj.SetValue(PropertyNameProperty, value);
		}

		// Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PropertyNameProperty =
			DependencyProperty.RegisterAttached(
				"PropertyName",
				typeof(string),
				typeof(GridViewSort),
				new UIPropertyMetadata(null)
			);

		public static bool GetShowSortGlyph(DependencyObject obj)
		{
			return (bool)obj.GetValue(ShowSortGlyphProperty);
		}

		public static void SetShowSortGlyph(DependencyObject obj, bool value)
		{
			obj.SetValue(ShowSortGlyphProperty, value);
		}

		// Using a DependencyProperty as the backing store for ShowSortGlyph.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ShowSortGlyphProperty =
			DependencyProperty.RegisterAttached("ShowSortGlyph", typeof(bool), typeof(GridViewSort), new UIPropertyMetadata(true));

		public static ImageSource GetSortGlyphAscending(DependencyObject obj)
		{
			return (ImageSource)obj.GetValue(SortGlyphAscendingProperty);
		}

		public static void SetSortGlyphAscending(DependencyObject obj, ImageSource value)
		{
			obj.SetValue(SortGlyphAscendingProperty, value);
		}

		// Using a DependencyProperty as the backing store for SortGlyphAscending.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SortGlyphAscendingProperty =
			DependencyProperty.RegisterAttached("SortGlyphAscending", typeof(ImageSource), typeof(GridViewSort), new UIPropertyMetadata(null));

		public static ImageSource GetSortGlyphDescending(DependencyObject obj)
		{
			return (ImageSource)obj.GetValue(SortGlyphDescendingProperty);
		}

		public static void SetSortGlyphDescending(DependencyObject obj, ImageSource value)
		{
			obj.SetValue(SortGlyphDescendingProperty, value);
		}

		// Using a DependencyProperty as the backing store for SortGlyphDescending.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SortGlyphDescendingProperty =
			DependencyProperty.RegisterAttached("SortGlyphDescending", typeof(ImageSource), typeof(GridViewSort), new UIPropertyMetadata(null));

		#endregion

		#region Private attached properties

		private static GridViewColumnHeader GetSortedColumnHeader(DependencyObject obj)
		{
			return (GridViewColumnHeader)obj.GetValue(SortedColumnHeaderProperty);
		}

		private static void SetSortedColumnHeader(DependencyObject obj, GridViewColumnHeader value)
		{
			obj.SetValue(SortedColumnHeaderProperty, value);
		}

		// Using a DependencyProperty as the backing store for SortedColumn.  This enables animation, styling, binding, etc...
		private static readonly DependencyProperty SortedColumnHeaderProperty =
			DependencyProperty.RegisterAttached("SortedColumnHeader", typeof(GridViewColumnHeader), typeof(GridViewSort), new UIPropertyMetadata(null));

		#endregion

		#region Column header click event handler

		private static void ColumnHeader_Click(object sender, RoutedEventArgs e)
		{
			GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
			if(headerClicked != null && headerClicked.Column != null)
			{
				string propertyName = GetPropertyName(headerClicked.Column);
				if(!string.IsNullOrEmpty(propertyName))
				{
					ListView listView = GetAncestor<ListView>(headerClicked);
					if(listView != null)
					{
						ICommand command = GetCommand(listView);
						if(command != null)
						{
							if(command.CanExecute(propertyName))
							{
								command.Execute(propertyName);
							}
						}
						else if(GetAutoSort(listView))
						{
							ApplySort(listView.Items, propertyName, listView, headerClicked);
						}
					}
				}
			}
		}

		#endregion

		#region Helper methods

		public static T GetAncestor<T>(DependencyObject reference) where T : DependencyObject
		{
			DependencyObject parent = VisualTreeHelper.GetParent(reference);
			while(!(parent is T))
			{
				parent = VisualTreeHelper.GetParent(parent);
			}
			if(parent != null)
				return (T)parent;
			else
				return null;
		}

		public static void ApplySort(ICollectionView view, string propertyName, ListView listView, GridViewColumnHeader sortedColumnHeader)
		{
			ListSortDirection direction = ListSortDirection.Ascending;
			if(view.SortDescriptions.Count > 0)
			{
				SortDescription currentSort = view.SortDescriptions[0];
				if(currentSort.PropertyName == propertyName)
				{
					if(currentSort.Direction == ListSortDirection.Ascending)
						direction = ListSortDirection.Descending;
					else
						direction = ListSortDirection.Ascending;
				}
				view.SortDescriptions.Clear();

				GridViewColumnHeader currentSortedColumnHeader = GetSortedColumnHeader(listView);
				if(currentSortedColumnHeader != null)
				{
					RemoveSortGlyph(currentSortedColumnHeader);
				}
			}
			if(!string.IsNullOrEmpty(propertyName))
			{
				view.SortDescriptions.Add(new SortDescription(propertyName, direction));
				if(GetShowSortGlyph(listView))
					AddSortGlyph(
						sortedColumnHeader,
						direction,
						direction == ListSortDirection.Ascending ? GetSortGlyphAscending(listView) : GetSortGlyphDescending(listView));
				SetSortedColumnHeader(listView, sortedColumnHeader);
			}
		}

		private static void AddSortGlyph(GridViewColumnHeader columnHeader, ListSortDirection direction, ImageSource sortGlyph)
		{
			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
			adornerLayer.Add(
				new SortGlyphAdorner(
					columnHeader,
					direction,
					sortGlyph
					));
		}

		private static void RemoveSortGlyph(GridViewColumnHeader columnHeader)
		{
			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(columnHeader);
			Adorner[] adorners = adornerLayer.GetAdorners(columnHeader);
			if(adorners != null)
			{
				foreach(Adorner adorner in adorners)
				{
					if(adorner is SortGlyphAdorner)
						adornerLayer.Remove(adorner);
				}
			}
		}

		#endregion

		#region SortGlyphAdorner nested class

		private class SortGlyphAdorner : Adorner
		{
			private GridViewColumnHeader _columnHeader;
			private ListSortDirection _direction;
			private ImageSource _sortGlyph;

			public SortGlyphAdorner(GridViewColumnHeader columnHeader, ListSortDirection direction, ImageSource sortGlyph)
				: base(columnHeader)
			{
				_columnHeader = columnHeader;
				_direction = direction;
				_sortGlyph = sortGlyph;
			}

			private Geometry GetDefaultGlyph()
			{
				double x1 = _columnHeader.ActualWidth - 13;
				double x2 = x1 + 10;
				double x3 = x1 + 5;
				double y1 = _columnHeader.ActualHeight / 2 - 3;
				double y2 = y1 + 5;

				if(_direction == ListSortDirection.Ascending)
				{
					double tmp = y1;
					y1 = y2;
					y2 = tmp;
				}

				PathSegmentCollection pathSegmentCollection = new PathSegmentCollection();
				pathSegmentCollection.Add(new LineSegment(new Point(x2, y1), true));
				pathSegmentCollection.Add(new LineSegment(new Point(x3, y2), true));

				PathFigure pathFigure = new PathFigure(
					new Point(x1, y1),
					pathSegmentCollection,
					true);

				PathFigureCollection pathFigureCollection = new PathFigureCollection();
				pathFigureCollection.Add(pathFigure);

				PathGeometry pathGeometry = new PathGeometry(pathFigureCollection);
				return pathGeometry;
			}

			protected override void OnRender(DrawingContext drawingContext)
			{
				base.OnRender(drawingContext);

				if(_sortGlyph != null)
				{
					double x = _columnHeader.ActualWidth - 13;
					double y = _columnHeader.ActualHeight / 2 - 5;
					Rect rect = new Rect(x, y, 10, 10);
					drawingContext.DrawImage(_sortGlyph, rect);
				}
				else
				{
					drawingContext.DrawGeometry(Brushes.LightGray, new Pen(Brushes.Gray, 1.0), GetDefaultGlyph());
				}
			}
		}

		#endregion





		public static string GetAutoFilter(DependencyObject obj)
		{
			return (string)obj.GetValue(AutoFilterProperty);
		}

		public static void SetAutoFilter(DependencyObject obj, string value)
		{
			obj.SetValue(AutoFilterProperty, value);
		}

		// Using a DependencyProperty as the backing store for AutoFilter.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty AutoFilterProperty =
			DependencyProperty.RegisterAttached("AutoFilter", typeof(string), typeof(GridViewSort), new PropertyMetadata(null, UpdateFilter));

		static DependencyProperty SubjectProperty = DependencyProperty.RegisterAttached("Subject", typeof(Subject<bool>), typeof(GridViewSort), new PropertyMetadata(null));


		static void UpdateFilter(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ui = SynchronizationContext.Current;
			Subject<bool> subject = d.GetValue(SubjectProperty) as Subject<bool>;
			if(subject == null)
			{
				subject = new Subject<bool>();
				subject.Sample(TimeSpan.FromSeconds(1.0))
						.ObserveOn(ui)
						.Subscribe(_ =>
						{
							var source = GetViewSource(d);
							if(source != null && !string.IsNullOrEmpty(GetFilterMemberPath(d)))
							{
								source.Filter += (o) =>
								{
									var value = (string)Convert.ChangeType(DataBinder.GetBindingValue(GetFilterMemberPath(d), o), typeof(String));
									if(value == null)
										value = "";

									return value.IndexOf(GetAutoFilter(d) ?? "", StringComparison.InvariantCultureIgnoreCase) != -1;
								};
							}
						});
				d.SetValue(SubjectProperty, subject);
			}
			subject.OnNext(true);

		}

		public static string GetFilterMemberPath(DependencyObject obj)
		{
			return (string)obj.GetValue(FilterMemberPathProperty);
		}

		public static void SetFilterMemberPath(DependencyObject obj, string value)
		{
			obj.SetValue(FilterMemberPathProperty, value);
		}

		// Using a DependencyProperty as the backing store for FilterBinding.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FilterMemberPathProperty =
			DependencyProperty.RegisterAttached("FilterMemberPath", typeof(string), typeof(GridViewSort), new PropertyMetadata(null, UpdateFilter));




		public static TextBox GetSearchBox(DependencyObject obj)
		{
			return (TextBox)obj.GetValue(SearchBoxProperty);
		}

		public static void SetSearchBox(DependencyObject obj, TextBox value)
		{
			obj.SetValue(SearchBoxProperty, value);
		}

		// Using a DependencyProperty as the backing store for SearchBox.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SearchBoxProperty =
			DependencyProperty.RegisterAttached("SearchBox", typeof(TextBox), typeof(GridViewSort), new PropertyMetadata(null, (s, a) =>
			{
				var command = new CommandBinding()
				{
					Command = NavigationCommands.Search,
				};
				command.Executed += (ss, aa) =>
				{
					GetSearchBox(s).Focus();
					aa.Handled = true;
				};

				((UIElement)(s)).InputBindings.Add(new KeyBinding(NavigationCommands.Search, new KeyGesture(Key.F, ModifierKeys.Control)));		
				((UIElement)(s)).CommandBindings.Add(command);
			}));



	}

	public class DataBinder : DependencyObject
	{
		private static readonly DependencyProperty BindingEvalProperty = DependencyProperty.RegisterAttached(
	   "BindingEval",
	   typeof(Object),
	   typeof(DataBinder),
	   new UIPropertyMetadata(null));
		public static object GetBindingValue(string property, object item)
		{
			var binding = new Binding
			{
				Path = new PropertyPath(property),
				Source = item
			};
			var evalobj = new DataBinder();
			BindingOperations.SetBinding(evalobj, BindingEvalProperty, binding);
			return evalobj.GetValue(BindingEvalProperty);
		}

	}
}
