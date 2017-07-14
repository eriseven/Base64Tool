using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Base64Tool
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args == null || args.Length != 2)
			{
				Console.WriteLine("Usage args: [encode|decode] <in-filename>" + Environment.NewLine + "Output will be <in-filename.[encoded|decoded]>");
				return;
			}

			bool encode;
			if (args[0] == "encode") encode = true;
			else if (args[0] == "decode") encode = false;
			else
			{
				Console.WriteLine("Invalid arg: " + args[0]);
				return;
			}

			if (encode) Encode(args[1]);
			else Decode(args[1]);
		}

		private static void Encode(string filename)
		{
			var base64Transform = new ToBase64Transform();
			using (var input = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None))
			using (var output = new FileStream(filename + ".encoded", FileMode.Create, FileAccess.Write, FileShare.None))
			using (var cryptoStream = new CryptoStream(input, base64Transform, CryptoStreamMode.Read))
			{
				long total = (input.Length + 2 - ((input.Length + 2) % 3)) / 3 * 4;
				var buffer = new byte[42 * 1024 * base64Transform.InputBlockSize];
				long totalBytesRead = 0;

				int lastPercent = -1;
				while (!cryptoStream.HasFlushedFinalBlock)
				{
					Array.Clear(buffer, 0, buffer.Length);
					int bytesRead = cryptoStream.Read(buffer, 0, buffer.Length);
					totalBytesRead += bytesRead;
					output.Write(buffer, 0, bytesRead);

					int percent = (int)((totalBytesRead / (double)total) * 100.0);
					if (lastPercent != percent) Console.WriteLine("Encoded: " + percent + '%');
					lastPercent = percent;
				}
			}
		}

		private static void Decode(string filename)
		{
			var base64Transform = new FromBase64Transform();
			using (var input = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None))
			using (var output = new FileStream(filename + ".decoded", FileMode.Create, FileAccess.Write, FileShare.None))
			using (var cryptoStream = new CryptoStream(input, base64Transform, CryptoStreamMode.Read))
			{
				long total = (input.Length / 4) * 3;
				var buffer = new byte[42 * 1024 * base64Transform.InputBlockSize];
				long totalBytesRead = 0;

				int lastPercent = -1;
				while (!cryptoStream.HasFlushedFinalBlock)
				{
					Array.Clear(buffer, 0, buffer.Length);
					int bytesRead = cryptoStream.Read(buffer, 0, buffer.Length);
					totalBytesRead += bytesRead;
					output.Write(buffer, 0, bytesRead);

					int percent = (int)((totalBytesRead / (double)total) * 100.0);
					if (lastPercent != percent) Console.WriteLine("Decoded: " + percent + '%');
					lastPercent = percent;
				}
			}
		}
	}
}
