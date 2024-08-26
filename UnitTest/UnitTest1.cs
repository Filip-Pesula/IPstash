﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using IP_Changer;
using System.Xml.Linq;
using System.Collections.Generic;

namespace IP_Changer.Tests
{
    [TestClass()]
    public class UnitTest1
    {
        [TestMethod()]
        public void serealizeSocketDatasetTest()
        {
            SocketDataset socket = new IP_Changer.SocketDataset("192.168.1.2", "255.255.255.0", true, "192.168.1.1", 0);
            XElement xelm = IP_Changer.ResourcePrarser.serealizeSocketDataset(new KeyValuePair<string, object> ("obj1",socket));
            var res = ResourcePrarser.parseSocketDataset(xelm);
            SocketDataset parsedSocket = (SocketDataset)res.Value;
            Console.WriteLine(xelm);
            Assert.AreEqual("SocketDataset", xelm.Name.LocalName);
            Assert.AreEqual("obj1", xelm.Attribute("Name").Value);
            Assert.AreEqual(socket.ipAddress, parsedSocket.ipAddress);
            Assert.AreEqual(socket.mask, parsedSocket.mask);
            Assert.AreEqual(socket.defaultGateway, parsedSocket.defaultGateway);
            Assert.AreEqual(socket.isStatic, parsedSocket.isStatic);
            Assert.AreEqual(socket.adapterIndex, parsedSocket.adapterIndex);
        }
    }
}
