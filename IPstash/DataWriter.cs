using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Xml.Linq;
namespace IP_Changer
{
    internal class DataWriter
    {
        public string activeID { get; private set; }
        public Dictionary<string, Object> resources { get; private set; }
        string rootPath;
        const string configPath = "/Data/Config.xml";
        const string resourcePath = "/Data/Resources.xml";
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
            initResouurces();
            if (File.Exists(rootPath + configPath))
            {
                string adapters = File.ReadAllText(rootPath + configPath);

                XDocument document;
                try
                {
                    document = XDocument.Load(rootPath + configPath);
                    var adaptList = document.Elements("Adapters");
                    var Adapters = adaptList.First();
                    activeID = Adapters.Attribute("currentID").Value;

                }
                catch (Exception ex)
                {
                    FileStream stream = File.Create(rootPath + configPath);
                    document = new XDocument();
                    XElement adapter = new XElement("Adapters");
                    adapter.SetAttributeValue("currentID", "");
                    document.Add(adapter);
                    document.Save(stream);

                }
            }
            else
            {
                FileStream stream = File.Create(rootPath + configPath);
                XDocument document = new XDocument();
                XElement adapter = new XElement("Adapters");
                adapter.SetAttributeValue("currentID", "");
                document.Add(adapter);
                document.Save(stream);
            }
        }
        private void initResouurces()
        {
            resources = new Dictionary<string, Object>();
            if (File.Exists(rootPath + resourcePath))
            {;

                XDocument document;
                try
                {
                    document = XDocument.Load(rootPath + resourcePath);
                    var ResElement = document.Element("Res");
                    var resourceList = ResElement.Elements();
                    foreach(var res in resourceList)
                    {
                        var kv = ResourcePrarser.parse(res);
                        resources.Add(kv.Key,kv.Value);
                    }
                }
                catch (Exception ex)
                {
                    save();
                }
            }
            else
            {
                save();
            }

        }

        public void update(String res,Object data)
        {
            resources.Add(res, data);
        }
        public Object get(String res)
        {
            return resources[res];
        }
        public void remove(String res)
        {
            try
            {
                resources.Remove(res);
            }
            catch { }
        }
        public void save()
        {
            XDocument document = new XDocument();
            
            XElement resElement = new XElement("Res");
            FileStream stream = File.Create(rootPath + resourcePath);
            foreach (var resource in resources)
            {
                var res = ResourcePrarser.serealize(resource);
                resElement.Add(res);
            }
            document.Add(resElement);
            try { 
                document.Save(stream); 
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
        public void write(String id)
        {
            activeID = id;
            try
            {
                FileStream stream = File.Create(rootPath + configPath);
                XDocument document = new XDocument();
                XElement adapter = new XElement("Adapters");
                adapter.SetAttributeValue("currentID", activeID);
                document.Add(adapter);
                document.Save(stream);
            }
            catch
            {

            }
        }
    }
}
