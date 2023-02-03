using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Xml;
namespace IP_Changer
{
    internal class DataWriter
    {
        public string activeID { get; private set; }
        string rootPath;
        public DataWriter()
        {
            string execPath = Assembly.GetEntryAssembly().Location;
            Debug.WriteLine("execPath: " + execPath);
            if (!File.Exists(execPath))
            {
                throw new Exception("Root folder does not exist:" + execPath);
            }
            rootPath = Directory.GetParent(execPath).FullName;
            Debug.WriteLine("root: " + rootPath);
            string[] subdirs = Directory.GetDirectories(rootPath);
            if (!Directory.Exists(rootPath + "/Data"))
            {
                Directory.CreateDirectory(rootPath + "/Data");
            }
            if (File.Exists(rootPath + "/Data/Config.xml"))
            {
                string adapters = File.ReadAllText(rootPath + "/Data/Config.xml");

                XmlDocument document = new XmlDocument();
                try
                {
                    document.LoadXml(adapters);
                    var adaptList = document.GetElementsByTagName("Adapters");
                    var Adapters = adaptList[0];
                    activeID = Adapters.Attributes["currentID"].Value;

                }
                catch (XmlException ex)
                {
                    FileStream stream = File.Create(rootPath + "/Data/Config.xml");
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Async = true;
                    settings.Encoding = Encoding.UTF8;
                    XmlElement adapter = document.CreateElement("Adapters");
                    XmlAttribute currentID = document.CreateAttribute("currentID");
                    currentID.InnerText = "";
                    adapter.SetAttributeNode(currentID);
                    document.AppendChild(adapter);
                    document.Save(stream);

                }
            }
            else
            {
                FileStream stream = File.Create(rootPath + "/Data/Config.xml");
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Async = true;
                settings.Encoding = Encoding.UTF8;
                XmlDocument document = new XmlDocument();
                XmlElement adapter = document.CreateElement("Adapters");
                XmlAttribute currentID = document.CreateAttribute("currentID");
                currentID.InnerText = "";
                adapter.SetAttributeNode(currentID);
                document.AppendChild(adapter);
                document.Save(stream);

            }
        }
        public void write(String id)
        {
            activeID = id;

            try
            {
                FileStream stream = File.Create(rootPath + "/Data/Config.xml");
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Async = true;
                settings.Encoding = Encoding.UTF8;
                XmlDocument document = new XmlDocument();
                XmlElement adapter = document.CreateElement("Adapters");
                XmlAttribute currentID = document.CreateAttribute("currentID");
                currentID.InnerText = activeID;
                adapter.SetAttributeNode(currentID);
                document.AppendChild(adapter);
                document.Save(stream);
            }
            catch
            {

            }
        }
    }
}
