using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GZipTest.Exceptions;

namespace GZipTest
{
	class GZipDecompress : GZip
	{
		Dictionary<int, byte[]> decompress = new Dictionary<int, byte[]>();
		int start = 0;
	
		public GZipDecompress(string source, string target): base(source, target)
		{

		}
		public override void Execute()
		{
			Thread readThread = new Thread(
					() => Read());
			readThread.Start();
			Thread[] thread = new Thread[ThreadCount];
			for (int i = 0; i < ThreadCount; i++)
			{
				thread[i] = new Thread(Decompress);
				waits[i] = new ManualResetEvent(false);
				thread[i].Start(i);
			}

			Thread writeThread = new Thread(() => Write());
			writeThread.Start();
			WaitHandle.WaitAll(waits);

			canceled = true;
			writeThread.Join();
		}

		public override void Read()
		{
			try
			{
				using (FileStream sourceStream = new FileStream(SourceFile, FileMode.Open, FileAccess.Read))
				{
					byte[] blockSize;
					while (sourceStream.Position < sourceStream.Length)
					{
						blockSize = new byte[Size];
						sourceStream.Read(blockSize, 0, Size); //информация о размере сжатого кусочка
						int readID = GetFromByteSize(blockSize);
						blockSize = new byte[Size];
						int readLenght = sourceStream.Read(blockSize, 0, Size);
						if (readLenght != Size)
						{
							throw new CompressionException("File is corrupted");
						}
						byte[] sliceFile = new byte[GetFromByteSize(blockSize)];
						int read = sourceStream.Read(sliceFile, 0, sliceFile.Length);
						if (read == 0)
						{
							throw new CompressionException("File is corrupted");
						}
						GzipSlice gz = new GzipSlice(readID, sliceFile);
						readerQueue.Enqueue(gz);
					}
					readerQueue.Close();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
			
		}
		private void Decompress(object i)
		{
			try
			{
				while (!readerQueue.closed || !readerQueue.IsEmpty())
				{
					GzipSlice gz = readerQueue.Dequeue();
					if (gz != null)
					{
						DecompressStreamParallel(gz);
					}
				}
				waits[(int)i].Set();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}			
		}
		private int GetFromByteSize(byte[] blockSize)
		{
			return BitConverter.ToInt32(blockSize, 0);
			
		}
		public override void Write()
		{
			try
			{
				using (FileStream targetStream = new FileStream(TargetFile, FileMode.Create))
				{
					while (!canceled || decompress.Count != 0)
					{
						if (decompress.ContainsKey(start))
						{
							var slice = decompress.Where(c => c.Key == start).First().Value;
							targetStream.Write(slice, 0, slice.Length);
							decompress.Remove(start);
							start++;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}	
		}
		private void DecompressStreamParallel(GzipSlice gz)
		{
			byte[] buffer = new byte[BufferSize];
			using (MemoryStream mstream = new MemoryStream(gz.Slice))
			using (GZipStream gzstream = new GZipStream(mstream, CompressionMode.Decompress, true))
			{
				int lenght = gzstream.Read(buffer, 0, buffer.Length);
				gz.Slice = new byte[lenght];
				Array.Copy(buffer, gz.Slice, lenght);
				decompress.Add(gz.ID, gz.Slice);
			}
		}
	}
}
