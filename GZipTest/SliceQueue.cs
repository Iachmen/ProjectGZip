using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    public class SliceQueue
    {
		public bool closed = false;
        object locker = new object();
        Queue queue;
        public SliceQueue()
        {
            queue = new Queue();
        }
        public void Enqueue(GzipSlice slice)
        {
			lock (locker)
            {
                queue.Enqueue(slice);
            }
        }
		public GzipSlice Dequeue()
        {
			lock (locker)
            {
				if (queue.Count == 0)
					return null;
                return (GzipSlice)queue.Dequeue();
            }
             
        }
		public void Close()
		{
			closed = true;
		}
		public bool IsEmpty()
		{
			return queue.Count == 0;
		}
    }
}
