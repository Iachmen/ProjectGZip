using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest
{
    public abstract class GZip
    {
        private int _threadCount;
        private int _size;
        private int _bufferSize;

        public int ThreadCount { get { return _threadCount; } }
        public int Size { get { return _size; } }
        public int BufferSize { get { return _bufferSize; } }
        protected string SourceFile { get; set; }
        protected string TargetFile { get; set; }
		protected SliceQueue readerQueue = new SliceQueue();
		protected ManualResetEvent[] waits;
		protected bool canceled = false;
        public GZip(string sourceFile, string targetFile)
        {
            _threadCount = Environment.ProcessorCount;
            _size = 4;
            _bufferSize = 1024*1024*4; //4Mb
            SourceFile = sourceFile;
            TargetFile = targetFile;
			waits = new ManualResetEvent[_threadCount];
        }

        public abstract void Execute();
        public abstract void Read();
        public abstract void Write();
    }
    
}
