using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Management;
using System.Diagnostics;
using System.Windows.Threading;

namespace IP_Changer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<NetworkInterface> intefaces = new List<NetworkInterface>();
        DataWriter dataWriter;
        SocketDataset dataset = new SocketDataset("0.0.0.0", "0.0.0.0",false, "0.0.0.0",-1);
        public MainWindow()
        {
            dataWriter = new DataWriter();
            InitializeComponent();
            /*
            Button selectIP = new Button();
            selectIP.Content = "014.184.488.475";
            selectIP.Click += Button_selectIP;
            selectIP.HorizontalAlignment = HorizontalAlignment.Stretch;
            selectIP.Width = IPselection.Width;
            IPselection.Children.Add(selectIP);
            Button selectIP1 = new Button();
            selectIP1.Content = "014.184.488.475";
            selectIP1.Click += Button_selectIP;
            selectIP1.HorizontalAlignment = HorizontalAlignment.Stretch;
            selectIP1.Width = IPselection.Width;
            IPselection.Children.Add(selectIP1);
            */

            FindIntefaces(NetworkInterfaceType.Ethernet,ref SocketList);
            SocketList.SelectedIndex = findInterface(dataWriter.activeID);
            foreach (var res in dataWriter.resources)
            {
                ipGrid.Items.Add(res.Key);
            }
            SocketList_SelectionChanged(null, null);


        }
        void FindIntefaces(NetworkInterfaceType _type,ref ComboBox sl)
        {
            intefaces = new List<NetworkInterface>();
            sl.Items.Clear();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                Debug.WriteLine(item.Id+":"+item.Name);
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    intefaces.Add(item);
                    sl.Items.Add(item.Name);
                }
            }
        }
        void Button_selectIP(object sender, RoutedEventArgs e)
        {
            if (setDhcp.IsChecked.Value)
            {
                IpInput.Text = ((Button)sender).Content.ToString();
            }
        }
        int findInterface(string Id)
        {
            int indexe = 0;
            foreach (NetworkInterface interf in intefaces)
            {
                if (interf.Id == Id)
                {
                    return indexe;
                }

                indexe++;
            }
            return -1;
        }
        private void SocketList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SocketList.SelectedIndex == -1)
            {
                return;
            }
            var Iinterface = intefaces[SocketList.SelectedIndex];
            //Debug.WriteLine("adressCout" + ": " + Iinterface.GetIPProperties().);
            int index = -1;
            {
                int subindex = 0;
                foreach (UnicastIPAddressInformation ip in Iinterface.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        index = subindex;
                        break;
                    }
                    subindex++;
                }
            }
            if (index>-1)
            {
                UnicastIPAddressInformation ip = Iinterface.GetIPProperties().UnicastAddresses[index];
                
                string ipAddress = "0.0.0.0";
                string mask = "0.0.0.0";
                string gateway = "0.0.0.0";
                bool isStatic = !Iinterface.GetIPProperties().GetIPv4Properties().IsDhcpEnabled;
                if (Iinterface.GetIPProperties().GetIPv4Properties() == null)
                {
                    isStatic = true;
                }
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ip.Address.ToString();
                }
                if (ip.IPv4Mask.AddressFamily == AddressFamily.InterNetwork)
                {
                    mask = ip.IPv4Mask.ToString();
                }
                if (Iinterface.GetIPProperties().GatewayAddresses.Count > 0 && Iinterface.GetIPProperties().GatewayAddresses[0].Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    gateway = Iinterface.GetIPProperties().GatewayAddresses[0].Address.ToString();
                }
                dataset = new SocketDataset(ipAddress,mask, isStatic,gateway, Iinterface.GetIPProperties().GetIPv4Properties().Index);
            }
            Button_Revert(null, null);
        }
        void setIO(bool isStatic)
        {
            if (!isStatic)
            {
                setDhcp.IsChecked = false;
                IpInput.IsReadOnly = true;
                IpInput.Background = new SolidColorBrush(Colors.LightGray);
                maskInput.IsReadOnly = true;
                maskInput.Background = new SolidColorBrush(Colors.LightGray);
                gatewayInput.IsReadOnly = true;
                gatewayInput.Background = new SolidColorBrush(Colors.LightGray);
            }
            else
            {
                setDhcp.IsChecked = true;
                IpInput.IsReadOnly = false;
                IpInput.Background = new SolidColorBrush(Colors.White);
                maskInput.IsReadOnly = false;
                maskInput.Background = new SolidColorBrush(Colors.White);
                gatewayInput.IsReadOnly = false;
                gatewayInput.Background = new SolidColorBrush(Colors.White);
            }
        }
        //[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, AllowMultiple = false)]
        public void SetIP(string ipAddress, string subnetMask, string gateway, int adapterIndex)
        {
            Debug.WriteLine("ipAddress: " + ipAddress);
            Debug.WriteLine("subnetMask: " + subnetMask);
            Debug.WriteLine("gateway: " + gateway);
            Debug.WriteLine("adapterIndex: " + adapterIndex);
            using (var networkConfigMng = new System.Management.ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                using (var networkConfigs = networkConfigMng.GetInstances())
                {
                    foreach (var managementObject in networkConfigs.Cast<ManagementObject>().Where(managementObject => (bool)managementObject["IPEnabled"]))
                    {
                        Debug.WriteLine("selectIndex: " + managementObject.GetPropertyValue("index"));
                        //Debug.WriteLine("selectInterfaceIndex: " + managementObject.GetPropertyValue("InterfaceIndex"));
                        Debug.WriteLine("selectAdresses: " + String.Join("", managementObject.GetPropertyValue("IPAddress")) );
                        if ((uint)managementObject.GetPropertyValue("InterfaceIndex") == adapterIndex)
                        {

                            using (var newIP = managementObject.GetMethodParameters("EnableStatic"))
                            {
                                //INetCfgLock. AcquireWriteLock(
                                // Set new IP address and subnet if needed
                                if ((!String.IsNullOrEmpty(ipAddress)) || (!String.IsNullOrEmpty(subnetMask)))
                                {
                                    if (!String.IsNullOrEmpty(ipAddress))
                                    {
                                        newIP["IPAddress"] = new[] { ipAddress };
                                    }
                                    if (!String.IsNullOrEmpty(subnetMask))
                                    {
                                        newIP["SubnetMask"] = new[] { subnetMask };
                                    }
                                    System.Management.InvokeMethodOptions ops = new InvokeMethodOptions();
                                    ops.Timeout = new TimeSpan(1000);
                                    uint retval = (uint)managementObject.InvokeMethod("EnableStatic", newIP, ops)["ReturnValue"];
                                    Debug.WriteLine("EnableStaticOutput: " + retval);
                                }

                                // Set mew gateway if needed
                                if (!String.IsNullOrEmpty(gateway))
                                {
                                    using (var newGateway = managementObject.GetMethodParameters("SetGateways"))
                                    {
                                        newGateway["DefaultIPGateway"] = new[] { gateway };
                                        newGateway["GatewayCostMetric"] = new[] { 1 };
                                        uint retval = (uint)managementObject.InvokeMethod("SetGateways", newGateway, null)["ReturnValue"];
                                        Debug.WriteLine("SetGatewaysOutput: " + retval);                           
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public void setDHCP(int adapterIndex)
        {
            using (var networkConfigMng = new System.Management.ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                using (var networkConfigs = networkConfigMng.GetInstances())
                {
                    foreach (var managementObject in networkConfigs.Cast<ManagementObject>().Where(managementObject => (bool)managementObject["IPEnabled"]))
                    {
                        Debug.WriteLine("selectIndex: " + managementObject.GetPropertyValue("index"));
                        //Debug.WriteLine("selectInterfaceIndex: " + managementObject.GetPropertyValue("InterfaceIndex"));
                        Debug.WriteLine("selectAdresses: " + String.Join("", managementObject.GetPropertyValue("IPAddress")));
                        if ((uint)managementObject.GetPropertyValue("InterfaceIndex") == adapterIndex)
                        {
                            using (var newDHCP = managementObject.GetMethodParameters("EnableDHCP"))
                            {
                                //INetCfgLock. AcquireWriteLock(
                                // Set new IP address and subnet if needed
                                uint retVal = (uint)managementObject.InvokeMethod("EnableDHCP", newDHCP, null)["ReturnValue"];
                                Debug.WriteLine("outDHCP: " + retVal);
                            }
                        }
                    }
                }
            }
        }


        public bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }
        /**
         * set socket to static IP
         */
        private void setDhcp_Checked(object sender, RoutedEventArgs e)
        {
            setIO(true);
        }
        /**
         * set socket to DHCP
         */
        private void setDhcp_Unchecked(object sender, RoutedEventArgs e)
        {
            setIO(false);
        }

        private void Button_Aply(object sender, RoutedEventArgs e)
        {
            if(setDhcp.IsChecked.Value)
            {
                SetIP(IpInput.Text,maskInput.Text, gatewayInput.Text,dataset.adapterIndex);
            }
            else{
                setDHCP(dataset.adapterIndex);
            }
            string currentId = intefaces[SocketList.SelectedIndex].Id;
            FindIntefaces(NetworkInterfaceType.Ethernet, ref SocketList);
            SocketList.SelectedIndex = findInterface(currentId);
            //SocketList_SelectionChanged(sender,null);
        }

        private void Button_Revert(object sender, RoutedEventArgs e)
        {
            IpInput.Text = dataset.ipAddress;
            maskInput.Text = dataset.mask;
            gatewayInput.Text = dataset.defaultGateway;
            setIO(dataset.isStatic);
        }

        private void Button_Reload(object sender, RoutedEventArgs e)
        {
            if (SocketList.SelectedIndex == -1)
            {
                return;
            }
            string currentId = intefaces[SocketList.SelectedIndex].Id;
            FindIntefaces(NetworkInterfaceType.Ethernet, ref SocketList);
            SocketList.SelectedIndex = findInterface(currentId);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            String activeID = "";
            try
            {
                activeID = intefaces[SocketList.SelectedIndex].Id;
            }
            catch
            {}
            dataWriter.write(activeID);
            dataWriter.save();
        }

        private void Button_OpenIPMenu(object sender, RoutedEventArgs e)
        {
            
            if(IPselection.Visibility == Visibility.Visible)
            {
                IPselection.Visibility = Visibility.Hidden;
                root.Width -= 141;
                ((Button)sender).Content = ">>";
            }
            else
            {
                IPselection.Visibility = Visibility.Visible;
                root.Width += 141;
                ((Button)sender).Content = "<<";
            }
        }

        private void StartCloseTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2d);
            timer.Tick += TimerTick;
            timer.Start();
        }
        private void TimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Stop();
            timer.Tick -= TimerTick;
            this.myPopup.IsOpen = false;
        }
        private void Add_Set(object sender, RoutedEventArgs e)
        {
            try {
                dataWriter.update(
                    IpInput.Text,
                    new SocketDataset(
                        IpInput.Text,
                        maskInput.Text,
                        true, 
                        gatewayInput.Text,
                        0));
                ipGrid.Items.Add(IpInput.Text);
            }
            catch(Exception ex)
            {
                myPopup.IsOpen = true;
                StartCloseTimer();
            }
        }


        private void selectIP(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem senderLV = (ListViewItem)sender;
            Debug.WriteLine("IP.selected");
            Debug.WriteLine(e);
            Debug.WriteLine(senderLV);
            SocketDataset selectedDataset;
            try
            {
                string name = (string)(senderLV.Content);
                selectedDataset = (SocketDataset)dataWriter.get(name);
            }
            catch (Exception ex)
            {
                selectedDataset = dataset;
                Debug.WriteLine(ex.ToString());
            }
            dataset.ipAddress = selectedDataset.ipAddress;
            dataset.mask = selectedDataset.mask;
            dataset.defaultGateway = selectedDataset.defaultGateway;
            dataset.adapterIndex = dataset.adapterIndex;
            dataset.isStatic = true;
            Button_Revert(null, null);
        }

        private void deleteItem(object sender, RoutedEventArgs e)
        {
            dataWriter.remove((string)ipGrid.SelectedItem);
            ipGrid.Items.Remove(ipGrid.SelectedItem);
        }

        private void select(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("IP.selected");
            Debug.WriteLine(e);
            Debug.WriteLine((string)ipGrid.SelectedItem);
            SocketDataset selectedDataset;
            try
            {
                string name = (string)ipGrid.SelectedItem;
                selectedDataset = (SocketDataset)dataWriter.get(name);
            }
            catch (Exception ex)
            {
                selectedDataset = dataset;
                Debug.WriteLine(ex.ToString());
            }
            dataset.ipAddress = selectedDataset.ipAddress;
            dataset.mask = selectedDataset.mask;
            dataset.defaultGateway = selectedDataset.defaultGateway;
            dataset.adapterIndex = dataset.adapterIndex;
            dataset.isStatic = true;
            Button_Revert(null, null);
        }
    }
}
