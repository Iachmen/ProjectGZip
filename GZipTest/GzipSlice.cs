using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    public class GzipSlice
    {
        static int IdCount = 0;
        public int ID { get; set; }
        public byte[] Slice { get; set; }

        public GzipSlice(int size)
        {
            Slice = new byte[size];
            ID = IdCount++;
        }
		public GzipSlice(int id, byte[] slice)
		{
			ID = id;
			Slice = slice;
		}
    }
}
