using System.Windows;
using System.IO.Ports;
using System.Threading;
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Data.SQLite;
using ProjetoCanaldeHidraulica.CadastrodoUsuario;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using System.Timers;
using System.Windows.Controls;
using LiveCharts.Wpf.Charts.Base;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.io;
using iTextSharp.text.api;
using Font = iTextSharp.text.Font;
using Paragraph = iTextSharp.text.Paragraph;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

namespace ProjetoCanaldeHidraulica
{
    /// <summary>
    /// Lógica interna para Leitura_de_variaveis.xaml
    /// </summary>
    public partial class Leitura_de_variaveis : Window
    {

        Window1 TelaPrincipal = new Window1();
        SerialPort serialPort1 = new SerialPort();
        String StringVelocidade = ("-");
        String StringFrequencia = ("-");
        String StringCorrente = ("-");
        String StringPotencia = ("-");
        String StringTorque = ("-");
        String StringTensão = ("-");
        String StringAceleração = ("-");
        String StringDesaceleração = ("-");
        String StringSobrecarga = ("-");
        String StringContador = ("-");

        public Leitura_de_variaveis(string RecebePortaCom)
        {
            InitializeComponent();
            PortaSerialCom3.Text = RecebePortaCom;


            Conexao con = new Conexao();
            try
            {
                con.conectar();

                #region Leituras Iniciais

                try
                {
                    serialPort1 = new SerialPort(PortaSerialCom3.Text, 57600, Parity.Even, 8, StopBits.One)
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
                        #region Velocidade

                        startAddress = 2;         //REGISTRADOR

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

                            LerVelocidade.Text = result;
                            StringVelocidade = result;
                        }
                        #endregion Velocidade


                        #region Frequência    
                        startAddress = 5;
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

                            LerFrequencia.Text = result;
                            StringFrequencia = result;
                        }
                        #endregion Frequência


                        #region Corrente
                        startAddress = 3;                                                  //registrador
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(150);                                                      // Delay 100ms

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();
                            //LerCorrente.Text = Display(bufferReceiver);

                            //processamento dos dados
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];
                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];

                            String result = string.Format("{0}", valorConvertido);

                            LerCorrente.Text = result;
                            StringCorrente = result;
                        }
                        #endregion Corrente


                        #region Potência 
                        startAddress = 10;
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(150);

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();
                            //LerPotencia.Text = Display(bufferReceiver);

                            //processamento dos dados
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];
                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];

                            String result = string.Format("{0}", valorConvertido);

                            LerPotencia.Text = result;
                            StringPotencia = result;
                        }
                        #endregion Potência 


                        #region Torque
                        startAddress = 9;                                                  //registrador
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(150);                                                      // Delay 100ms

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();
                            //LerTorque.Text = Display(bufferReceiver);

                            //processamento dos dados
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];
                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];

                            String result = string.Format("{0}", valorConvertido);

                            LerTorque.Text = result;
                            StringTorque = result;
                        }

                        #endregion Torque


                        #region Tensão
                        startAddress = 7;                                                  //registrador
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(100);                                                      // Delay 100ms

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();
                            //LerTensão.Text = Display(bufferReceiver);

                            //processamento dos dados
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];

                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];

                            String result = string.Format("{0}", valorConvertido);

                            LerTensão.Text = result;
                            StringTensão = result;
                        }
                        #endregion Tensão 


                        #region Tempo de Aceleração
                        startAddress = 100;                                                  //registrador
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(150);                                                      // Delay 100ms


                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();

                            //processamento dos dados
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];
                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];

                            String result = string.Format("{0}", valorConvertido / 10);

                            LerAceleração.Text = result;
                            StringAceleração = result;
                        }
                        #endregion Tempo de Aceleração


                        #region Tempo de Desaceleração
                        startAddress = 101;                                                  //registrador
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(150);                                                      // Delay 100ms

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();

                            //processamento dos dados
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];
                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];

                            String result = string.Format("{0}", valorConvertido / 10);

                            LerDesaceleração.Text = result;
                            StringDesaceleração = result;
                        }

                        #endregion Tempo de desaceleração


                        #region Sobrecarga
                        startAddress = 37;                                                  //registrador
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        LerSobrecarga.Text = StringSobrecarga;                                    //Recebe o resultado da função Display 
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(150);                                                      // Delay 100ms

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();
                            //LerSobrecarga.Text = Display(bufferReceiver);

                            //processamento dos dados
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];
                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];

                            String result = string.Format("{0}", valorConvertido);

                            LerSobrecarga.Text = result;
                            StringSobrecarga = result;
                        }
                        #endregion Sobrecarga


                        #region Contador
                        startAddress = 44;                                                  //registrador
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(150);                                                      // Delay 100ms

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

                            LerContador.Text = result;
                            StringContador = result;
                        }
                        #endregion Contador

                        String sqlite = "INSERT INTO AtualizacaoVariaveis(Velocidade, Frequencia, Tensao, Corrente, Potencia, Torque, Aceleracao, Desaceleracao, Sobrecarga, Contador) " +
                            "VALUES ('" + LerVelocidade.Text + "', '" + LerFrequencia.Text + "', '" + LerTensão.Text + "', '" + LerCorrente.Text + "', '" + LerPotencia.Text + "', '" + LerTorque.Text + "', '" + LerAceleração.Text + "', '" + LerDesaceleração.Text + "', '" + LerSobrecarga.Text + "', '" + LerContador.Text + "')";
                        SQLiteCommand comando = new SQLiteCommand(sqlite, con.conn2);
                        _ = comando.ExecuteNonQuery();
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                serialPort1.Close();
            }

            #endregion Leituras Iniciais

            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            con.desconectar();
        }

        #region Gráficos 

        #region Gráfico Frequência

        private void AtualizarGrafico1_Click(object sender, RoutedEventArgs e)
        {

            Conexao con = new Conexao();
            try
            {
                con.conectar();

                string sqlite = "SELECT Frequencia FROM AtualizacaoVariaveis WHERE Frequencia IS NOT NULL";
                var cmd = new SQLiteCommand(sqlite, con.conn2);
                SQLiteDataAdapter dp = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable();
                dp.Fill(dt);

                var ContagemLinhas = dt.Rows.Count;

                FrequenciaGrafico1 = Convert.ToInt32(dt.Rows[ContagemLinhas - 1]["Frequencia"].ToString());
                FrequenciaGrafico2 = Convert.ToInt32(dt.Rows[ContagemLinhas - 2]["Frequencia"].ToString());
                FrequenciaGrafico3 = Convert.ToInt32(dt.Rows[ContagemLinhas - 3]["Frequencia"].ToString());
                FrequenciaGrafico4 = Convert.ToInt32(dt.Rows[ContagemLinhas - 4]["Frequencia"].ToString());
                FrequenciaGrafico5 = Convert.ToInt32(dt.Rows[ContagemLinhas - 5]["Frequencia"].ToString());
                FrequenciaGrafico6 = Convert.ToInt32(dt.Rows[ContagemLinhas - 6]["Frequencia"].ToString());


                SeriesCollection = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Frequência", FontSize = 3,
                        Values = new ChartValues<double> { FrequenciaGrafico1, FrequenciaGrafico2, FrequenciaGrafico3,
                        FrequenciaGrafico4, FrequenciaGrafico5, FrequenciaGrafico6}
                    },

                };

                Labels = new[] { "0", "150", "300", "450", "600", "750" };
                YFormatter = value => value.ToString();
                SeriesCollection[0].Values.Add(5d);

                DataContext = this;

            }

            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro Gráfico 1", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            con.desconectar();
        }
        #endregion Gráfico Frequência


        #region Gráfico Corrente

        private void AtualizarGrafico2_Click(object sender, RoutedEventArgs e)
        {

            Conexao con = new Conexao();

            try
            {
                con.conectar();

                string sqlite2 = "SELECT Corrente FROM AtualizacaoVariaveis WHERE Corrente IS NOT NULL";
                var cmd2 = new SQLiteCommand(sqlite2, con.conn2);
                SQLiteDataAdapter dp2 = new SQLiteDataAdapter(cmd2);
                DataTable dt2 = new DataTable();
                dp2.Fill(dt2);

                var ContagemLinhas = dt2.Rows.Count;

                CorrenteGrafico1 = Convert.ToInt32(dt2.Rows[ContagemLinhas - 1]["Corrente"].ToString());
                CorrenteGrafico2 = Convert.ToInt32(dt2.Rows[ContagemLinhas - 2]["Corrente"].ToString());
                CorrenteGrafico3 = Convert.ToInt32(dt2.Rows[ContagemLinhas - 3]["Corrente"].ToString());
                CorrenteGrafico4 = Convert.ToInt32(dt2.Rows[ContagemLinhas - 4]["Corrente"].ToString());
                CorrenteGrafico5 = Convert.ToInt32(dt2.Rows[ContagemLinhas - 5]["Corrente"].ToString());
                CorrenteGrafico6 = Convert.ToInt32(dt2.Rows[ContagemLinhas - 6]["Corrente"].ToString());


                SeriesCollection2 = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Corrente", FontSize = 3,
                        Values = new ChartValues<double> { CorrenteGrafico1, CorrenteGrafico2, CorrenteGrafico3,
                            CorrenteGrafico4, CorrenteGrafico5, CorrenteGrafico6},
                         PointGeometry = DefaultGeometries.Square,
                         PointGeometrySize = 8
                    },

                };

                Labels2 = new[] { "0", "150", "300", "450", "600", "750" };
                YFormatter2 = value => value.ToString();
                SeriesCollection2[0].Values.Add(5d);

                DataContext = this;


            }

            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro Gráfico 2", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            con.desconectar();


        }
        #endregion Gráfico Corrente


        #region Gráfico Frequência x Tensão 
        private void AtualizarGrafico3_Click(object sender, RoutedEventArgs e)
        {

            Conexao con = new Conexao();

            try
            {
                con.conectar();

                string sqlite3 = "SELECT Tensao, Frequencia FROM AtualizacaoVariaveis WHERE Tensao and Frequencia IS NOT NULL";
                var cmd3 = new SQLiteCommand(sqlite3, con.conn2);
                SQLiteDataAdapter dp3 = new SQLiteDataAdapter(cmd3);
                DataTable dt3 = new DataTable();
                dp3.Fill(dt3);

                var ContagemLinhas = dt3.Rows.Count;

                TensaoFrequenciaGrafico1 = Convert.ToInt32(dt3.Rows[ContagemLinhas - 1]["Tensao"].ToString());
                TensaoFrequenciaGrafico2 = Convert.ToInt32(dt3.Rows[ContagemLinhas - 2]["Tensao"].ToString());
                TensaoFrequenciaGrafico3 = Convert.ToInt32(dt3.Rows[ContagemLinhas - 3]["Tensao"].ToString());
                TensaoFrequenciaGrafico4 = Convert.ToInt32(dt3.Rows[ContagemLinhas - 4]["Tensao"].ToString());
                TensaoFrequenciaGrafico5 = Convert.ToInt32(dt3.Rows[ContagemLinhas - 5]["Tensao"].ToString());
                TensaoFrequenciaGrafico6 = Convert.ToInt32(dt3.Rows[ContagemLinhas - 6]["Tensao"].ToString());

                FrequenciaGrafico1 = Convert.ToInt32(dt3.Rows[ContagemLinhas - 1]["Frequencia"].ToString());
                FrequenciaGrafico2 = Convert.ToInt32(dt3.Rows[ContagemLinhas - 2]["Frequencia"].ToString());
                FrequenciaGrafico3 = Convert.ToInt32(dt3.Rows[ContagemLinhas - 3]["Frequencia"].ToString());
                FrequenciaGrafico4 = Convert.ToInt32(dt3.Rows[ContagemLinhas - 4]["Frequencia"].ToString());
                FrequenciaGrafico5 = Convert.ToInt32(dt3.Rows[ContagemLinhas - 5]["Frequencia"].ToString());
                FrequenciaGrafico6 = Convert.ToInt32(dt3.Rows[ContagemLinhas - 6]["Frequencia"].ToString());


                SeriesCollection3 = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Tensão x Frequência", FontSize = 3,
                        Values = new ChartValues<double> { TensaoFrequenciaGrafico1, TensaoFrequenciaGrafico2, TensaoFrequenciaGrafico3,
                            TensaoFrequenciaGrafico4, TensaoFrequenciaGrafico5, TensaoFrequenciaGrafico6},
                         PointGeometry = null
                    },

                };

                Labels3 = new[] {FrequenciaGrafico1.ToString(), FrequenciaGrafico2.ToString(), FrequenciaGrafico3.ToString(),
                FrequenciaGrafico4.ToString(), FrequenciaGrafico5.ToString(), FrequenciaGrafico6.ToString()}; 
                YFormatter3 = value => value.ToString();
                SeriesCollection3[0].Values.Add(5d);

                DataContext = this;


            }

            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro Gráfico 3", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            con.desconectar();
        }
        #endregion Gráfico Frequência x Tensão 

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public SeriesCollection SeriesCollection2 { get; set; }
        public string[] Labels2 { get; set; }
        public Func<double, string> YFormatter2 { get; set; }

        public SeriesCollection SeriesCollection3 { get; set; }
        public string[] Labels3 { get; set; }
        public Func<double, string> YFormatter3 { get; set; }

        public int FrequenciaGrafico1 { get; set; }
        public int FrequenciaGrafico2 { get; set; }
        public int FrequenciaGrafico3 { get; set; }
        public int FrequenciaGrafico4 { get; set; }
        public int FrequenciaGrafico5 { get; set; }
        public int FrequenciaGrafico6 { get; set; }

        public int CorrenteGrafico1 { get; set; }
        public int CorrenteGrafico2 { get; set; }
        public int CorrenteGrafico3 { get; set; }
        public int CorrenteGrafico4 { get; set; }
        public int CorrenteGrafico5 { get; set; }
        public int CorrenteGrafico6 { get; set; }



        public int TensaoFrequenciaGrafico1 { get; set; }
        public int TensaoFrequenciaGrafico2 { get; set; }
        public int TensaoFrequenciaGrafico3 { get; set; }
        public int TensaoFrequenciaGrafico4 { get; set; }
        public int TensaoFrequenciaGrafico5 { get; set; }
        public int TensaoFrequenciaGrafico6 { get; set; }

        #endregion Gráficos


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

        #endregion Bloco de funções 


    #region Voltar para Página Principal 
        private void Voltar3_Click(object sender, RoutedEventArgs e)
        {
            Window1 principal3 = new Window1();
            principal3.Show();
        }

        #endregion Voltar para Página Principal 


    #region Relatório
        private void Relatorio_Click(object sender, RoutedEventArgs e)
        {

            Conexao con = new Conexao();

            Document doc = new Document(PageSize.A4);           //tipo da página
            doc.SetMargins(40, 40, 40, 80);                     //margens da página
            string caminho = @"C:\Relatorio\" + "RelatórioLeituradeVariaveis.pdf";

            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(caminho, FileMode.Create));

            doc.Open();

            try
            {
                con.conectar();

                string sqlite = "SELECT Velocidade, Frequencia, Tensao, Corrente, Potencia, Torque, Aceleracao, Desaceleracao, Sobrecarga, Contador FROM AtualizacaoVariaveis";
                var cmd = new SQLiteCommand(sqlite, con.conn2);
                SQLiteDataReader rdr = cmd.ExecuteReader();

                string sqlite2 = "SELECT Nome FROM Login";
                var cmd2 = new SQLiteCommand(sqlite2, con.conn);
                SQLiteDataReader rdr2 = cmd2.ExecuteReader();

                rdr.Read();
                rdr2.Read();

                Paragraph titulo = new Paragraph();
                titulo.Font = new Font(Font.FontFamily.COURIER, 16); //fonte e tamanho do título
                titulo.Alignment = Element.ALIGN_CENTER;            // alinhamento do título no centro 
                titulo.Add("Sistema de Supervisão e Controle - Relatório\n\n"); //título
                doc.Add(titulo);                                    //adiciona o parágrafo no documento


                Paragraph Usuario = new Paragraph("", new Font(Font.NORMAL, 12));
                string login = "Usuário:" + " " + rdr2["Nome"].ToString() + "\n";
                Usuario.Add(login);
                doc.Add(Usuario);

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

                con.desconectar();

            }
            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            doc.Close();

        }
        #endregion Relatório 


    #region Atualização Assincrona

        public void LeiturasSistema()
        {
            Conexao con = new Conexao();
            try
            {
                con.conectar();

                try
                {
                    serialPort1 = new SerialPort(PortaSerialCom3.Text, 57600, Parity.Even, 8, StopBits.One)
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
                        #region Velocidade

                        startAddress = 2;         //REGISTRADOR

                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(200);

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

                            LerVelocidade.Text = result;
                            StringVelocidade = result;
                        }
                        #endregion Velocidade


                        #region Frequência    
                        startAddress = 5;
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);

                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(200);

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

                            LerFrequencia.Text = result;
                            StringFrequencia = result;
                        }
                        #endregion Frequência

                        #region Tensão
                        startAddress = 7;                                                  //registrador
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(200);                                                      // Delay 100ms

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();
                            //LerTensão.Text = Display(bufferReceiver);

                            //processamento dos dados
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];

                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];

                            String result = string.Format("{0}", valorConvertido);

                            LerTensão.Text = result;
                            StringTensão = result;
                        }
                        #endregion Tensão 


                        String sqlite = "INSERT INTO AtualizacaoVariaveis(Velocidade, Frequencia, Tensao) " +
                            "VALUES ('" + LerVelocidade.Text + "', '" + LerFrequencia.Text + "', '" + LerTensão.Text + "')";
                        SQLiteCommand comando = new SQLiteCommand(sqlite, con.conn2);
                        _ = comando.ExecuteNonQuery();
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
                MessageBox.Show(E.Message.ToString(), "Erro8", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            con.desconectar();

        }

        public void LeiturasSistema2()
        {
            Conexao con = new Conexao();
            try
            {
                con.conectar();

                try
                {
                    serialPort1 = new SerialPort(PortaSerialCom3.Text, 57600, Parity.Even, 8, StopBits.One)
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
                        
                        #region Corrente
                        startAddress = 3;                                                  //registrador
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);

                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(200);                                                      // Delay 100ms

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();
                            //LerCorrente.Text = Display(bufferReceiver);

                            //processamento dos dados
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];
                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];

                            String result = string.Format("{0}", valorConvertido);

                            LerCorrente.Text = result;
                            StringCorrente = result;
                        }
                        #endregion Corrente


                        #region Potência 
                        startAddress = 10;
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(200);

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();
                            //LerPotencia.Text = Display(bufferReceiver);

                            //processamento dos dados
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];
                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];

                            String result = string.Format("{0}", valorConvertido);

                            LerPotencia.Text = result;
                            StringPotencia = result;
                        }
                        #endregion Potência 


                        #region Torque
                        startAddress = 9;                                                  //registrador
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
      
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(200);                                                      // Delay 100ms

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();
                            //LerTorque.Text = Display(bufferReceiver);

                            //processamento dos dados
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];
                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];

                            String result = string.Format("{0}", valorConvertido);

                            LerTorque.Text = result;
                            StringTorque = result;
                        }

                        #endregion Torque


                        String sqlite = "INSERT INTO AtualizacaoVariaveis(Corrente, Potencia, Torque) " +
                            "VALUES ('" + LerCorrente.Text + "', '" + LerPotencia.Text + "', '" + LerTorque.Text + "')";
                        SQLiteCommand comando = new SQLiteCommand(sqlite, con.conn2);
                        _ = comando.ExecuteNonQuery();
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
                MessageBox.Show(E.Message.ToString(), "Erro8", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            con.desconectar();

        }

        public void LeiturasSistema3()
        {
            Conexao con = new Conexao();
            try
            {
                con.conectar();

                try
                {
                    serialPort1 = new SerialPort(PortaSerialCom3.Text, 57600, Parity.Even, 8, StopBits.One)
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

                        #region Tempo de Aceleração

                        startAddress = 100; //registrador
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(200);                                                      


                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();
                            //LerAceleração.Text = Display(bufferReceiver);

                            //processamento dos dados
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];
                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];

                            String result = string.Format("{0}", valorConvertido / 10);

                            LerAceleração.Text = result;
                            StringAceleração = result;
                        }
                        else
                        {
                            MessageBox.Show("Não recebi nada");
                        }
                        #endregion Tempo de Aceleração


                        #region Tempo de Desaceleração
                        startAddress = 101;                                                  //registrador
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(200);                                                      // Delay 100ms

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();

                            //processamento dos dados
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];
                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];

                            String result = string.Format("{0}", valorConvertido / 10);

                            LerDesaceleração.Text = result;
                            StringDesaceleração = result;
                        }

                        #endregion Tempo de desaceleração


                        #region Sobrecarga
                        startAddress = 37;                                                  //registrador
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        LerSobrecarga.Text = StringSobrecarga;                                    //Recebe o resultado da função Display 
                        serialPort1.Write(frame, 0, frame.Length);
                        System.Threading.Thread.Sleep(200);                                                      // Delay 100ms

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();
                            //LerSobrecarga.Text = Display(bufferReceiver);

                            //processamento dos dados
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];
                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];

                            String result = string.Format("{0}", valorConvertido);

                            LerSobrecarga.Text = result;
                            StringSobrecarga = result;
                        }
                        #endregion Sobrecarga


                        #region Contador
                        startAddress = 44;                                                  //registrador
                        frame = ReadHoldingRegistersMsg(slaveAddress, startAddress, function, numberOfPoints);
                        serialPort1.Write(frame, 0, frame.Length);
                        System.Threading.Thread.Sleep(200);                                                      // Delay 100ms

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();
                            //LerContador.Text = Display(bufferReceiver);

                            //processamento dos dados
                            byte[] data = new byte[2];
                            data[0] = bufferReceiver[3];
                            data[1] = bufferReceiver[4];
                            UInt16[] valores = Word.ByteToUInt16(data);
                            UInt16 valorConvertido = valores[0];

                            String result = string.Format("{0}", valorConvertido);

                            LerContador.Text = result;
                            StringContador = result;
                        }
                        #endregion Contador

                        String sqlite = "INSERT INTO AtualizacaoVariaveis(Aceleracao, Desaceleracao, Sobrecarga, Contador) " +
                            "VALUES ('" + LerAceleração.Text + "', '" + LerDesaceleração.Text + "', '" + LerSobrecarga.Text + "', '" + LerContador.Text + "')";
                        SQLiteCommand comando = new SQLiteCommand(sqlite, con.conn2);
                        _ = comando.ExecuteNonQuery();
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
                MessageBox.Show(E.Message.ToString(), "Erro8", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            con.desconectar();

        }



        public async void Atualizar_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    this.Dispatcher.Invoke(() => { LeiturasSistema(); });

                    this.Dispatcher.Invoke(() => { LeiturasSistema2(); });

                    this.Dispatcher.Invoke(() => { LeiturasSistema3(); });


                    await Task.Delay(1000);
                }
          });
        }

    }    
 

    #endregion Atualização Assincrona

}

