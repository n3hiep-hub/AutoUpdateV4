using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace ConfigCreator.Properties
{
	// Token: 0x02000005 RID: 5
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		// Token: 0x0600000C RID: 12 RVA: 0x000020AF File Offset: 0x000002AF
		internal Resources()
		{
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x0600000D RID: 13 RVA: 0x000020B7 File Offset: 0x000002B7
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					Resources.resourceMan = new ResourceManager("ConfigCreator.Properties.Resources", typeof(Resources).Assembly);
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600000E RID: 14 RVA: 0x000020E3 File Offset: 0x000002E3
		// (set) Token: 0x0600000F RID: 15 RVA: 0x000020EA File Offset: 0x000002EA
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000010 RID: 16 RVA: 0x000020F2 File Offset: 0x000002F2
		internal static Icon RebornICO
		{
			get
			{
				return (Icon)Resources.ResourceManager.GetObject("RebornICO", Resources.resourceCulture);
			}
		}

		// Token: 0x04000025 RID: 37
		private static ResourceManager resourceMan;

		// Token: 0x04000026 RID: 38
		private static CultureInfo resourceCulture;
	}
}
