using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest
{
    public class GZipCompress : GZip
    {
		SliceQueue writerQueue = new SliceQueue();
		
        public GZipCompress(string source, string target)
            :base(source, target){}

        public override void Execute()
        {
			Thread readThread = new Thread(
					() =>Read());
			readThread.Start();
			Thread[] thread = new Thread[ThreadCount];
			for (int i = 0; i < ThreadCount; i++)
			{
				thread[i] = new Thread(Compress);
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
					while (sourceStream.Position < sourceStream.Length)
					{
						byte[] slices = new byte[BufferSize];
						int lenght = sourceStream.Read(slices, 0, BufferSize);
						GzipSlice gz = new GzipSlice(lenght);
						Array.Copy(slices, gz.Slice, lenght);
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
		private void Compress(object i)
		{
			try
			{
				while (!readerQueue.closed || !readerQueue.IsEmpty())
				{
					GzipSlice gz = readerQueue.Dequeue();
					if (gz != null)
					{
						CompressStreamParall(gz);
					}
				}
				waits[(int)i].Set();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
		private void CompressStreamParall(GzipSlice gz)
		{
			GzipSlice _out;
			using (MemoryStream mStream = new MemoryStream())
			{
				using (GZipStream gzStream = new GZipStream(mStream, CompressionLevel.Optimal))
				{
					gzStream.Write(gz.Slice, 0, gz.Slice.Length);
				}
				_out = new GzipSlice(gz.ID, mStream.ToArray());
				writerQueue.Enqueue(_out);
			}
		}
        public override void Write()
        {
			try
			{
				using (FileStream targetStream = new FileStream(TargetFile, FileMode.Create))
				{
					while (!canceled || !writerQueue.IsEmpty())
					{
						GzipSlice gz = writerQueue.Dequeue();
						if (gz != null)
						{
							targetStream.Write(GetSizeInBytes((int)gz.ID), 0, (GetSizeInBytes((int)gz.ID)).Length);
							targetStream.Write(GetSizeInBytes((int)gz.Slice.Length), 0, (GetSizeInBytes((int)gz.Slice.Length)).Length);
							targetStream.Write(gz.Slice, 0, gz.Slice.Length);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
        }
		private byte[] GetSizeInBytes(int size)
		{
			return BitConverter.GetBytes(size);
		}	
    }
}
