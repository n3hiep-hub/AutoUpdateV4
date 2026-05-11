using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Ini
{
    // Token: 0x02000002 RID: 2
    public class IniFile
    {
        // Token: 0x06000001 RID: 1
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        // Token: 0x06000002 RID: 2
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        // Token: 0x06000003 RID: 3 RVA: 0x00002050 File Offset: 0x00000250
        public IniFile(string INIPath)
        {
            this.path = INIPath;
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002061 File Offset: 0x00000261
        public void IniWriteValue(string Section, string Key, string Value)
        {
            IniFile.WritePrivateProfileString(Section, Key, Value, this.path);
        }

        // Token: 0x06000005 RID: 5 RVA: 0x00002074 File Offset: 0x00000274
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder stringBuilder = new StringBuilder(255);
            int privateProfileString = IniFile.GetPrivateProfileString(Section, Key, "", stringBuilder, 255, this.path);
            return stringBuilder.ToString();
        }

        // Token: 0x04000001 RID: 1
        public string path;
    }
}
