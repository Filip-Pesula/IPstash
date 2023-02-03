using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IP_Changer
{
    internal class SocketDataset
    {
        public String ipAddress { get;  set; }
        public String mask { get;  set; }
        public bool isStatic { get;  set; }
        public String defaultGateway { get;  set; }
        public int adapterIndex { get;  set; }
        public SocketDataset(String ipAddress,String mask,bool isStatic,String defaultGateway,int adapterIndex)
        {
            this.ipAddress = ipAddress;
            this.mask = mask;
            this.isStatic = isStatic;
            this.defaultGateway = defaultGateway;
            this.adapterIndex = adapterIndex;
        }
    }
}
