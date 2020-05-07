using System.Windows;
using System.Windows.Controls;
using System.IO.Ports;
using System.Threading;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ProjetoCanaldeHidraulica.CadastrodoUsuario;
using System.Data.SQLite;

namespace ProjetoCanaldeHidraulica
{
    /// <summary>
    /// Lógica interna para Alarme.xaml
    /// </summary>
    public partial class Alarme : Window
    {

        SerialPort serialPort1 = new SerialPort(); //Chamada da porta serial 

        public Alarme(string RecebePortaCom)
        {
            InitializeComponent();
            PortaSerialCom4.Text = RecebePortaCom;

            Conexao con = new Conexao();

            #region Alarmes Gerais

                try
                {
                    serialPort1 = new SerialPort(PortaSerialCom4.Text, 57600, Parity.Even, 8, StopBits.One)
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
                        #region Alarmes
                        startAddress = 48;         //REGISTRADOR ALARME

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
                            String result = string.Format("{0}", valorConvertido);

                            string recebealarme = result.ToString();
                            LeituraAlarmes.Items.Add(result);

                        }

                        #endregion Alarmes

                        #region Falhas
                        startAddress = 49;         //REGISTRADOR ALARME

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
                            String result = string.Format("{0}", valorConvertido);

                            LeituraFalhas.Items.Add(result);
                               
                        }

                        #endregion Falhas

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                serialPort1.Close();
   
            #endregion Alarmes Gerais

            #region Alarme Específico 

            try
            {
                con.conectar();

                try
                {
                    serialPort1 = new SerialPort(PortaSerialCom4.Text, 57600, Parity.Even, 8, StopBits.One)
                    {
                        DtrEnable = true,
                        RtsEnable = true
                    };
                    serialPort1.Open();

                    byte slaveAddress = 246;                                                    //endereço do escravo  
                    byte function = 3;                                                          //código do de função Modbus
                    uint numberOfPoints = 1;                                                    //número de registradores 
                    ushort startAddress = 0;
                    byte[] frame;


                    if (serialPort1.IsOpen)
                    {
                        #region  Estado Interface Serial 

                        startAddress = 316;         //REGISTRADOR

                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(150);

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

                            foreach (var item in valores)
                            {
                                if (item == 0)
                                {
                                    result += string.Format("Estado da interface serial:" + " " + item + " - Inativo: ocorre quando o equipamento não possui cartão de interface RS485 instalado.");
                                    AlertaConexão.Background = System.Windows.Media.Brushes.Red;
                                    AlertaConexão.Foreground = System.Windows.Media.Brushes.White;

                                }
                                if (item == 1)
                                {
                                    result += string.Format("Estado da interface serial:" + " " + (item) + " - Ativo: cartão de interface RS485 instalado e reconhecido.");
                                    AlertaConexão.Background = System.Windows.Media.Brushes.White;
                                    AlertaConexão.Foreground = System.Windows.Media.Brushes.Black;

                                }
                                if (item == 2)
                                {
                                    result += string.Format("Estado da interface serial:" + " " + (item) + " - ERRO DE WATCHDOG: interface serial ativa, mas detectado erro de comunicação serial - alarme A128!");
                                    AlertaConexão.Background = System.Windows.Media.Brushes.Red;
                                    AlertaConexão.Foreground = System.Windows.Media.Brushes.White;

                                }
                            }

                            AlertaConexão.Text = result;


                            //String sqlite = "INSERT INTO AlarmeseFalhas(AlertaConexao) " + "VALUES ('" + AlertaConexão.Text + "')";
                            //SQLiteCommand comando = new SQLiteCommand(sqlite, con.conn3);
                            //_ = comando.ExecuteNonQuery();

                        }
                        #endregion  Estado Interface Serial 

                        #region Estado do Inversor 

                        startAddress = 6;

                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(100);                                                      // Delay 100ms

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();

                            byte[] data = new byte[bufferReceiver.Length - 5];
                            Array.Copy(bufferReceiver, 3, data, 0, data.Length);
                            UInt16[] temp = Word.ByteToUInt16(data);
                            string result = string.Empty;

                            foreach (var item in temp)
                            {
                                if (item == 0)
                                {
                                    result += string.Format("{0}", "Estado do inversor:" + " " + item + " - Pronto: inversor está pronto para ser habilitado.");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.White;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.Black;

                                }
                                if (item == 1)
                                {
                                    result += string.Format("{0}", "Estado do inversor:" + " " + (item) + " - Run: indica que o inversor está habilitado.");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.White;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.Black;
                                }
                                if (item == 2)
                                {
                                    result += string.Format("{0}", "Estado do inversor:" + " " + (item) + " - SUBTENSÃO: indica que o inversor está com tensão de rede insuficiente para operação!");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.Red;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.White;
                                }
                                if (item == 3)
                                {
                                    result += string.Format("{0}", "Estado do inversor:" + " " + (item) + " - FALHA: indica que o inversor está no estado de falha.");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.Red;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.White;
                                }
                                if (item == 4)
                                {
                                    result += string.Format("{0}", "Estado do inversor:" + " " + (item) + " - Autoajuste: indica que o inversor está executando a rotina de autoajuste.");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.White;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.Black;
                                }
                                if (item == 5)
                                {
                                    result += string.Format("{0}", "Estado do inversor:" + " " + (item) + " - Start-up Orientado: indica que o inversor está no modo de start-up orientado.");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.White;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.Black;
                                }
                                if (item == 6)
                                {
                                    result += string.Format("{0}", "Estado do inversor:" + " " + (item) + " - Frenagem CC: indica que o inversor está aplicando frenagem CC para parada no motor.");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.White;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.Black;
                                }
                                if (item == 7)
                                {
                                    result += string.Format("Estado do inversor:" + " " + (item) + " - Safe Torque Off: a função parada de segurança está ativa.");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.Red;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.White;
                                }
                            }

                            AlertaEstadoInversor.Text = result;
                        }

                        #endregion Estado do Inversor

                    }

                    String sqlite3 = "INSERT INTO AlarmeseFalhas(AlertaEstadoInversor, AlertaConexao) " + "VALUES ('" + AlertaEstadoInversor.Text + "', '" + AlertaConexão.Text + "')";
                    SQLiteCommand comando3 = new SQLiteCommand(sqlite3, con.conn3);
                    _ = comando3.ExecuteNonQuery();
                }


                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                serialPort1.Close();
            }

            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro1", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            con.desconectar();

            #endregion Alarme Específico 

            #region Gráfico 

            try
            {
                con.conectar();

                string sqlite = "SELECT Velocidade, Vazao FROM AtualizacaoVariaveis";
                var cmd = new SQLiteCommand(sqlite, con.conn2);
                SQLiteDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    VelocidadeBD.Text = rdr["Velocidade"].ToString();
                    VazaoDB.Text = rdr["Vazao"].ToString();

                }

            }

            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            con.desconectar();

            GraficoVelocidade.Value = Convert.ToDouble(VelocidadeBD.Text);
            GraficoVazao.Value = Convert.ToDouble(VazaoDB.Text);

            #endregion Gráfico 


        }



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

                    serialPort1 = new SerialPort(PortaSerialCom4.Text, 57600, Parity.Even, 8, StopBits.One)
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


        #region Funções

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
    
    #endregion Funções


        #region Voltar página principal
    private void Voltar_Click_1(object sender, RoutedEventArgs e)
        {
            Window1 principal = new Window1();
            principal.Show();
        }

        #endregion Voltar página principal 


        #region Alarme
        private void LeituraAlarmes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Conexao con = new Conexao();

            try
            {
                con.conectar();

                foreach (var item in LeituraAlarmes.Items)
                {

                    if (item.ToString() == "46")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Carga Alta no Motor!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }

                    if (item.ToString() == "47")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Carga Alta nos IGBTs!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }

                    if (item.ToString() == "50")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Temperatura dos IGBTs alta!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "90")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Alarme externo!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "98")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Ativar habilita geral!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "110")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Atenção! Temperatura do Motor Alta";
                        AvisosGerais.Background = System.Windows.Media.Brushes.Red;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.White;
                    }
                    if (item.ToString() == "128")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Timeout de Comunicação Serial!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "152")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Temperatura de Ar Interno Alta!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "159")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: HMI incompatível!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "163")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Fio Partido Al1!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "164")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Fio Partido Al2!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "168")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Erro de Velocidade muito Alto!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "170")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Parada de Segurança!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "177")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Substituição Ventilador!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "702")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Inversor Desabilitado";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "704")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Dois Movimentos Habilitados!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "706")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Referência não Programada para SoftPLC!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "708")
                    {
                        AvisosGerais.Clear();
                        AvisosGerais.Text = "ALARME: Aplicativo SoftPLC Parado!";
                        AvisosGerais.Background = System.Windows.Media.Brushes.White;
                        AvisosGerais.Foreground = System.Windows.Media.Brushes.Black;
                    }

                    String sqlite4 = "INSERT INTO AlarmeseFalhas(Alarme) " + "VALUES ('" + AvisosGerais.Text + "')";
                    SQLiteCommand comando4 = new SQLiteCommand(sqlite4, con.conn3);
                    _ = comando4.ExecuteNonQuery();
                }


            }

            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            con.desconectar();
        }
        #endregion Alarme

        #region Falhas
        private void AvisosGeraisFalha_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Conexao con = new Conexao();

            try
            {
                con.conectar();

                foreach (var item in LeituraFalhas.Items)
                {
                    if (item.ToString() == "6")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Desequilíbrio Falta de Fase na Rede!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }

                    if (item.ToString() == "21")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Subtensão Link DC!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }

                    if (item.ToString() == "22")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Sobretensão Link DC!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "48")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Sobrecarga nos IGBTs!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "51")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Sobretemperatura IGBTs!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "70")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Atenção! Sobrecorrente/Curto-Circuito";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.Red;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.White;
                    }
                    if (item.ToString() == "71")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Atenção! Sobrecorrente na saída!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.Red;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.White;
                    }
                    if (item.ToString() == "72")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Sobrecarga no Motor!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "74")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Falta à Terra!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "78")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Sobretemperatura no Motor!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "90")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Falha Externa!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "99")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Offset Corrente Inválido!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "150")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Sobrevelocidade Motor!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "156")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Subtemperatura!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "157")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Perda Dados Tabela Parâmetros!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "160")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Relés Parada de Segurança!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "169")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Erro de Velocidade Muito Alto";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "179")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Falha Velocidade Ventilador!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "183")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Sobrecarga IGBTs + Temperatura!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "228")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Timeout Comunicação Serial !";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "709")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Aplicativo SoftPLC Parado!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    if (item.ToString() == "711")
                    {
                        AvisosGeraisFalha.Clear();
                        AvisosGeraisFalha.Text = "FALHA: Falha de Execução da SoftPLC!";
                        AvisosGeraisFalha.Background = System.Windows.Media.Brushes.White;
                        AvisosGeraisFalha.Foreground = System.Windows.Media.Brushes.Black;
                    }
                    String sqlite5 = "INSERT INTO AlarmeseFalhas(Falha) " + "VALUES ('" + AvisosGeraisFalha.Text + "')";
                    SQLiteCommand comando5 = new SQLiteCommand(sqlite5, con.conn3);
                    _ = comando5.ExecuteNonQuery();
                }


            }

            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro2", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            con.desconectar();
        }

        #endregion Falhas

        #region Alarme Assíncrono 

        public void FuncaoLeituras()
        {
            Conexao con = new Conexao();

            #region Alarmes Gerais
            try
            {
                con.conectar();

                try
                {
                    serialPort1 = new SerialPort(PortaSerialCom4.Text, 57600, Parity.Even, 8, StopBits.One)
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
                        #region Alarmes
                        startAddress = 48;         //REGISTRADOR ALARME

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
                            String result = string.Format("{0}", valorConvertido);

                            LeituraAlarmes.Items.Add(result);

                        }

                        #endregion Alarmes

                        #region Falhas
                        startAddress = 49;         //REGISTRADOR ALARME

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
                            String result = string.Format("{0}", valorConvertido);

                            LeituraFalhas.Items.Add(result);

                        }

                        #endregion Falhas

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                serialPort1.Close();

            }
            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro3", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            con.desconectar();

            #endregion Alarmes Gerais

            #region Alarme Específico 
            try
            {
                con.conectar();

                try
                {
                    serialPort1 = new SerialPort(PortaSerialCom4.Text, 57600, Parity.Even, 8, StopBits.One)
                    {
                        DtrEnable = true,
                        RtsEnable = true
                    };
                    serialPort1.Open();

                    byte slaveAddress = 246;                                                    //endereço do escravo  
                    byte function = 3;                                                          //código do de função Modbus
                    uint numberOfPoints = 1;                                                    //número de registradores 
                    ushort startAddress = 0;
                    byte[] frame;


                    if (serialPort1.IsOpen)
                    {
                        #region  Estado Interface Serial 

                        startAddress = 316;         //REGISTRADOR

                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(150);

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

                            foreach (var item in valores)
                            {
                                if (item == 0)
                                {
                                    result += string.Format("Estado da interface serial:" + " " + item + "- Inativo: ocorre quando o equipamento não possui cartão de interface RS485 instalado.");
                                    AlertaConexão.Background = System.Windows.Media.Brushes.Red;
                                    AlertaConexão.Foreground = System.Windows.Media.Brushes.White;

                                }
                                if (item == 1)
                                {
                                    result += string.Format("Estado da interface serial:" + " " + (item) + "- Ativo: cartão de interface RS485 instalado e reconhecido.");
                                    AlertaConexão.Background = System.Windows.Media.Brushes.White;
                                    AlertaConexão.Foreground = System.Windows.Media.Brushes.Black;

                                }
                                if (item == 2)
                                {
                                    result += string.Format("Estado da interface serial:" + " " + (item) + "- ERRO DE WATCHDOG: interface serial ativa, mas detectado erro de comunicação serial - alarme A128!");
                                    AlertaConexão.Background = System.Windows.Media.Brushes.Red;
                                    AlertaConexão.Foreground = System.Windows.Media.Brushes.White;

                                }
                            }

                            AlertaConexão.Text = result;

                        }
                        #endregion  Estado Interface Serial 

                        #region Estado do Inversor 

                        startAddress = 6;

                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(100);                                                      // Delay 100ms

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();

                            byte[] data = new byte[bufferReceiver.Length - 5];
                            Array.Copy(bufferReceiver, 3, data, 0, data.Length);
                            UInt16[] temp = Word.ByteToUInt16(data);
                            string result = string.Empty;

                            foreach (var item in temp)
                            {
                                if (item == 0)
                                {
                                    result += string.Format("{0}", "Estado do inversor:" + " " + item + "- Pronto: inversor está pronto para ser habilitado.");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.White;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.Black;

                                }
                                if (item == 1)
                                {
                                    result += string.Format("{0}", "Estado do inversor:" + " " + (item) + "- Run: indica que o inversor está habilitado.");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.White;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.Black;
                                }
                                if (item == 2)
                                {
                                    result += string.Format("{0}", "Estado do inversor:" + " " + (item) + "- SUBTENSÃO: indica que o inversor está com tensão de rede insuficiente para operação!");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.Red;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.White;
                                }
                                if (item == 3)
                                {
                                    result += string.Format("{0}", "Estado do inversor:" + " " + (item) + "- FALHA: indica que o inversor está no estado de falha.");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.Red;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.White;
                                }
                                if (item == 4)
                                {
                                    result += string.Format("{0}", "Estado do inversor:" + " " + (item) + "- Autoajuste: indica que o inversor está executando a rotina de autoajuste.");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.White;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.Black;
                                }
                                if (item == 5)
                                {
                                    result += string.Format("{0}", "Estado do inversor:" + " " + (item) + "- Start-up Orientado: indica que o inversor está no modo de start-up orientado.");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.White;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.Black;
                                }
                                if (item == 6)
                                {
                                    result += string.Format("{0}", "Estado do inversor:" + " " + (item) + "- Frenagem CC: indica que o inversor está aplicando frenagem CC para parada no motor.");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.White;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.Black;
                                }
                                if (item == 7)
                                {
                                    result += string.Format("Estado do inversor:" + " " + (item) + "- Safe Torque Off: a função parada de segurança está ativa.");
                                    AlertaEstadoInversor.Background = System.Windows.Media.Brushes.Red;
                                    AlertaEstadoInversor.Foreground = System.Windows.Media.Brushes.White;
                                }
                            }

                            AlertaEstadoInversor.Text = result;

                        }

                        #endregion Estado do Inversor

                    }


                    String sqlite2 = "INSERT INTO AlarmeseFalhas(AlertaConexao, AlertaEstadoInversor) " + "VALUES ('" + AlertaConexão.Text + "', '" + AlertaEstadoInversor.Text + "')";
                    SQLiteCommand comando2 = new SQLiteCommand(sqlite2, con.conn3);
                    _ = comando2.ExecuteNonQuery();
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                serialPort1.Close();
            }
            catch (Exception ex1)
            {
                MessageBox.Show(ex1.Message, "Erro4");
            }
            con.desconectar();



            #endregion Alarme Específico 

            #region Gráfico Assíncrono

            try
            {
                con.conectar();

                string sqlite = "SELECT Velocidade, Vazao FROM AtualizacaoVariaveis";
                var cmd = new SQLiteCommand(sqlite, con.conn2);
                SQLiteDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    VelocidadeBD.Text = rdr["Velocidade"].ToString();
                    VazaoDB.Text = rdr["Vazao"].ToString();
                }

            }

            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            con.desconectar();

            GraficoVelocidade.Value = Convert.ToDouble(VelocidadeBD.Text);
            GraficoVazao.Value = Convert.ToDouble(VazaoDB.Text);

            #endregion Gráfico Assíncrono

        }

        public async void Button_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    this.Dispatcher.Invoke(() => { FuncaoLeituras(); });

                    await Task.Delay(1000);
                }
            });
        }

        #endregion Alarme Assíncrono 





        private void LimparLista_Click(object sender, RoutedEventArgs e)
        {
            LeituraAlarmes.Items.Clear();
            LeituraFalhas.Items.Clear();
        }
    }

}
