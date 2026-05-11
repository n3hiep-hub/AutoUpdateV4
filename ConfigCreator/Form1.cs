using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ConfigCreator
{
	// Token: 0x02000002 RID: 2
	public partial class Form1 : Form
	{
		private const string EncryptKey = "zxczxczxc";
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public Form1()
		{
			this.InitializeComponent();
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002134 File Offset: 0x00000334
		private void button1_Click(object sender, EventArgs e)
		{
			string text = string.Concat(new string[]
			{
				this.Version.Text,
				"\r\n",
				this.ServerUpdate.Text,
				"\r\n",
				this.Slides.Text,
				"\r\n",
				this.News.Text,
				"\r\n",
				this.GSIP.Text,
				"\r\n",
				this.GSPort.Text,
				"\r\n",
				this.FileGame.Text,
				"\r\n",
				this.AutoTrain.Text,
				"\r\n",
				this.AutoPK.Text,
				"\r\n",
				this.WebHome.Text,
				"\r\n",
				this.WebRegister.Text,
				"\r\n",
				this.WebSupport.Text,
				"\r\n",
				this.WebCom.Text,
				"\r\n"
			});
			text = SecureStringManager.Encrypt(text, EncryptKey);
			this.SaveFile("autoupdate.dat", text);
		}

		private void SaveFile(string FileName_, string Content_)
		{
			File.WriteAllText(FileName_, Content_);
			MessageBox.Show("Lưu Dữ Liệu Mới Thành Công.", "Thông Báo!");
		}


		private void button2_Click(object sender, EventArgs e)
		{
			string path = null;
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "LauncherConfig(autoupdate.dat)|autoupdate.dat"
			};
			string directoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			openFileDialog.InitialDirectory = directoryName;
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				path = openFileDialog.FileName;
				string decryptedText = SecureStringManager.Decrypt(File.ReadAllText(path), EncryptKey);
				if (decryptedText == "Mật khẩu mã hóa không chính xác!")
				{
					MessageBox.Show("Mật khẩu mã hóa không chính xác!", "Lỗi");
				}
				else
				{
					// Nếu mật khẩu đúng, gán giá trị đã giải mã vào các trường Text
					string[] array = decryptedText.Split(new[] { "\r\n" }, StringSplitOptions.None);
					if (array.Length >= 13) // Đảm bảo có đủ phần tử để tránh lỗi index out of range
					{
						this.Version.Text = array[0];
						this.ServerUpdate.Text = array[1];
						this.Slides.Text = array[2];
						this.News.Text = array[3];
						this.GSIP.Text = array[4];
						this.GSPort.Text = array[5];
						this.FileGame.Text = array[6];
						this.AutoTrain.Text = array[7];
						this.AutoPK.Text = array[8];
						this.WebHome.Text = array[9];
						this.WebRegister.Text = array[10];
						this.WebSupport.Text = array[11];
						this.WebCom.Text = array[12];
					}
					else
					{
						MessageBox.Show("Dữ liệu giải mã không đúng định dạng!", "Lỗi");
					}
				}
			}
			else
			{
				MessageBox.Show("không thể load file config", "Lỗi!");
			}
			
		}
	}
}
