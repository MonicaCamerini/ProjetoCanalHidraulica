using System;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Windows.Threading;
using System.Net;
using System.IO;
using ProjetoCanaldeHidraulica.CadastrodoUsuario;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.io;
using iTextSharp.text.api;
using Font = iTextSharp.text.Font;
using Paragraph = iTextSharp.text.Paragraph;

namespace ProjetoCanaldeHidraulica                                               
{

    public partial class Window1 : Window
    {


        SerialPort serialPort1 = new SerialPort();
        public const string PortaCom = ("-");


        public Window1()
        {
            InitializeComponent();

            Conexao con = new Conexao();

            try
            {
                con.conectar();


                string sqlite = "SELECT Vertedor FROM AtualizacaoVariaveis WHERE Vertedor IS NOT NULL";
                var cmd = new SQLiteCommand(sqlite, con.conn2);
                SQLiteDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    UltimoVertedor.Text = rdr["Vertedor"].ToString();
                    Condicao4.Fill = System.Windows.Media.Brushes.Red;

                }

            }
            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro56", MessageBoxButton.OK, MessageBoxImage.Error);

            }

            con.desconectar();
        }

        public Window1(string ValorVertedor)
        {
            InitializeComponent();
            ValorVertedor = "";
            LeituraNivelSensor1.Text = ValorVertedor.ToString();

        }


        #region Bloco de funções

        /// <summary>
        /// Function 03 (03hex) Read Holding Registers
        /// </summary>
        /// <param name="slaveAddress">Slave Address</param>
        /// <param name="startAddress">Starting Address</param>
        /// <param name="function">Function</param>
        /// <param name="numberOfPoints">Quantity of inputs</param>
        /// <returns>Byte Array</returns>
        byte[] ReadHoldingRegistersMsg(byte slaveAddress, ushort startAddress, byte function, uint numberOfPoints)
        {
            byte[] frame = new byte[8];
            frame[0] = slaveAddress;                // Endereço do escravo
            frame[1] = function;                    // Código de função             
            frame[2] = (byte)(startAddress >> 8);   // Starting Address High
            frame[3] = (byte)startAddress;          // Starting Address Low            
            frame[4] = (byte)(numberOfPoints >> 8); // Quantity of Registers High
            frame[5] = (byte)numberOfPoints;        // Quantity of Registers Low
            byte[] crc = CalculateCRC(frame);       // Calculate CRC
            frame[frame.Length - 2] = crc[0];       // Error Check Low
            frame[frame.Length - 1] = crc[1];       // Error Check High
            return frame;
        }

        /// <summary>
        /// Function 06 (06hex) Write Holding Registers
        /// </summary>
        /// <param name="slaveAddress">Slave Address</param>
        /// <param name="startAddress">Starting Address</param>
        /// <param name="function">Function</param>
        /// <param name="numberOfPoints">Quantity of inputs</param>
        /// <returns>Byte Array</returns>
        byte[] WriteHoldingRegistersMsg(byte slaveAddress, ushort startAddress, byte function, byte[] values)
        {
            byte[] frame = new byte[8];
            frame[0] = slaveAddress;                // Endereço do escravo
            frame[1] = function;                    // Código de função             
            frame[2] = (byte)(startAddress >> 8);   // Starting Address High
            frame[3] = (byte)startAddress;          // Starting Address Low            
            Array.Copy(values, 0, frame, 4, values.Length);
            byte[] crc = CalculateCRC(frame);       // Calculate CRC
            frame[frame.Length - 2] = crc[0];       // Error Check Low
            frame[frame.Length - 1] = crc[1];       // Error Check High
            return frame;
        }


        /// <summary>
        /// Calculo do CRC
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        byte[] CalculateCRC(byte[] data) //data trocado para frame
        {
            ushort CRCFull = 0xFFFF; // Set the 16-bit register (CRC register) = FFFFH.
            byte CRCHigh = 0xFF;
            byte CRCLow = 0xFF;
            char CRCLSB;
            byte[] CRC = new byte[2];
            for (int i = 0; i < (data.Length) - 2; i++)
            {
                CRCFull = (ushort)(CRCFull ^ data[i]); // 

                for (int j = 0; j < 8; j++)
                {
                    CRCLSB = (char)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }
            CRC[1] = CRCHigh = (byte)((CRCFull >> 8) & 0xFF);
            CRC[0] = CRCLow = (byte)(CRCFull & 0xFF);
            return CRC;
        }

        /// <summary>
        /// Display Data
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Message</returns>
        string Display(byte[] data)
        {
            string result = string.Empty;
            foreach (var item in data)
            {
                result += string.Format("{0:X2}", item);
            }
            return result;
        }

        /// <summary>
        /// Convert UInt16 to byte array
        /// </summary>
        /// <param name="value">UInt16</param>
        /// <returns>byte[]</returns>
        byte[] ToByteArray(UInt16 value)
        {
            byte[] array = BitConverter.GetBytes(value);
            Array.Reverse(array);
            return array;
        }

        #endregion Bloco de funções 


        #region Janela de Leitura de Variáveis 
        private void LerDados_Click(object sender, RoutedEventArgs e)
        {
            if (PortaSerialCom.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione a Porta COM!", "Atenção!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var PassagemCom = PortaSerialCom.Text; //A variável PassagemCom recebe a porta de comunicação selecionada.
                Leitura_de_variaveis leitura = new Leitura_de_variaveis(PassagemCom); //Passo uma string contendo o valor COMX (x corresponde ao número da porta COM selecionada)
                leitura.Show();
            }
        }
        #endregion Janela de Leitura de Variáveis 


        #region Janela de Alarme e Falhas
        private void Alarme_Click(object sender, RoutedEventArgs e)
        {
            if (PortaSerialCom.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione a Porta COM!", "Atenção!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else {
                var PassagemCom2 = PortaSerialCom.Text; //A variável PassagemCom recebe a porta de comunicação selecionada.
                Alarme alarme = new Alarme(PassagemCom2);
                alarme.Show();
            }
        }
        #endregion Janela de Alarme e Falhas 


        #region Janela Sobre
        private void Sobre_Click(object sender, RoutedEventArgs e)
        {
            Sobre sobre = new Sobre();
            sobre.Show();
        }

        #endregion Janela Sobre


        #region Janela Ajuda
        private void Ajuda_Click(object sender, RoutedEventArgs e)
        {
            Ajuda ajuda = new Ajuda();
            ajuda.Show();
        }

        #endregion Janela Ajuda


        #region Janela de Vazão 
        private void Vazão_Click(object sender, RoutedEventArgs e)
        {
            if (PortaSerialCom.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione a Porta COM!", "Atenção!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var PassagemCom3 = PortaSerialCom.Text; //A variável PassagemCom recebe a porta de comunicação selecionada.
                VazãoeNível vazãoeNível = new VazãoeNível(PassagemCom3);
                vazãoeNível.Show();
            }
        }
        #endregion Janela de Vazão


        #region Janela de Parametrização
        private void Parametrização_Click(object sender, RoutedEventArgs e)
        {
            Parametrizacao parametrizacao = new Parametrizacao();
            parametrizacao.Show();
        }
        #endregion Janela de Parametrização 


        #region Botão Voltar para Login
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        //private void RecebeUsuario_TextChanged(object sender, EventArgs e)
        //{
        //    MainWindow login = new MainWindow();
        //    login.CaixaUsuario.Text = RecebeUsuario.Text;
        //}

        #endregion Botão Voltar para Login


        #region Menu
        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {

            CloseMenu.Visibility = Visibility.Visible;
            OpenMenu.Visibility = Visibility.Collapsed;
        }

        private void CloseMenu_Click(object sender, RoutedEventArgs e)
        {
            CloseMenu.Visibility = Visibility.Collapsed;
            OpenMenu.Visibility = Visibility.Visible;
        }
        #endregion Menu
            

        #region Botão Acionamento
        private void Acionamento_Click(object sender, RoutedEventArgs e)
        {
            if (PortaSerialCom.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione a Porta COM!", "Atenção!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var PassagemCom2 = PortaSerialCom.Text; //A variável PassagemCom recebe a porta de comunicação selecioanda.
                Acionamento acionamento = new Acionamento(PassagemCom2);
                acionamento.Show();
            }
        }
        #endregion Botão Acionamento


        #region Botão de Emergência

        private void EmergenciaBotao1_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Deseja acionar o botão de EMERGÊNCIA?", "Confirmação", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {

                #region Bits de entrada para acionar o motor 

                byte menosSignificativo = 0b00000100;
                byte maisSignificativo = 0b00000000;

                int BotaoEmergencia = (maisSignificativo << 8) | menosSignificativo;

                #endregion Bits de entrada para acionar o motor

                try
                {

                    serialPort1 = new SerialPort(PortaSerialCom.Text, 57600, Parity.Even, 8, StopBits.One)
                    {
                        DtrEnable = true,
                        RtsEnable = true
                    };                                                                          //definição dos dados da porta serial 
                    serialPort1.Open();                                                         //abre uma nova conexão na porta serial 
                }
                catch (Exception ex)                                                            //erros que acontecem enquanto inicializa a porta serial são chamados de "ex"
                {
                    MessageBox.Show(ex.Message);                                                //descreve uma mensagem sobre o estado atual 
                }

                try
                {
                    //descreve o conjunto de informações sobre o escravo Modbus (inversor de frequência)
                    byte slaveAddress = 246;                                                     //endereço do escravo  
                    byte function = 6;                                                           //código do de função Modbus
                    ushort startAddress = 682;                                                   //registrador
                    ushort value = Convert.ToUInt16(BotaoEmergencia);


                    if (serialPort1.IsOpen)
                    {

                        byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value));
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(100);                                                       //Delay 100ms

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                serialPort1.Close();

                /// <summary>
                /// Function 03 (03hex) Read Holding Registers
                /// </summary>
                /// <param name="slaveAddress">Slave Address</param>
                /// <param name="startAddress">Starting Address</param>
                /// <param name="function">Function</param>
                /// <param name="numberOfPoints">Quantity of inputs</param>
                /// <returns>Byte Array</returns>
                byte[] WriteHoldingRegistersMsg(byte slaveAddress, ushort startAddress, byte function, byte[] values)
                {
                    byte[] frame = new byte[8];
                    frame[0] = slaveAddress;                // Endereço do escravo
                    frame[1] = function;                    // Código de função             
                    frame[2] = (byte)(startAddress >> 8);   // Starting Address High
                    frame[3] = (byte)startAddress;          // Starting Address Low            
                    Array.Copy(values, 0, frame, 4, values.Length);
                    byte[] crc = CalculateCRC(frame);       // Calculate CRC
                    frame[frame.Length - 2] = crc[0];       // Error Check Low
                    frame[frame.Length - 1] = crc[1];       // Error Check High
                    return frame;
                }


                /// <summary>
                /// Calculo do CRC
                /// </summary>
                /// <param name="frame"></param>
                /// <returns></returns>
                byte[] CalculateCRC(byte[] data) //data trocado para frame
                {
                    ushort CRCFull = 0xFFFF; // Set the 16-bit register (CRC register) = FFFFH.
                    byte CRCHigh = 0xFF;
                    byte CRCLow = 0xFF;
                    char CRCLSB;
                    byte[] CRC = new byte[2];
                    for (int i = 0; i < (data.Length) - 2; i++)
                    {
                        CRCFull = (ushort)(CRCFull ^ data[i]); // 

                        for (int j = 0; j < 8; j++)
                        {
                            CRCLSB = (char)(CRCFull & 0x0001);
                            CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

                            if (CRCLSB == 1)
                                CRCFull = (ushort)(CRCFull ^ 0xA001);
                        }
                    }
                    CRC[1] = CRCHigh = (byte)((CRCFull >> 8) & 0xFF);
                    CRC[0] = CRCLow = (byte)(CRCFull & 0xFF);
                    return CRC;
                }


                /// <summary>
                /// Convert UInt16 to byte array
                /// </summary>
                /// <param name="value">UInt16</param>
                /// <returns>byte[]</returns>
                byte[] ToByteArray(UInt16 value)
                {
                    byte[] array = BitConverter.GetBytes(value);
                    Array.Reverse(array);
                    return array;
                }

                /// <summary>
                /// Display Data
                /// </summary>
                /// <param name="data">Data</param>
                /// <returns>Message</returns>
                string Display(byte[] data)
                {
                    string result = string.Empty;
                    foreach (byte item in data)
                    {
                        result += string.Format("{0:X2}", item);
                    }
                    return result;
                }

            }
            else
            {
                //nada acontece
            }

        }


        #endregion Botão de Emergência 


        #region Data e Hora
        void Data_Load(object sender, EventArgs e)
        {
            Data.Text = DateTime.Now.ToString("dd/MM/yyyy") + " " + "às" + " " + DateTime.Now.ToShortTimeString(); ;
        }

        #endregion Data e Hora


        #region Botão de Controle Ativo

        private void ControleAtivado_Click(object sender, RoutedEventArgs e)
        {
            if (PortaSerialCom.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione a Porta COM!", "Atenção!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {

                MessageBox.Show("Neste modo é possível receber as informações dos sensores de nível e realizar o controle da bomba.", "Modo Controle Acionado!", MessageBoxButton.OK, MessageBoxImage.Information);


                int SelecaoP0222 = 7; //referencia de velocidade
                int SelecaoP1001 = 1; // executa aplicação SoftPLC
                int SelecaoP1003 = 1; //Seleção Aplicação SoftPLC
                int SelecaoP1016 = 2; //Seleção do Setpoint de Controle do PID
                int SelecaoP1024 = 0; // Tipo de Ação do Controlador PID
                int SelecaoP0236 = 6; //Função do Sinal AI2 - função 2 da aplicação 
                int SelecaoP0238 = 1; //Sinal da Entrada AI2
                int SelecaoP0263 = 20; //Função da Entrada DI1 - função 1 da aplicação
                int SelecaoP0105 = 0; //Seleção da rampa (1ª rampa)
                int SelecaoP0227 = 2; // Seleção Gira/Para
                int SelecaoP0133 = 650; //Velocidade mínima
                int SelecaoP0134 = 1350; //Velocidade mázima

                try
                {
                    serialPort1 = new SerialPort(PortaSerialCom.Text, 57600, Parity.Even, 8, StopBits.One)
                    {
                        DtrEnable = true,
                        RtsEnable = true
                    };
                    serialPort1.Open();

                    #region Parametrizações de PID

                    #region Seleção P1001

                    try
                    {
                        byte slaveAddress = 246;
                        byte function = 6;
                        ushort startAddress = 1001;
                        ushort value = Convert.ToUInt16(SelecaoP1001);


                        if (serialPort1.IsOpen)
                        {
                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value));
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100);

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    #endregion Seleção P1001 

                    #region Seleção P1003

                    try
                    {
                        byte slaveAddress = 246;
                        byte function = 6;
                        ushort startAddress = 1003;
                        ushort value = Convert.ToUInt16(SelecaoP1003);


                        if (serialPort1.IsOpen)
                        {
                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value));
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100);

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    #endregion Seleção P1003 

                    #region Seleção P1016

                    try
                    {
                        byte slaveAddress = 246;
                        byte function = 6;
                        ushort startAddress = 1016;
                        ushort value = Convert.ToUInt16(SelecaoP1016);


                        if (serialPort1.IsOpen)
                        {
                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value));
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100);

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    #endregion Seleção P1016 

                    #region Seleção P1024

                    try
                    {
                        byte slaveAddress = 246;
                        byte function = 6;
                        ushort startAddress = 1024;
                        ushort value = Convert.ToUInt16(SelecaoP1024);


                        if (serialPort1.IsOpen)
                        {
                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value));
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100);

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    #endregion Seleção P1024 

                    #region Seleção P0236

                    try
                    {
                        byte slaveAddress = 246;
                        byte function = 6;
                        ushort startAddress = 236;
                        ushort value = Convert.ToUInt16(SelecaoP0236);


                        if (serialPort1.IsOpen)
                        {
                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value));
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100);

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    #endregion Seleção P0236 

                    #region Seleção P0238

                    try
                    {
                        byte slaveAddress = 246;
                        byte function = 6;
                        ushort startAddress = 238;
                        ushort value = Convert.ToUInt16(SelecaoP0238);


                        if (serialPort1.IsOpen)
                        {
                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value));
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100);

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    #endregion Seleção P0238 

                    #region Seleção P0263

                    try
                    {
                        byte slaveAddress = 246;
                        byte function = 6;
                        ushort startAddress = 263;
                        ushort value = Convert.ToUInt16(SelecaoP0263);


                        if (serialPort1.IsOpen)
                        {
                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value));
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100);

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    #endregion Seleção P0263 

                    #region Seleção P0105

                    try
                    {
                        byte slaveAddress = 246;
                        byte function = 6;
                        ushort startAddress = 105;
                        ushort value = Convert.ToUInt16(SelecaoP0105);


                        if (serialPort1.IsOpen)
                        {
                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value));
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100);

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    #endregion Seleção P0105 

                    #region Seleção P0227

                    try
                    {
                        byte slaveAddress = 246;
                        byte function = 6;
                        ushort startAddress = 227;
                        ushort value = Convert.ToUInt16(SelecaoP0227);


                        if (serialPort1.IsOpen)
                        {
                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value));
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100);

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    #endregion Seleção P0227

                    #region Seleção P0133

                    try
                    {
                        byte slaveAddress = 246;
                        byte function = 6;
                        ushort startAddress = 133;


                        ushort valor = Convert.ToUInt16(SelecaoP0133);


                        if (serialPort1.IsOpen)
                        {

                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(valor));
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100);

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    #endregion Seleção P0133

                    #region Seleção P0134

                    try
                    {
                            byte slaveAddress = 246;
                            byte function = 6;
                            ushort startAddress = 134;

                            ushort valor = Convert.ToUInt16(SelecaoP0134);


                            if (serialPort1.IsOpen)
                            {

                                byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(valor));
                                serialPort1.Write(frame, 0, frame.Length);
                                Thread.Sleep(100);

                                if (serialPort1.BytesToRead >= 5)
                                {
                                    byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                    serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                    serialPort1.DiscardInBuffer();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    #endregion Seleção P0134



                    #endregion Parametrizações de PID

                    #region Seleção P0222 = 7 - SoftPLC

                    try
                    {
                        byte slaveAddress = 246;            //endereço do escravo
                        byte function = 6;                  //código de função - 6 = escrita de registradores
                        ushort startAddress = 222;          //endereço inicial de letiura
                        ushort value = Convert.ToUInt16(SelecaoP0222);  //recebe o valor escrito na variável SelecaoP0222

                        if (serialPort1.IsOpen)
                        {
                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value)); //Função de escrita 
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100); //delay

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                            Condicao2.Fill = System.Windows.Media.Brushes.Red;
                            Condicao3.Fill = System.Windows.Media.Brushes.White;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    #endregion Seleção P0222 = 7 - SoftPLC              


                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                serialPort1.Close();

                #region Escolha do Vertedor

                //decimal nivel2 = 0.00M;

                var vertedor = new Vertedor(VazaoparaVertedor.Text); //Envia para a página Vertedor a vazão lida nesta página
                vertedor.ShowDialog();
                LeituraNivelSensor1.Text = vertedor.Teste1.ToString(); //Sensor de nível recebe o valor de nível construido na página Vertedor

                if (LeituraNivelSensor1.Text == "System.Windows.Controls.TextBox: Sem Leitura!" || LeituraNivelSensor1.Text == "System.Windows.Controls.TextBox")
                {
                    LeituraNivelSensor1.Text = "Sem Leitura!";
                    LeituraNivelSensor2.Text = "Sem Leitura!";

                }

                else
                {
                    var teste = LeituraNivelSensor1.Text.Substring(32, 5);

                    //double teste2 = Convert.ToDouble(teste);
                    LeituraNivelSensor1.Text = teste + " " + "centímetros";


                    //nivel2 = Convert.ToDecimal(teste) / 100;
                    //LeituraNivelSensor2.Text = nivel2.ToString() + " " + "metros";
                }


                #endregion Escolha do Vertedor
            }
        }

        #endregion Código de Controle Ativo


        #region Botão de Controle Desativado
        private void ControleDesativado_Click(object sender, RoutedEventArgs e)
        {

            if (PortaSerialCom.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione a Porta COM!", "Atenção!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {

                MessageBox.Show("Neste modo NÃO é possível receber as informações dos sensores de nível e realizar o controle da bomba.", "Modo Controle Desacionado!", MessageBoxButton.OK, MessageBoxImage.Information);
                int SelecaoP0222 = 5;
                int SelecaoP0105 = 0; //Seleção da rampa (1ª rampa)
                int SelecaoP0227 = 2; //Seleção Gira/Para
                int SelecaoP0133 = 90; //Velocidade Mínima
                int SelecaoP0134 = 1350; //Velocidade Máxima


                #region Parte do Código Modbus 

                try
                {
                    serialPort1 = new SerialPort(PortaSerialCom.Text, 57600, Parity.Even, 8, StopBits.One)
                    {
                        DtrEnable = true,
                        RtsEnable = true
                    };
                    serialPort1.Open();


                    #region Seleção P0222 = 5 - Serial

                    try
                    {
                        byte slaveAddress = 246;                                                     //endereço do escravo  
                        byte function = 6;                                                           //código do de função Modbus
                        ushort startAddress = 222;                                                   //registrador
                        ushort value = Convert.ToUInt16(SelecaoP0222);                        //ATENÇÃO PARA AQUIIIII!


                        if (serialPort1.IsOpen)
                        {
                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value));
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100);

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                            Condicao3.Fill = System.Windows.Media.Brushes.Red;
                            Condicao2.Fill = System.Windows.Media.Brushes.White;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }


                    #endregion Seleção P0222 = 5 - Serial

                    #region Seleção P0105

                    try
                    {
                        byte slaveAddress = 246;
                        byte function = 6;
                        ushort startAddress = 105;
                        ushort value = Convert.ToUInt16(SelecaoP0105);


                        if (serialPort1.IsOpen)
                        {
                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value));
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100);

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    #endregion Seleção P0105 

                    #region Seleção P0227

                    try
                    {
                        byte slaveAddress = 246;
                        byte function = 6;
                        ushort startAddress = 227;
                        ushort value = Convert.ToUInt16(SelecaoP0227);


                        if (serialPort1.IsOpen)
                        {
                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value));
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100);

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    #endregion Seleção P0227

                    #region Seleção P0133

                    try
                    {
                        byte slaveAddress = 246;
                        byte function = 6;
                        ushort startAddress = 133;


                        ushort valor = Convert.ToUInt16(SelecaoP0133);


                        if (serialPort1.IsOpen)
                        {

                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(valor));
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100);

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    #endregion Seleção P0133

                    #region Seleção P0134

                    try
                    {
                        byte slaveAddress = 246;
                        byte function = 6;
                        ushort startAddress = 134;

                        ushort valor = Convert.ToUInt16(SelecaoP0134);


                        if (serialPort1.IsOpen)
                        {

                            byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(valor));
                            serialPort1.Write(frame, 0, frame.Length);
                            Thread.Sleep(100);

                            if (serialPort1.BytesToRead >= 5)
                            {
                                byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                                serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                                serialPort1.DiscardInBuffer();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    #endregion Seleção P0134


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                serialPort1.Close();

                #endregion Parte do Código Modbus
            }
        }

        #endregion Botão de Controle Desativado


        #region Porta Com

        public void PortaSerialCom_DropDownOpened(object sender, EventArgs e)
        {


            string[] ports = SerialPort.GetPortNames();
            PortaSerialCom.Items.Clear();
            foreach (string comport in ports)
            {
                PortaSerialCom.Items.Add(comport);
            }

            Condicao1.Fill = System.Windows.Media.Brushes.Red;
        }


        public void LeiturasIniciais()
        {
            decimal conversao1 = 0.00M;
            decimal conversao2 = 0.001M;
            var LeituraVelocidadeDB = " ";
            decimal nivel2 = 0.00M;


            Conexao con = new Conexao();

            try
            {
                con.conectar();

                string sqlite4 = "SELECT Vertedor FROM AtualizacaoVariaveis WHERE Vertedor IS NOT NULL";
                var cmd4 = new SQLiteCommand(sqlite4, con.conn2);
                SQLiteDataReader rdr4 = cmd4.ExecuteReader();


                while (rdr4.Read())
                {

                    UltimoVertedor.Text = rdr4["Vertedor"].ToString();
                    Condicao4.Fill = System.Windows.Media.Brushes.Red;

                }

                string sqlite5 = "SELECT Hb FROM AtualizacaoVariaveis WHERE Hb IS NOT NULL";
                var cmd5 = new SQLiteCommand(sqlite5, con.conn2);
                SQLiteDataReader rdr5 = cmd5.ExecuteReader();

                while (rdr5.Read())
                {
                    LeituraHb.Text = rdr5["Hb"].ToString();
                }

                string sqlite6 = "SELECT L FROM AtualizacaoVariaveis WHERE L IS NOT NULL";
                var cmd6 = new SQLiteCommand(sqlite6, con.conn2);
                SQLiteDataReader rdr6 = cmd6.ExecuteReader();

                while (rdr6.Read())
                {
                    LeituraL.Text = rdr6["L"].ToString();
                }


                #region Leituras Iniciais
                try
                {
                    serialPort1 = new SerialPort(PortaSerialCom.Text, 57600, Parity.Even, 8, StopBits.One)
                    {
                        DtrEnable = true,
                        RtsEnable = true
                    };
                    serialPort1.Open();

                    byte slaveAddress = 246;            // endereço do escravo
                    byte function = 3;                  //código de função - 3 = leitura de registradores
                    uint numberOfPoints = 1;            //número de registradores a serem lido
                    ushort startAddress = 0;            //endereço inicial 
                    byte[] frame;

                    if (serialPort1.IsOpen)
                    {
                        #region Velocidade

                        startAddress = 2;              //REGISTRADOR

                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints); // Função de leitura
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(150);              //delay

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];
                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];
                            String result = string.Format("{0}", valorConvertido);

                            LeituraVelocidadeDB = result;
                            LeituraVelocidade.Text = result + " " + "rpm"; //escrita do valor obtido em uma textbox

                            GraficoVelocidade.Value = Convert.ToDouble(result); // Gráfico Velocidade

                            if (Convert.ToInt16(result) > 1)
                            {
                                EstadoMotorON.Background = System.Windows.Media.Brushes.Green;
                                EstadoMotorOFF.Background = System.Windows.Media.Brushes.Lavender;
                            }
                            else
                            {
                                EstadoMotorOFF.Background = System.Windows.Media.Brushes.Red;
                                EstadoMotorON.Background = System.Windows.Media.Brushes.Lavender;

                            }

                            if (Convert.ToInt16(result) > 1700)
                            {
                                Alarme.Background = System.Windows.Media.Brushes.Red;
                                MessageBox.Show("ATENÇÃO com a velocidade do motor!", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                            else
                            {
                                Alarme.Background = System.Windows.Media.Brushes.MidnightBlue;
                            }

                        }
                        #endregion Velocidade

                        #region Vazão

                        startAddress = 1012;         //REGISTRADOR de leitura no formato wxy.z

                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(150);

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();

                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];
                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];
                            String result = string.Format("{0}", valorConvertido/10); // divisão por 10 por não apresentar virgula

                            VazaoparaVertedor.Text = result;

                            LeituraVazao.Text = result + " " + "litros/segundo";


                            conversao1 = Convert.ToDecimal(result) * conversao2;
                            LeituraVazao2.Text = conversao1.ToString() + " " + "m³/segundo";
                            var StringVazao = result;

                            if (LeituraVazao.Text != "0 litros/segundo")
                            {
                                SemVazao.Visibility = Visibility.Hidden;
                                ImagemSemVazao.Visibility = Visibility.Hidden;
                                LeituraVazao.Visibility = Visibility.Visible;
                                LeituraVazao2.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                SemVazao.Visibility = Visibility.Visible;
                                ImagemSemVazao.Visibility = Visibility.Visible;
                                LeituraVazao.Visibility = Visibility.Visible;
                                LeituraVazao2.Visibility = Visibility.Visible;
                            }

                            #region Leitura Nivel
                            if (Convert.ToInt16(VazaoparaVertedor.Text) > 0)
                            {
                                double VazaoLida = Double.Parse(VazaoparaVertedor.Text)/1000; //transformando a vazão de l/s para m³/s

                                if (UltimoVertedor.Text == "Triangular 90°")
                                {
                                    var H1 = Math.Pow(VazaoLida / 1.420, 0.4);

                                    var teste3 = Convert.ToDouble((H1 * 100));

                                    LeituraNivelSensor1.Text = Math.Round(teste3, 2) + " " + "centímetros"; //LeituraNivelSensor1.Text = (H1 * 100).ToString() + " " + "centímetros";

                                    LeituraNivelSensor2.Text = Convert.ToString(H1) + " " + "metros";
                                }

                                if (UltimoVertedor.Text == "Triangular 120°")
                                {
                                    var H2 = Math.Pow(VazaoLida / 2.302, 0.408329930584);

                                    var teste3 = Convert.ToDouble((H2 * 100));
                                    LeituraNivelSensor1.Text = Math.Round(teste3, 2) + " " + "centímetros";

                                    LeituraNivelSensor2.Text = Convert.ToString(H2) + " " + "metros";
                                }

                                if (UltimoVertedor.Text == "Triangular 135°")
                                {
                                    var H3 = Math.Pow(VazaoLida / 3.187, 0.404858299595);

                                    var teste3 = Convert.ToDouble((H3 * 100));
                                    LeituraNivelSensor1.Text = Math.Round(teste3, 2) + " " + "centímetros";

                                    LeituraNivelSensor2.Text = Convert.ToString(H3) + " " + "metros";

                                }

                                if (UltimoVertedor.Text == "Triangular 90° truncado")
                                {
                                    double RecebeHb1 = double.Parse(LeituraHb.Text);
                                    var H4 = 1.32 * (Math.Pow(VazaoLida, 2.47) - Math.Pow((VazaoLida - RecebeHb1), 2.47));
                                    var teste3 = Convert.ToDouble((H4 * 100));
                                    LeituraNivelSensor1.Text = Math.Round(teste3, 2) + " " + "centímetros";

                                    LeituraNivelSensor2.Text = Convert.ToString(H4) + " " + "metros";

                                }

                                if (UltimoVertedor.Text == "Triangular 120° truncado")
                                {
                                    double RecebeHb2 = double.Parse(LeituraHb.Text);
                                    var H5 = 2.302 * (Math.Pow(VazaoLida, 2.449) - Math.Pow((VazaoLida - RecebeHb2), 2.449));
                                    var teste3 = Convert.ToDouble((H5 * 100));
                                    LeituraNivelSensor1.Text = Math.Round(teste3, 2) + " " + "centímetros";

                                    LeituraNivelSensor2.Text = Convert.ToString(H5) + " " + "metros";
                                }

                                if (UltimoVertedor.Text == "Triangular trapezoidal (Tipo Cipoletti)")
                                {
                                    double RecebeL1 = double.Parse(LeituraL.Text);

                                    var H6 = Math.Pow(VazaoLida / (1.86 * RecebeL1), 0.6666667);
                                    var teste3 = Convert.ToDouble((H6 * 100));

                                    LeituraNivelSensor1.Text = Math.Round(teste3, 2) + " " + "centímetros";

                                    LeituraNivelSensor2.Text = Convert.ToString(H6) + " " + "metros";
                                }


                                if (UltimoVertedor.Text == null)
                                {
                                    var H7 = Math.Pow(VazaoLida / 1.420, 0.4);
                                    var teste3 = Convert.ToDouble((H7 * 100));

                                    LeituraNivelSensor1.Text = Math.Round(teste3, 2) + " " + "centímetros";

                                    LeituraNivelSensor2.Text = Convert.ToString(H7) + " " + "metros";
                                }
                            }
                        else
                        {
                            LeituraNivelSensor1.Text = "Sem leitura!";
                            LeituraNivelSensor2.Text = "Sem leitura!";
                        }
                            #endregion Leitura Nivel
                        }

                        #endregion Vazão

                    }

                    String sqlite2 = "INSERT INTO AtualizacaoVariaveis(Velocidade, Vazao) " + "VALUES ('" + LeituraVelocidadeDB + "', '" + VazaoparaVertedor.Text + "')";
                    SQLiteCommand comando2 = new SQLiteCommand(sqlite2, con.conn2);
                    _ = comando2.ExecuteNonQuery();
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                #endregion Leituras Iniciais 

                serialPort1.Close();

            }
            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro23", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            con.desconectar();


            #region Gráfico Velocidade

            try
            {
                con.conectar();

                var graficovelocidade = "SELECT Velocidade FROM AtualizacaoVariaveis Where Velocidade > 0";
                graficovelocidade = Convert.ToString(GraficoVelocidade.Value);

            }
            catch
            {
                MessageBox.Show("Erro no gráfico de velocidade", "Atenção");
            }

            con.desconectar();
            #endregion Gráfico Velocidade

        }

        private async void ConfirmaCom_Click(object sender, EventArgs e)
        {
            if (PortaSerialCom.SelectedItem == null)
            {
                MessageBox.Show("Selecione a porta de comunicação COM ou verifique a conexão do conversor RS485 para USB.", "Atenção!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            else
            {
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        this.Dispatcher.Invoke(() => { LeiturasIniciais(); }); //LeiturasInicias é um função
                        await Task.Delay(100); //Tempo de atualização de leitura da função
                    }
                });
            }
        }
  
        #endregion Porta Com


        #region Chamada Vertedor 2
    private void ChamadaVertedor2_Click(object sender, RoutedEventArgs e)
    {

            decimal var1 = 0.00M;

        if (PortaSerialCom.SelectedIndex == -1)
        {
            MessageBox.Show("Selecione a Porta COM!", "Atenção!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {

            var vertedor = new Vertedor(VazaoparaVertedor.Text); //Envia para a página Vertedor a vazão lida nesta página
            vertedor.ShowDialog();
            LeituraNivelSensor1.Text = vertedor.Teste1.ToString(); //Sensor de nível recebe o valor de nível construido na página Vertedor


                if (LeituraNivelSensor1.Text == "System.Windows.Controls.TextBox: Sem Leitura!" || LeituraNivelSensor1.Text == "System.Windows.Controls.TextBox")
                {
                    LeituraNivelSensor1.Text = "Sem Leitura!";
                    LeituraNivelSensor2.Text = "Sem Leitura!";
                }

                else
                {
                    var teste = LeituraNivelSensor1.Text.Substring(32, 5);
                    double teste2 = Convert.ToDouble(teste);
                    LeituraNivelSensor1.Text = Math.Round(teste2) + " " + "centímetros";
                    //var1 = Convert.ToDecimal(teste) / 100;
                    //LeituraNivelSensor2.Text = var1.ToString() + " " + "metros";

                }
            }
        }

        #endregion Chamada Vertedor 2


        #region Relatório
        private void Relatorio_Click(object sender, RoutedEventArgs e)
        {
            Conexao con = new Conexao();


                Document doc = new Document(PageSize.A4);           //tipo da página
                doc.SetMargins(40, 40, 40, 80);                     //margens da página
                string caminho = @"C:\Relatorio\" + "Relatório.pdf";

                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(caminho, FileMode.Create));

                doc.Open();

            try
            {
                con.conectar();

                string sqlite = "SELECT Velocidade, Frequencia, Tensao, Corrente, Potencia, Torque, Aceleracao, Desaceleracao, Sobrecarga, Contador, Vertedor, Vazao, CargaHidraulica, Setpoint, SaidaPID, GanhoKp, GanhoKi, GanhoKd, FiltroSetpoint FROM AtualizacaoVariaveis";
                var cmd = new SQLiteCommand(sqlite, con.conn2);
                SQLiteDataReader rdr = cmd.ExecuteReader();

                string sqlite2 = "SELECT Nome FROM Login";
                var cmd2 = new SQLiteCommand(sqlite2, con.conn);
                SQLiteDataReader rdr2 = cmd2.ExecuteReader();

                string sqlite3 = "SELECT Alarme, Falha, AlertaConexao, AlertaEstadoInversor FROM AlarmeseFalhas";
                var cmd3 = new SQLiteCommand(sqlite3, con.conn3);
                SQLiteDataReader rdr3 = cmd3.ExecuteReader();

                rdr.Read();
                rdr2.Read();
                rdr3.Read();

                Paragraph titulo = new Paragraph();
                titulo.Font = new Font(Font.FontFamily.COURIER, 16); //fonte e tamanho do título
                titulo.Alignment = Element.ALIGN_CENTER;            // alinhamento do título no centro 
                titulo.Add("Sistema de Supervisão e Controle - Relatório\n\n"); //título
                doc.Add(titulo);                                    //adiciona o parágrafo no documento


                Paragraph Usuario = new Paragraph("", new Font(Font.NORMAL, 12));
                string login = "Usuário:" + " " + rdr2["Nome"].ToString() + "\n";
                Usuario.Add(login);
                doc.Add(Usuario);


                Paragraph HoraDataAcesso = new Paragraph("", new Font(Font.NORMAL, 12));
                string HoraData = "Hora e Data do seu último acesso:" + " " + Data.Text + "\n\n";
                HoraDataAcesso.Add(HoraData);
                doc.Add(HoraDataAcesso);

                Paragraph TextoTabela1 = new Paragraph("", new Font(Font.NORMAL, 12));
                string textotabela1 = "Tabela 1: Contém as leituras das variáveis relacionadas ao motor.\n\n";
                TextoTabela1.Add(textotabela1);
                doc.Add(TextoTabela1);


                PdfPTable tabela1 = new PdfPTable(2); //número de colunas 

                tabela1.AddCell(" ");
                tabela1.AddCell("Último Valor Lido");

                tabela1.AddCell("Velocidade do Motor");
                tabela1.AddCell(rdr["Velocidade"].ToString() + " " + "rpm");

                tabela1.AddCell("Frequência do Motor");
                tabela1.AddCell(rdr["Frequencia"].ToString() + " " + "Hz");

                tabela1.AddCell("Corrente do Motor");
                tabela1.AddCell(rdr["Corrente"].ToString() + " " + "A");

                tabela1.AddCell("Potência do Motor");
                tabela1.AddCell(rdr["Potencia"].ToString() + " " + "kW");

                tabela1.AddCell("Tensão de Saída");
                tabela1.AddCell(rdr["Tensao"].ToString() + " " + "V");

                tabela1.AddCell("Torque do Motor");
                tabela1.AddCell(rdr["Torque"].ToString() + " " + "%");

                tabela1.AddCell("Tempo de Aceleração");
                tabela1.AddCell(rdr["Aceleracao"].ToString() + " " + "segundos");

                tabela1.AddCell("Tempo de Desaceleração");
                tabela1.AddCell(rdr["Desaceleracao"].ToString() + " " + "segundos");

                tabela1.AddCell("Sobrecarga do Motor");
                tabela1.AddCell(rdr["Sobrecarga"].ToString() + " " + "%");

                tabela1.AddCell("Contador");
                tabela1.AddCell(rdr["Contador"].ToString() + " " + "kWh");

                doc.Add(tabela1);

                Paragraph TextoTabela2 = new Paragraph("", new Font(Font.NORMAL, 12));
                string textotabela2 = "Tabela 2: Contém as leituras das variáveis relacionadas ao sistema de controle.\n\n";
                TextoTabela2.Add(textotabela2);
                doc.Add(TextoTabela2);

                PdfPTable tabela2 = new PdfPTable(2); //número de colunas 

                tabela2.AddCell(" ");
                tabela2.AddCell("Último Valor Lido");

                tabela2.AddCell("Vazão - Leitura 2");
                tabela2.AddCell(rdr["Vazao"].ToString() + " " + "l/s");

                tabela2.AddCell("Vazão - Leitura 1");
                tabela2.AddCell(LeituraVazao2.Text + " " + "m³/s");

                tabela2.AddCell("Vertedor");
                tabela2.AddCell(rdr["Vertedor"].ToString());

                tabela2.AddCell("Carga Hidraulica");
                tabela2.AddCell(rdr["CargaHidraulica"].ToString() + " " + "cm");

                tabela2.AddCell("Carga Hidraulica");
                tabela2.AddCell(rdr["CargaHidraulica"].ToString() + " " + "m");

                tabela2.AddCell("Setpoint");
                tabela2.AddCell(rdr["Setpoint"].ToString());

                tabela2.AddCell("Ganho Proporcional");
                tabela2.AddCell(rdr["GanhoKp"].ToString());

                tabela2.AddCell("Ganho Integral");
                tabela2.AddCell(rdr["GanhoKi"].ToString());

                tabela2.AddCell("Ganho Derivativo");
                tabela2.AddCell(rdr["GanhoKd"].ToString());

                tabela2.AddCell("Saída PID");
                tabela2.AddCell(rdr["SaidaPID"].ToString());

                tabela2.AddCell("Filtro Setpoint");
                tabela2.AddCell(rdr["FiltroSetpoint"].ToString());

                doc.Add(tabela2);

                Paragraph TextoTabela3 = new Paragraph("", new Font(Font.NORMAL, 12));
                string textotabela3 = "Tabela 3: Contém as leituras dos alarmes e das falhas.\n\n";
                TextoTabela3.Add(textotabela3);
                doc.Add(TextoTabela3);

                PdfPTable tabela3 = new PdfPTable(2); //número de colunas 

                tabela3.AddCell(" ");
                tabela3.AddCell("Último Valor Lido");

                tabela3.AddCell("Alarme");
                tabela3.AddCell(rdr3["Alarme"].ToString());

                tabela3.AddCell("Falha");
                tabela3.AddCell(rdr3["Falha"].ToString());

                tabela3.AddCell("Estado da Conexão");
                tabela3.AddCell(rdr3["AlertaConexao"].ToString());

                tabela3.AddCell("Estado do Inversor");
                tabela3.AddCell(rdr3["AlertaEstadoInversor"].ToString());

                doc.Add(tabela3);

                con.desconectar();
            }
            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro10", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            doc.Close();

        }



        #endregion Relatório

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Teste teste = new Teste();
            teste.Show();
        }
    }
}










