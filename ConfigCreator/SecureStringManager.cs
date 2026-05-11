using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ConfigCreator
{
	// Token: 0x02000004 RID: 4
	internal class SecureStringManager
	{
		// Token: 0x06000009 RID: 9 RVA: 0x000035F4 File Offset: 0x000017F4
		public static string Encrypt(string plainText, string passPhrase)
		{
			byte[] bytes = Encoding.UTF8.GetBytes("tu89geji340t89u2");
			byte[] bytes2 = Encoding.UTF8.GetBytes(plainText);
			byte[] bytes3 = new PasswordDeriveBytes(passPhrase, null).GetBytes(32);
			ICryptoTransform transform = new RijndaelManaged
			{
				Mode = CipherMode.CBC
			}.CreateEncryptor(bytes3, bytes);
			MemoryStream memoryStream = new MemoryStream();
			CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
			cryptoStream.Write(bytes2, 0, bytes2.Length);
			cryptoStream.FlushFinalBlock();
			byte[] inArray = memoryStream.ToArray();
			memoryStream.Close();
			cryptoStream.Close();
			return Convert.ToBase64String(inArray);
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00003680 File Offset: 0x00001880
		public static string Decrypt(string cipherText, string passPhrase)
		{
			try
			{
				byte[] bytes = Encoding.ASCII.GetBytes("tu89geji340t89u2");
				byte[] array = Convert.FromBase64String(cipherText);
				byte[] bytes2 = new PasswordDeriveBytes(passPhrase, null).GetBytes(32);
				ICryptoTransform transform = new RijndaelManaged
				{
					Mode = CipherMode.CBC
				}.CreateDecryptor(bytes2, bytes);
				MemoryStream memoryStream = new MemoryStream(array);
				CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
				byte[] array2 = new byte[array.Length];
				int count = cryptoStream.Read(array2, 0, array2.Length);
				memoryStream.Close();
				cryptoStream.Close();
				return Encoding.UTF8.GetString(array2, 0, count);
			}
			catch (CryptographicException)
			{
				// Thông báo cho người dùng rằng mật khẩu mã hóa không đúng
				return "Mật khẩu mã hóa không chính xác!";
			}
		}

		// Token: 0x04000023 RID: 35
		private const string initVector = "tu89geji340t89u2";

		// Token: 0x04000024 RID: 36
		private const int keysize = 256;
	}
}
