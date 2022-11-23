using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO.Ports;
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace CiscoAutoConfig
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public bool isReady = true;
        public SerialPort sport;
        public MainWindow()
        {

            InitializeComponent();

            //Стандартные настройки для подклбючения по COM порту
            BaudRateDD.SelectedIndex = 0;
            ParityDD.SelectedIndex = 0;
            DatabitsDD.SelectedIndex = 4;
            StopBitsDD.SelectedIndex = 1;
            EquiptmentDD.SelectedIndex = 0;
            DelayDD.SelectedIndex = 2;
            PortTypeDD.SelectedIndex = 1;
            BaudRateDD.SelectedItem = 9600;


            CloseBTN.IsEnabled = false;
           
            foreach (String s in SerialPort.GetPortNames())
            {
                PortDD.Items.Add(s);
            }
        }


        /// <summary>
        /// Подключение по ком порту
        /// </summary>
        /// <param name="port">Имя порта</param>
        /// <param name="baudrate">Скорость передачи</param>
        /// <param name="parity">Протокол проверки четности</param>
        /// <param name="databits">Количество Бит для передачи</param>
        /// <param name="stopbits">Получает или задает стандартное количество стоповых битов на байт</param>
        private void serialport_connect(String port, int baudrate, Parity parity, int databits, StopBits stopbits)
        {
            DateTime dt = DateTime.Now;
            String dtn = dt.ToShortTimeString();

            sport = new SerialPort(
            port, baudrate, parity, databits, stopbits);
            try
            {
                sport.Open();
                sport.ReadTimeout = 0;
                sport.DtrEnable = true;
                sport.RtsEnable = true;

                CloseBTN.IsEnabled = true;
                ConnectBTN.IsEnabled = false;
                txtReceive.AppendText("\n[" + dtn + "] " + "Connected\n");
                sport.DataReceived += new SerialDataReceivedEventHandler(sPort_DataReceived);
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "Error"); }
        }

        /// <summary>
        /// Ивент обработки нажатия кнопки отключения
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void CloseBTN_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            String dtn = dt.ToShortTimeString();

            if (sport.IsOpen)
            {
                sport.Close();
                CloseBTN.IsEnabled = false;
                ConnectBTN.IsEnabled = true;
                txtReceive.AppendText("\n[" + dtn + "] " + "Disconnected\n");
            }
        }

        /// <summary>
        /// Получение данных по ком потру
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            String dtn = dt.ToShortTimeString();

            isReady = false;

            this.Dispatcher.Invoke(() =>
            {
                txtReceive.AppendText(sport.ReadExisting());
                txtReceive.ScrollToEnd();
            });
        }

        /// <summary>
        /// Ивент обработки нажатия кнопки отправки данных
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void SendBTN_Click(object sender, RoutedEventArgs e)
        {

            if(CloseBTN.IsEnabled)
            {
                sport.WriteLine(txtDatatoSend.Text);
                txtReceive.AppendText(txtDatatoSend.Text);
                txtDatatoSend.Text = "";
            }
            else
            {
                MessageBox.Show(@"Не было проведено подключение к Ком порту ¯\_(ツ)_/¯","AAAAAaaAaAAaAa");
            }

        }

        /// <summary>
        /// Ивент обработки нажатия кнопки подключения
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void ConnectBTN_Click(object sender, RoutedEventArgs e)
        {
            String port = PortDD.Text;
            int baudrate = Convert.ToInt32(BaudRateDD.Text);
            Parity parity = (Parity)Enum.Parse(typeof(Parity), ParityDD.Text);
            int databits = Convert.ToInt32(DatabitsDD.Text);
            StopBits stopbits = (StopBits)Enum.Parse(typeof(StopBits), StopBitsDD.Text);
            if(PortDD.Text != "")
            {
                serialport_connect(port, baudrate, parity, databits, stopbits);
            }
            else
            {
                MessageBox.Show(@"Не был выбран порт, к котоому подключатся ¯\_(ツ)_/¯", "AAAAAaaAaAAaAa");
            }
        }

        /// <summary>
        /// Форматирование ДНС по номеру Магазина
        /// </summary>
        /// <returns>Строка со сгенерированым DNS именем</returns>
        private string FindDNS()
        {

            while (DNStext.Text.Length != 4)
            {
                DNStext.Text = DNStext.Text.Insert(0, "0");
            }
            string dns = $"a{DNStext.Text}-router";
            return dns;
        }

        /// <summary>
        /// Поиск ИП по  ДНС
        /// </summary>
        /// <param name="dns">ДНС адрес узла, ип которого нужно найти</param>
        /// <returns>Строка с полкченым IP адресом</returns>
        private string FindIPByDNS(string dns)
        {
            string ip = Dns.GetHostEntry(dns).AddressList[0].ToString();

            IPtxt.Text = ip;
            return ip;
        }

        /// <summary>
        /// Просчет тунелей 
        /// </summary>
        private void FindTunels()
        {
           //В данном методе присутвовала приватная информация компании, которую пришлось удалить

        }

        /// <summary>
        /// Преобразовывает строку с IP адресом в целочисленный массив(Разбивает ИП по сегментам)
        /// </summary>
        /// <returns>Целочисленный массив с сегментами входящего ИП адреса</returns>
        private int[] IPtoINTArr()
        {
            string ip = FindIPByDNS(FindDNS());

            int[] iFormatIp = ip.Split('.').
            Where(x => !string.IsNullOrWhiteSpace(x)).
            Select(x => int.Parse(x)).ToArray();
            return iFormatIp;
        }
        /// <summary>
        /// Перевод массива чисел в строку
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        static string NumberArrayToString(int[] array)
        {
            string strIp = "";
            for (int i = 0; i < array.Length; i++)
            {
                if(i< array.Length - 1 )
                {
                    strIp += array[i].ToString() + ".";
                }
                else
                {
                    strIp += array[i].ToString();
                }
            }
            return strIp;
        }

        /// <summary>
        /// Ивент обработки нажатия кнопки генерации
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void GenerateBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IPSwitch();
                FindTunels();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не коректно введен номер магазина (По DNS {FindDNS()} IP адрес не найден)  | Error message - " + ex.Message, "AAaaAAAAaaaAAAaa");
            }
        }

        /// <summary>
        /// Ивент обработки нажатия кнопки считывания
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void ReadCFG_BTN_Click(object sender, RoutedEventArgs e)
        {
            if(IPtxt.Text != "IP (Auto)")
            {
                string path = EquiptmentDD.SelectedIndex == 0 ? @"..\CiscoAutoConfig\ROUTER.txt" : @"..\CiscoAutoConfig\SWITCH.txt";
                txtCFG.Text = "";

                //Считывание конфига с файла, и вывод в интерфейс
                using (StreamReader sr = new StreamReader(path, Encoding.Default))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = ParseInputCFG(line);

                        txtCFG.AppendText(line + "\n");
                        txtCFG.ScrollToEnd();
                    }
                }
            }
            else
            {
                MessageBox.Show(@"Не были сгенерированы ИП, и Тонели ¯\_(ツ)_/¯", "AAAAAaaAaAAaAa");
            }

        }

        /// <summary>
        /// Замена данных в считаном конфиге
        /// </summary>
        /// <param name="line">Строка для обработки</param>
        /// <returns></returns>
        private string ParseInputCFG(string line)
        {
            return line.Replace("{TUNEL1}", Tunel1txt.Text) //Тунель 1
                .Replace("{TUNEL2}", Tunel2txt.Text) //Тунель 2
                .Replace("{IP}", IPtxt.Text) //ИП
                .Replace("{PASS}", Passtxt.Text) //Пароль
                .Replace("{NUMTT}", DNStext.Text) //Номер Магазина(4 цифры)
                .Replace("{IP_RRO}", IPRRO()) //ИП РРО
                .Replace("{IP_TERM}", IPTerminal()) //ИП Терминал
                .Replace("{IP_SWITCH}", IPSwitchtxt.Text); //ИП Свитч
        }

        //TODO: обьеденить в один метод
        #region редактирование ИП (РРО, терминал, свитч)
        private string IPRRO()
        {
            int[] segmentIP= new[] { 11, 21, 31, 41 };
            string ipRRO = "";
            int[] IP = IPtoINTArr();

            for (int i = 0; i < segmentIP.Length; i++)
            {
                IP[3] = segmentIP[i];
                ipRRO += "host " + NumberArrayToString(IP) + "\n";
            }

            return ipRRO;
        }

        private string IPTerminal()
        {
            int[] segmentIP = new[] { 12, 22, 32, 42 };
            string IPTerminal = "";
            int[] IP = IPtoINTArr();

            for (int i = 0; i < segmentIP.Length; i++)
            {
                IP[3] = segmentIP[i];
                IPTerminal += "host " + NumberArrayToString(IP) + "\n";
            }

            return IPTerminal;
        }

        private string IPSwitch()
        {
            string IPSwitch = "";
            int[] IP = IPtoINTArr();

            IP[3] = 3;
            IPSwitch = NumberArrayToString(IP);

            IPSwitchtxt.Text = IPSwitch;
            return IPSwitch;
        }
        #endregion

        /// <summary>
        /// Ивент обработки нажатия кнопки отправки конфига
        /// </summary>
        /// <param name="sender">System WPF parametr</param>
        /// <param name="e">System WPF parametr</param>
        private void SendCFG_BTN_Click(object sender, RoutedEventArgs e)
        {
            int delay = 0;

            if(DelayDD.SelectedIndex == 0)
                delay = 100;
            else if(DelayDD.SelectedIndex == 1)
                delay = 500;
            else if(DelayDD.SelectedIndex == 2)
                delay = 1000;


            if (CloseBTN.IsEnabled)
            {
                var dataArr = txtCFG.Text.Split(new[] { "\n" }, StringSplitOptions.None);
                if((bool)waitCB.IsChecked)
                {
                    for (int i = 0; i < dataArr.Length - 1; i++)
                    {
                        isReady = true;
                        sport.WriteLine(dataArr[i]);
                        Dispatcher.Invoke(new Action(() => txtReceive.AppendText(dataArr[i])));
                        do
                        {

                            this.Dispatcher.Invoke(
                                DispatcherPriority.Background,
                                new ThreadStart(delegate { }));
                            Thread.Sleep(20);
                        }
                        while (isReady);

                        Thread.Sleep(delay);
                    }
                }
                else
                {
                    for (int i = 0; i < dataArr.Length - 1; i++)
                    {
                        this.Dispatcher.Invoke(DispatcherPriority.Background,new ThreadStart(delegate { }));
                        Thread.Sleep(2000);
                        sport.WriteLine(dataArr[i]);
                        Dispatcher.Invoke(new Action(() => txtReceive.AppendText(dataArr[i])));
                    }
                }

            }
            else
            {
                MessageBox.Show(@"Не было проведено подключение к Ком порту ¯\_(ツ)_/¯", "AAAAAaaAaAAaAa");
            }
        }
    }
}
