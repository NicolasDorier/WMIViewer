using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMIViewer
{
	public class SelectableObservableCollection<T> : ObservableCollection<T> where T : class
	{
		private T _Selected;
		

		public SelectableObservableCollection()
		{

		}
		public SelectableObservableCollection(IEnumerable<T> init):base(init)
		{
		}
		public T Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				if(value != _Selected)
				{
					_Selected = value;
					OnPropertyChanged(new PropertyChangedEventArgs("Selected"));
				}
			}
		}
	}
}
