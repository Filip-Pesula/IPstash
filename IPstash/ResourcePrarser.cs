using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace IPstash
{
    
    public class ResourcePrarser
    {
        public static KeyValuePair<string, Object> parse(XElement res)
        {
            switch (res.Name.LocalName)
                //GetNamedItem("type").Value)
            {
                case "SocketDataset":
                    {
                        return parseSocketDataset(res);
                    }
                default:
                    throw new FormatException(res.Name.LocalName + " Is unknow class type");
            }
        }
        public static XElement serealize(KeyValuePair<string,Object> resource)
        {
            switch (resource.Value)
                //GetNamedItem("type").Value)
            {
                case SocketDataset soc:
                    {
                        return serealizeSocketDataset(resource);
                    }
                default:
                    throw new FormatException(resource.GetType().FullName + " Is unknow class type");
            }
        }

        public static KeyValuePair<string, Object> parseSocketDataset(XElement res)
        {
            try
            {
                string ipAddress = res.Element("ipAddress").Value;
                string mask = res.Element("mask").Value;
                string defaultGateway = res.Element("defaultGateway").Value;
                return new KeyValuePair<string, Object>(res.Attribute("Name").Value ,new SocketDataset(ipAddress, mask, true, defaultGateway, 0));
            }
            catch (Exception ex)
            {
                throw new FormatException("Could not parse Socket Dataset", ex);
            }
        }
        public static XElement serealizeSocketDataset(KeyValuePair<string, Object> resource)
        {
            SocketDataset data =  (SocketDataset)resource.Value;
            XElement res = new XElement("SocketDataset");
            res.Add(
                new XElement("ipAddress", data.ipAddress), 
                new XElement("mask", data.mask),
                new XElement("defaultGateway", data.defaultGateway)
                );
            res.SetAttributeValue("Name", resource.Key);
            return res; 
        }
    }
}
