using AODesign.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : AOWindow
	{
		public MainWindow()
		{
			InitializeComponent();
			var ns = new NamespaceViewModel();
			this.root.DataContext = ns;
		}

		private void RunPowershell(object sender, RoutedEventArgs e)
		{
			var file = new FileInfo(System.IO.Path.GetTempFileName() + ".ps1");
			File.WriteAllText(file.FullName,query.Text);
			Process.Start("powershell.exe", "-NoExit -File \"" + file.FullName +"\"");
		}

	}
}
