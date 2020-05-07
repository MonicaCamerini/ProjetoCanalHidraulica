using ProjetoCanaldeHidraulica.CadastrodoUsuario;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.io;
using iTextSharp.text.api;
using Font = iTextSharp.text.Font;
using Paragraph = iTextSharp.text.Paragraph;
using System.IO;
using System.Data.SQLite;

namespace ProjetoCanaldeHidraulica
{
    /// <summary>
    /// Lógica interna para VazãoeNível.xaml
    /// </summary>
    public partial class VazãoeNível : Window
    {
        SerialPort serialPort1 = new SerialPort();
        Window1 TelaPrincipal = new Window1();

        public VazãoeNível(string RecebePortaCom)
        {

            InitializeComponent();
            PortaSerialCom4.Text = RecebePortaCom;

            Conexao con = new Conexao();
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
                        #region Variável de Processo 

                        startAddress = 1012;         //REGISTRADOR

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

                            String result = string.Format("{0}", valorConvertido / 10);

                            VariavelProcesso.Text = result + " " + "litros/segundo";
                        }
                        #endregion Variável de Processo 

                        #region Setpoint de controle

                        startAddress = 1011;         //REGISTRADOR

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

                            Setpoint.Text = result;
                        }
                        #endregion Setpoint de controle

                        #region Saída do PID %

                        startAddress = 1013;         //REGISTRADOR

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

                            SaídaPID.Text = result;
                        }
                        #endregion Saída do PID %

                        #region Velocidade Mínima

                        startAddress = 133;         //REGISTRADOR

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

                            VelocidadeMinima.Text = result;
                        }
                        #endregion Velocidade Mínima

                        #region Velocidade Máxima

                        startAddress = 134;         //REGISTRADOR

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

                            VelocidadeMaxima.Text = result;
                        }
                        #endregion Velocidade Máxima

                        #region Ganho Kp

                        startAddress = 1020;         //REGISTRADOR

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

                            GanhoProporcional.Text = result;
                        }
                        #endregion Ganho Kp

                        #region Ganho Ki

                        startAddress = 1021;         //REGISTRADOR

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

                            GanhoIntegral.Text = "0," + result;
                        }
                        #endregion Ganho Ki

                        #region Ganho Kd

                        startAddress = 1022;         //REGISTRADOR

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

                            GanhoDerivativo.Text = "0," + result;
                        }
                        #endregion Ganho Kd

                        #region Filtro Setpoint

                        startAddress = 1023;         //REGISTRADOR

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

                            FiltroSetpoint.Text = "0," + result + " " + "Segundos";
                        }
                        #endregion Filtro Setpoint

                        #region Ação de Controle

                        startAddress = 1024;         //REGISTRADOR

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

                            if (result == Convert.ToString(0))
                            {
                                AçãoControle.Text = result + ":" + " " + "Direto";
                            }

                            if (result == Convert.ToString(1))
                            {
                                AçãoControle.Text = result + ":" + " " + "Reverso";
                            }

                        }
                        #endregion Ação de Controle

                        #region Fundo de Escala

                        startAddress = 1018;         //REGISTRADOR

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

                            FundoEscala.Text = result + "." + "0";
                        }
                        #endregion Fundo de Escala




                        String sqlite = "INSERT INTO AtualizacaoVariaveis(Setpoint, VariavelProcesso, SaidaPID, GanhoKp, GanhoKi, GanhoKd, FiltroSetpoint) " +
                            "VALUES ('" + Setpoint.Text + "', '" + VariavelProcesso.Text + "', '" + SaídaPID.Text + "', '" + GanhoProporcional.Text + "', '" + GanhoIntegral.Text + "', '" + GanhoDerivativo.Text + "', '" + FiltroSetpoint.Text + "')";
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
                MessageBox.Show(E.Message.ToString(), "Erro 1 Tela Vazao e Controle", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            con.desconectar();

        }

        #region Leituras

        public void LeiturasVariaveis()
        {
            Conexao con = new Conexao();
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

                    byte slaveAddress = 246;
                    byte function = 3;
                    uint numberOfPoints = 1;
                    ushort startAddress = 0;
                    byte[] frame;


                    if (serialPort1.IsOpen)
                    {
                        #region Variável de Processo 

                        startAddress = 1012;         //REGISTRADOR

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

                            String result = string.Format("{0}", valorConvertido / 10);

                            VariavelProcesso.Text = result + " " + "litros/segundo";
                        }
                        #endregion Variável de Processo 

                        #region Setpoint de controle

                        startAddress = 1011;         //REGISTRADOR

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

                            String result = string.Format("{0}", valorConvertido / 10);

                            Setpoint.Text = result;
                        }
                        #endregion Setpoint de controle

                        #region Saída do PID %

                        startAddress = 1013;         //REGISTRADOR

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

                            SaídaPID.Text = result;
                        }
                        #endregion Saída do PID %

                        #region Velocidade Mínima

                        startAddress = 133;         //REGISTRADOR

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

                            VelocidadeMinima.Text = result;
                        }
                        #endregion Velocidade Mínima

                        #region Velocidade Máxima

                        startAddress = 134;         //REGISTRADOR

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

                            VelocidadeMaxima.Text = result;
                        }
                        #endregion Velocidade Máxima

                        #region Ganho Kp

                        startAddress = 1020;         //REGISTRADOR

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

                            GanhoProporcional.Text = "0," + result;
                        }
                        #endregion Ganho Kp

                        #region Ganho Ki

                        startAddress = 1021;         //REGISTRADOR

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

                            GanhoIntegral.Text = "0," + result;
                        }
                        #endregion Ganho Ki

                        #region Ganho Kd

                        startAddress = 1022;         //REGISTRADOR

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

                            GanhoDerivativo.Text = "0," + result;
                        }
                        #endregion Ganho Kd

                        #region Filtro Setpoint

                        startAddress = 1023;         //REGISTRADOR

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

                            FiltroSetpoint.Text = "0," + result + " " + "Segundos";
                        }
                        #endregion Filtro Setpoint

                        String sqlite = "INSERT INTO AtualizacaoVariaveis(Setpoint, VariavelProcesso, SaidaPID, GanhoKp, GanhoKi, GanhoKd, FiltroSetpoint) " +
                            "VALUES ('" + Setpoint.Text + "', '" + VariavelProcesso.Text + "', '" + SaídaPID.Text + "', '" + GanhoProporcional.Text + "', '" + GanhoIntegral.Text + "', '" + GanhoDerivativo.Text + "', '" + FiltroSetpoint.Text + "')";
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
                MessageBox.Show(E.Message.ToString(), "Erro 2 Tela Vazão e controle", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            con.desconectar();
        }



        private async void LeituraVariaveisBotao_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    this.Dispatcher.Invoke(() => { LeiturasVariaveis(); });

                    await Task.Delay(100);

                }
            });
        }


        #endregion Leituras

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
        /// Função 06 - Escrita de Registrador 
        /// </summary>
        /// <param name="slaveAddress">/Endereço do Escravo</param>
        /// <param name="startAddress">Endereço do Registrador Inicial</param>
        /// <param name="function">Código de Função</param>
        /// <param name="numberOfPoints">Quantidade de registradores</param>
        /// <returns>Byte Array</returns>
        byte[] WriteHoldingRegistersMsg(byte slaveAddress, ushort startAddress, byte function, byte[] values)
        {
            byte[] frame = new byte[8];
            frame[0] = slaveAddress;                        // Endereço do escravo
            frame[1] = function;                            // Código de função             
            frame[2] = (byte)(startAddress >> 8);           // Endereço inicial alto
            frame[3] = (byte)startAddress;                  // Endereço inicial baixo           
            Array.Copy(values, 0, frame, 4, values.Length);
            byte[] crc = CalculateCRC(frame);               // Calculo do CRC
            frame[frame.Length - 2] = crc[0];               // Verificação de erro baixa
            frame[frame.Length - 1] = crc[1];               // Verificação de erro alta
            return frame;
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


        #region Voltar página principal 
        private void Voltar4_Click(object sender, RoutedEventArgs e)
        {
            Window1 principal4 = new Window1();
            principal4.Show();
        }

        #endregion Voltar página Principal 

        #region Relatório
        private void Relatorio_Click(object sender, RoutedEventArgs e)
        {

            Conexao con = new Conexao();

            Document doc = new Document(PageSize.A4);           //tipo da página
            doc.SetMargins(40, 40, 40, 80);                     //margens da página
            string caminho = @"C:\Relatorio\" + "RelatórioVazãoeNível.pdf";

            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(caminho, FileMode.Create));

            doc.Open();

            try
            {
                con.conectar();

                string sqlite = "SELECT Vertedor, Vazao1, Vazao2, CargaHidraulica, Setpoint FROM AtualizacaoVariaveis";
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

                Paragraph TextoTabela2 = new Paragraph("", new Font(Font.NORMAL, 12));
                string textotabela2 = "Tabela 1: Contém as leituras das variáveis relacionadas ao sistema de controle.\n\n";
                TextoTabela2.Add(textotabela2);
                doc.Add(TextoTabela2);

                PdfPTable tabela2 = new PdfPTable(2); //número de colunas 

                tabela2.AddCell(" ");
                tabela2.AddCell("Último Valor Lido");

                tabela2.AddCell("Vazão - Leitura 1");
                tabela2.AddCell(rdr["Vazao1"].ToString() + " " + "m³/s");

                tabela2.AddCell("Vazão - Leitura 2");
                tabela2.AddCell(rdr["Vazao2"].ToString() + " " + "litros/s");

                tabela2.AddCell("Vertedor");
                tabela2.AddCell(rdr["Vertedor"].ToString());

                tabela2.AddCell("Carga Hidraulica");
                tabela2.AddCell(rdr["CargaHidraulica"].ToString() + " " + "cm");

                tabela2.AddCell("Setpoint");
                tabela2.AddCell(rdr["Setpoint"].ToString());

                doc.Add(tabela2);


                con.desconectar();

            }
            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            doc.Close();

        }
        #endregion Relatório

        #region Escrita Velocidade Minima
        private void ConfirmaVelocidadeMinima_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                serialPort1 = new SerialPort(PortaSerialCom4.Text, 57600, Parity.Even, 8, StopBits.One)
                {
                    DtrEnable = true,
                    RtsEnable = true
                };
                serialPort1.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            try
            {
                byte slaveAddress = 246;
                byte function = 6;
                ushort startAddress = 133;


                decimal value = decimal.Parse(VelocidadeMinimaEscrita.Text);
                ushort valor = Convert.ToUInt16(value);


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

            serialPort1.Close();
        }

        #endregion Escrita Velocidade Minima

        #region Escrita Velocidade Maxima
        private void ConfirmaVelocidadeMaxima_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                serialPort1 = new SerialPort(PortaSerialCom4.Text, 57600, Parity.Even, 8, StopBits.One)
                {
                    DtrEnable = true,
                    RtsEnable = true
                };
                serialPort1.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            try
            {
                byte slaveAddress = 246;
                byte function = 6;
                ushort startAddress = 134;


                decimal value = decimal.Parse(VelocidadeMaximaEscrita.Text);
                ushort valor = Convert.ToUInt16(value);


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

            serialPort1.Close();
        }


        #endregion Escrita Velocidade Maxima 



        #region Botões para Ajuda

        private void AjudaFundoEscala_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Define como será apresentada a variável de processo do Controlador PID " +
                "(em litros/segundo ou em porcentagem, por exemplo). O formato da variável sempre será wxy.z." +
                "\nEsta deve ser definida diretamente no inversor de frequência no parâmetro P1018 (no momento é utilizado 165.0 " +
                "litros/segundo).", "Ajuda sobre Fundo de Escala", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AjudaVazaoMaxima_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Considere a vazão máxima para o vertedor que estiver utilizando no momento.",
                "Observação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion Botões para Ajuda

        private void ConfirmaVazaoMaxima_Click(object sender, RoutedEventArgs e)
        {
            Conexao con = new Conexao();
            try
            {
                con.conectar();


                String sqlite = "INSERT INTO AtualizacaoVariaveis(VazaoMaxima) VALUES ('" + VazaoMaxima.Text + "')";
                SQLiteCommand comando = new SQLiteCommand(sqlite, con.conn2);
                _ = comando.ExecuteNonQuery();
            }

            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro 3 Tela Vazão e controle", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            con.desconectar();
        }
    }
}
