using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace 服务器
{

    class Message
    {
        private byte[] data = new byte[1024];
        private int startIndex = 0;//我们存取了多少个字节的数据在数组里面
        public void AddCount(int count)
        {
            startIndex += count;
        }
        public byte[] Data
        {
            get
            {
                return data;
            }
        }
        public int StartIndex
        {
            get
            {
                return startIndex;
            }
        }
        public int RemindSize
        {
            get
            {
                return data.Length - startIndex;
            }
        }
    }
}
