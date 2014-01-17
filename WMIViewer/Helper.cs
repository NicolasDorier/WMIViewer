using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMIViewer
{
	class Helper
	{
		internal static string Escape(string p)
		{
			return p.Replace("\"", "\"\"");
		}
	}
}
