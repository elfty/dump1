using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Toolkit.Logging;

namespace MarketQADataProcessorApp
{
	public partial class BaseFormControl : UserControl
	{
		public BaseFormControl()
		{
			InitializeComponent();
		}

		delegate void AddMemoTextDelegate(string format, object[] args);

		internal void AddMemoText(string format, params object[] args)
		{
			
			
			if (listviewMemo.InvokeRequired)
			{
				Invoke(new AddMemoTextDelegate(AddMemoText), new object[] { format, args });
			}
			else
			{
				listviewMemo.Items.Add(string.Format(format, args));
			}

			listviewMemo.EnsureVisible(listviewMemo.Items.Count - 1);
		}
	}
}