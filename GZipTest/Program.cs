using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
			try
			{
				string func = args[0];
				string sourceFile = args[1];
				string targetFile = args[2];
				GZip gZip;
				Console.CancelKeyPress += (sender, e) =>
				{ e.Cancel = (e.SpecialKey == ConsoleSpecialKey.ControlC) ? false : true; };

				switch (func.ToLower())
				{
					case "compress": targetFile += ".compress";
						gZip = new GZipCompress(sourceFile, targetFile);
						break;
					case "decompress":
						gZip = new GZipDecompress(sourceFile, targetFile);
						break;
					default:
						throw new Exception("Error with command");
				}
				gZip.Execute();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			if (e.SpecialKey == ConsoleSpecialKey.ControlC)
			{
				e.Cancel = false;
			}
		}
    }
}
