using System;
using System.Windows.Forms;

namespace ConfigCreator
{
	// Token: 0x02000003 RID: 3
	internal static class Program
	{
		// Token: 0x06000008 RID: 8 RVA: 0x00002098 File Offset: 0x00000298
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
	}
}
