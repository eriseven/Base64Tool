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
			
			Coder(args[1], encode);
		}

		private static void Coder(string filename, bool encode)
		{
			ICryptoTransform base64Transform;
			if (encode) base64Transform = new ToBase64Transform();
			else base64Transform = new FromBase64Transform();

			using (var input = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None))
			using (var output = new FileStream(filename + (encode ? ".encoded" : ".decoded"), FileMode.Create, FileAccess.Write, FileShare.None))
			using (var cryptoStream = new CryptoStream(input, base64Transform, CryptoStreamMode.Read))
			{
				long total;
				if (encode) total = (input.Length + 2 - ((input.Length + 2) % 3)) / 3 * 4;
				else total = (input.Length + 2 - ((input.Length + 2) % 3)) / 3 * 4;

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
					if (lastPercent != percent) Console.WriteLine((encode ? "Encoded: " : "Decoded: ") + percent + '%');
					lastPercent = percent;
				}
			}
		}
	}
}
