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
using System.Windows.Shapes;
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions;
using ProjetoCanaldeHidraulica.CadastrodoUsuario;
using System.Data.SQLite;
using System.Data.SqlClient;
using System.Data;

namespace ProjetoCanaldeHidraulica
{
    /// <summary>
    /// Lógica interna para Acionamento.xaml
    /// </summary>
    public partial class Acionamento : Window
    {
        public Acionamento(string recebeCOM)
        {
            InitializeComponent();

            PortaSerialCom2.Text = recebeCOM;

            Conexao con = new Conexao();
            try
            {
                con.conectar();


                string sqlite = "SELECT Vertedor FROM AtualizacaoVariaveis WHERE Vertedor IS NOT NULL";
                var cmd = new SQLiteCommand(sqlite, con.conn2);
                SQLiteDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    LeituraOpcaoVertedor.Text = rdr["Vertedor"].ToString();
                }

            }
            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro", MessageBoxButton.OK, MessageBoxImage.Error);

            }

            con.desconectar();

        }

        private bool jaDefiniuVelocidade = false;
        public const int VelocidadeMaxima = 1350; 
        public const int VelocidadeMinima = 450;
        SerialPort serialPort1 = new SerialPort(); 



        #region Funções
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
        #endregion Funções

        #region Código da ComboBox
        private void ComboVelocidade_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem)ComboVelocidade.SelectedValue;
            Velocidade.Text = (string)item.Content;

        }

        private void ComboAceleracao_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem)ComboAceleracao.SelectedValue;
            TempoAceleracao.Text = (string)item.Content;
        }

        private void ComboDesaceleracao_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem)ComboDesaceleracao.SelectedValue;
            TempoDesaceleracao.Text = (string)item.Content;
        }

        private void ComboCurva_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem)ComboCurva.SelectedValue;
            TesteCurva.Text = (string)item.Content;
        }
        #endregion Código da ComboBox




        #region Escrita Aceleração
        public void ConfAceleracao_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                serialPort1 = new SerialPort(PortaSerialCom2.Text, 57600, Parity.Even, 8, StopBits.One)
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
                byte slaveAddress = 246;
                byte function = 6;
                ushort startAddress = 100;
                decimal value = decimal.Parse(TempoAceleracao.Text);
                ushort valor = Convert.ToUInt16(value * 10);

                if (serialPort1.IsOpen)
                {
                    byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(valor));
                    //TempoAceleracao.Text = Display(frame);                                        //conectado com a TextBox que vai mostrar a velocidade 
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
        }

        #endregion Escrita Aceleração

        #region Escrita Desaceleração
        private void ConfDesaceleracao_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                serialPort1 = new SerialPort(PortaSerialCom2.Text, 57600, Parity.Even, 8, StopBits.One)
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
                ushort startAddress = 101;                                                   //registrador
                                                                                             // ushort value = Convert.ToUInt16(TempoDesaceleracao.Text);
                decimal value = decimal.Parse(TempoDesaceleracao.Text);
                ushort valor = Convert.ToUInt16(value * 10);


                if (serialPort1.IsOpen)
                {

                    byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(valor));
                    //TempoDesaceleracao.Text = Display(frame);                                        //conectado com a TextBox que vai mostrar a velocidade 
                    serialPort1.Write(frame, 0, frame.Length);
                    Thread.Sleep(100);                                                       //Delay 100ms

                    if (serialPort1.BytesToRead >= 5)
                    {
                        byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                        serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                        serialPort1.DiscardInBuffer();
                        //TempoDesaceleracao.Text = Display(bufferReceiver);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            serialPort1.Close();
        }
        #endregion Escrita Desaceleração

        #region Escrita Velocidade
        public void ConfVelocidade_Click(object sender, RoutedEventArgs e)
        {

            int Velocidade13bits = ((Convert.ToInt32(Velocidade.Text) * 8192) / 1800);

            if (!Regex.IsMatch(Velocidade.Text, @"^[0-9]+$"))
            {
                MessageBox.Show("Digite um valor válido!", "Atenção!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Velocidade.Clear();
            }

            else
            {
                MessageBox.Show("Lembre-se: usando esta opção você NÃO pode definir uma VAZÃO ou um valor de CARGA HIDRÁULICA!", "Controle DESATIVADO", MessageBoxButton.OK, MessageBoxImage.Information);

                if (Convert.ToInt32(Velocidade.Text) > VelocidadeMaxima || Convert.ToInt32(Velocidade.Text) < VelocidadeMinima)
                {
                    MessageBox.Show("Verifique a velocidade digitada! Valores inferiores a" + " " + VelocidadeMinima
                        + " " + "rpm não geram energia suficiente para a bomba e valores acima de" + " " + VelocidadeMaxima + " "
                        + "rpm ultrapassam o limite do vertedor.", "ATENÇÃO!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                try
                {
                    serialPort1 = new SerialPort(PortaSerialCom2.Text, 57600, Parity.Even, 8, StopBits.One)
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
                    byte slaveAddress = 246;                                           //endereço do escravo  
                    byte function = 6;                                                 //código do de função Modbus
                    ushort startAddress = 683;                                         //registrador
                                                                                       //ushort value = Convert.ToUInt16(Velocidade13bits);
                                                                                       //decimal value = decimal.Parse(Velocidade13bits);
                                                                                       //ushort valor = Convert.ToUInt16(value * 10);
                    ushort valor = Convert.ToUInt16(Velocidade13bits);

                    if (serialPort1.IsOpen)
                    {

                        byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(valor));
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(100);                                            //Delay 100ms
                        jaDefiniuVelocidade = true;


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
            }
            serialPort1.Close();
        }
        #endregion Escrita Velocidade

        #region Escrita Curva 

        private void ConfCurva_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                serialPort1 = new SerialPort(PortaSerialCom2.Text, 57600, Parity.Even, 8, StopBits.One)
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
                ushort startAddress = 104;                                                   //registrador
                ushort value = Convert.ToUInt16(TesteCurva.Text);


                if (serialPort1.IsOpen)
                {

                    byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value));
                    //Teste.Text = Display(frame);                                        //conectado com a TextBox que vai mostrar a velocidade 
                    serialPort1.Write(frame, 0, frame.Length);
                    Thread.Sleep(100);                                                       //Delay 100ms

                    if (serialPort1.BytesToRead >= 5)
                    {
                        byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                        serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                        serialPort1.DiscardInBuffer();
                        //Teste.Text = Display(bufferReceiver);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            serialPort1.Close();

        }
        #endregion Escrita Curva

        #region Ligar o Motor
        private void BotaoLiga_Click(object sender, RoutedEventArgs e)
        {

            if (!jaDefiniuVelocidade)
            {
                MessageBox.Show("É necessário definir a VELOCIDADE, informar a VAZÃO ou PORCENTAGEM do setpoint antes de acionar o motor!", "Atenção!", MessageBoxButton.OKCancel, MessageBoxImage.Stop);
                return;
            }

            #region Bits de entrada para acionar o motor 


            byte menosSignificativo = 0b00010111;
            byte maisSignificativo = 0b00000000;

            int LigaoMotor = (maisSignificativo << 8) | menosSignificativo;


            #endregion Bits de entrada para acionar o motor


            try
            {
                serialPort1 = new SerialPort(PortaSerialCom2.Text, 57600, Parity.Even, 8, StopBits.One)
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
                ushort value = Convert.ToUInt16(LigaoMotor);


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

        }
        #endregion Ligar o Motor 

        #region Desligar o Motor
        private void BotaoDesliga_Click(object sender, RoutedEventArgs e)
        {
            #region Bits de entrada para desacionar o motor 

            byte menosSignificativo = 0b00000110; //bit 5 colocado para 1 
            byte maisSignificativo = 0b00000000;

            int DesligaroMotor = (maisSignificativo << 8) | menosSignificativo;

            #endregion Bits de entrada para desacionar o motor

            try
            {

                serialPort1 = new SerialPort(PortaSerialCom2.Text, 57600, Parity.Even, 8, StopBits.One)
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
                ushort value = Convert.ToUInt16(DesligaroMotor);


                if (serialPort1.IsOpen)
                {

                    byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(value));
                    //Teste8.Text = Display(frame);                                        //Leitura da mensagem enviada para o escravo
                    serialPort1.Write(frame, 0, frame.Length);
                    Thread.Sleep(100);                                                       //Delay 100ms

                    if (serialPort1.BytesToRead >= 5)
                    {
                        byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                        serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                        serialPort1.DiscardInBuffer();
                        //Teste7.Text = Display(bufferReceiver);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            serialPort1.Close();
        }

        #endregion Desligar o Motor 

        #region Botão de Emergência

        private void BotaoEmergencia_Click(object sender, RoutedEventArgs e)
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

                    serialPort1 = new SerialPort(PortaSerialCom2.Text, 57600, Parity.Even, 8, StopBits.One)
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
                        //Teste8.Text = Display(frame);                                        //Leitura da mensagem enviada para o escravo
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(100);                                                       //Delay 100ms

                        if (serialPort1.BytesToRead >= 5)
                        {
                            byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
                            serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
                            serialPort1.DiscardInBuffer();
                            //Teste7.Text = Display(bufferReceiver);

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                serialPort1.Close();
            }
            else
            {
                //nada acontece
            }

        }


        #endregion Botão de Emergência 

        #region Escrita Vazão
        private void ConfVazao_Click(object sender, RoutedEventArgs e)
        {
            Conexao con = new Conexao();

            MessageBox.Show("Lembre-se: NÃO é necessário definir a velocidade do motor!", "Controle ATIVADO", MessageBoxButton.OK, MessageBoxImage.Information);

            try
            {
                serialPort1 = new SerialPort(PortaSerialCom2.Text, 57600, Parity.Even, 8, StopBits.One)
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
                con.conectar();

                try
                {

                    string sqlite = "SELECT VazaoMaxima FROM AtualizacaoVariaveis WHERE VazaoMaxima IS NOT NULL";
                    var cmd = new SQLiteCommand(sqlite, con.conn2);
                    //SQLiteDataReader rdr = cmd.ExecuteReader();
                    SQLiteDataAdapter rdr = new SQLiteDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    rdr.Fill(dt);

                    var ContagemLinhas = dt.Rows.Count;

                    
                    byte slaveAddress = 246;
                    byte function = 6;
                    ushort startAddress = 683;

                    decimal value = decimal.Parse(EscritaVazao.Text);
                    decimal valor1 = Convert.ToDecimal(dt.Rows[ContagemLinhas - 1]["VazaoMaxima"]);   //165.0M;
                    ushort valor = Convert.ToUInt16((value * 100) / valor1); // equação para transformar a vazão escrita em porcentagem 
                    ushort valor2 = Convert.ToUInt16((valor * 1000) / 1000); //equação que converte a porcentagem em referencia 13 bits


                    if (serialPort1.IsOpen)
                    {

                        byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(valor2));
                        serialPort1.Write(frame, 0, frame.Length);
                        Thread.Sleep(100);
                        jaDefiniuVelocidade = true;

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

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            con.desconectar();
        }

        #endregion Escrita Vazão

        //#region Escrita Carga Hidráulica
        //private void ConfNivel_Click(object sender, RoutedEventArgs e)
        //{

        //        MessageBox.Show("Lembre-se: NÃO é necessário definir a velocidade do motor!", "Controle ATIVADO", MessageBoxButton.OK, MessageBoxImage.Information);

        //            try
        //            {
        //                serialPort1 = new SerialPort(PortaSerialCom2.Text, 57600, Parity.Even, 8, StopBits.One)
        //                {
        //                    DtrEnable = true,
        //                    RtsEnable = true
        //                };
        //                serialPort1.Open();
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show(ex.Message);
        //            }

        //            try
        //            {
        //                byte slaveAddress = 246;
        //                byte function = 6;
        //                ushort startAddress = 683;
        //                double valor = 0;

        //                decimal value = decimal.Parse(EscritaNivel.Text);
        //                double NivelLido = Double.Parse(EscritaNivel.Text);

                    
        //            if (LeituraOpcaoVertedor.Text == "Triangular 90°")
        //            {
        //                valor = Math.Pow(NivelLido, 2.5) * 1.420; // escreve a vazão com base no valor de nível escrito pelo usuário e convertido para double (por causa do Math.Pow)
        //            }

        //            if (LeituraOpcaoVertedor.Text == "Triangular 120°")
        //            {
        //                valor = Math.Pow(NivelLido, 2.449) * 2.302; // escreve a vazão com base no valor de nível escrito pelo usuário e convertido para double (por causa do Math.Pow)
        //            }

        //            if (LeituraOpcaoVertedor.Text == "Triangular 135°")
        //            {
        //                valor = Math.Pow(NivelLido, 2.47) * 3.187; // escreve vazão com base no valor de nível escrito pelo usuário e convertido para double (por causa do Math.Pow)
        //            }

        //            if (LeituraOpcaoVertedor.Text == "Triangular 90° truncado")
        //            {
        //                var vertedor = new Vertedor();
        //                double DefinicaoHb2 = Double.Parse(vertedor.DefiniçãoHb.Text);
        //                double variavel1 = NivelLido - DefinicaoHb2;
        //                valor = (Math.Pow(NivelLido, 2.47) - (Math.Pow(variavel1, 2.47))) * 1.32; // escreve a vazão com base no valor de nível escrito pelo usuário e convertido para double (por causa do Math.Pow)
        //            }

        //            if (LeituraOpcaoVertedor.Text == "Triangular 120° truncado")
        //            {
        //                var vertedor = new Vertedor();
        //                double DefinicaoHb3 = Double.Parse(vertedor.DefiniçãoHb.Text);
        //                double variavel2 = NivelLido - DefinicaoHb3;
        //                valor = (Math.Pow(NivelLido, 2.449) - (Math.Pow(variavel2, 2.449))) * 2.302; // escreve a vazão com base no valor de nível escrito pelo usuário e convertido para double (por causa do Math.Pow)
        //            }

        //            if (LeituraOpcaoVertedor.Text == "Triangular trapezoidal (Tipo Cipoletti)")
        //            {
        //                var vertedor = new Vertedor();
        //                double DefinicaoL1 = double.Parse(vertedor.DefiniçãoL.Text);
        //                valor = Math.Pow(NivelLido, 1.5) * 1.86 * DefinicaoL1; // escreve a vazão com base no valor de nível escrito pelo usuário e convertido para double (por causa do Math.Pow)
        //            }

        //            ushort valor1 = Convert.ToUInt16((valor * 100) / 165); // equação para transformar a vazão escrita pelo usuário para rpm, considerando a relação 165 l/s - 1350 rpm
        //            ushort valor2 = Convert.ToUInt16((valor1 * 1000) / 100); //  equação que converte o rpm resultante para a referência de 13 bits requerida pelo P0683


        //        if (serialPort1.IsOpen)
        //                {

        //                    byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(valor2));
        //                    serialPort1.Write(frame, 0, frame.Length);
        //                    Thread.Sleep(100);
        //                    jaDefiniuVelocidade = true;


        //            if (serialPort1.BytesToRead >= 5)
        //                    {
        //                        byte[] bufferReceiver = new byte[this.serialPort1.BytesToRead];
        //                        serialPort1.Read(bufferReceiver, 0, serialPort1.BytesToRead);
        //                        serialPort1.DiscardInBuffer();
        //                    }
        //                }
        //            }

        //            catch (Exception ex)
        //            {
        //                MessageBox.Show(ex.Message);
        //            }

        //            serialPort1.Close();
        //}


        //#endregion Carga Hidráulica

        #region Chamada da tela Ajuda
        private void ChamarAjuda_Click(object sender, RoutedEventArgs e)
        {
            Ajuda ajuda = new Ajuda();
            ajuda.Show();
        }
        #endregion Chamada da tela Ajuda

        #region Escrita Porcentagem
        private void ConfPorcentagem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Lembre-se: NÃO é necessário definir a velocidade do motor!", "Controle ATIVADO", MessageBoxButton.OK, MessageBoxImage.Information);
            try
            {

                serialPort1 = new SerialPort(PortaSerialCom2.Text, 57600, Parity.Even, 8, StopBits.One)
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
                ushort startAddress = 683;


                decimal value = decimal.Parse(EscritaPorcentagem.Text);
                ushort valor = Convert.ToUInt16((value * 1000) / 100); // equação que converte a porcentagem em referencia 13 bits


                if (serialPort1.IsOpen)
                {

                    byte[] frame = WriteHoldingRegistersMsg(slaveAddress, startAddress, function, ToByteArray(valor));
                    serialPort1.Write(frame, 0, frame.Length);
                    Thread.Sleep(100);
                    jaDefiniuVelocidade = true;

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
        #endregion Escrita Porcentagem





        private void Voltar2_Click(object sender, RoutedEventArgs e)
        {
            Window1 principal2 = new Window1();
            principal2.Show();
        }

        private void AjudaTelaAcionamento_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Para a definição do Setpoint de controle, conside a vazão máxima.", "Observação", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
