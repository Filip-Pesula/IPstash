using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace IPstash
{
    internal class NetInterdaceData
    {
        private String netIntefaceId;
        public String name { get; }
        public NetInterdaceData(NetworkInterface networkInterface)
        {
            netIntefaceId = networkInterface.Id;
            name = networkInterface.Name;
        }
        public virtual bool Equals(NetworkInterface other)
        {
            return netIntefaceId == other.Id;
        }
        public static bool operator  ==(NetInterdaceData a, NetworkInterface other){
            return a.netIntefaceId == other.Id;
        }
        public static bool operator !=(NetInterdaceData a, NetworkInterface other){
            return a.netIntefaceId != other.Id;
        }
    }
}
