using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using ProjetoCanaldeHidraulica.CadastrodoUsuario;
using System.Data.SQLite;

namespace ProjetoCanaldeHidraulica
{
    /// <summary>
    /// Lógica interna para Vertedor.xaml
    /// </summary>
    public partial class Vertedor : Window
    {
        public Vertedor()
        {
            InitializeComponent();
        }

        public Vertedor(string ValorVazao) // Recebo a vazão da página principal (nome da viarável criada aqui)
        {
            InitializeComponent();
            RecebeVazao.Text = ValorVazao;
        }


        #region Escolha Vertedor
        private void ComboVertedor_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem)ComboVertedor.SelectedValue;
            VertedorEscolhido.Text = (string)item.Content;

            var windows = new Window1(VertedorEscolhido.Text);


            if (VertedorEscolhido.Text == "Retangular com contração")
            {
                DefiniçãoL.Visibility = Visibility.Visible;
                DefiniçãoLTexto.Visibility = Visibility.Visible;
                DefiniçãoHb.Visibility = Visibility.Hidden;
                DefiniçãoHbTexto.Visibility = Visibility.Hidden;

                MessageBox.Show("Para o vertedor escolhido, preencha o valor da largura da soleira em METROS.");
            }

            if (VertedorEscolhido.Text == "Triangular 90°")
            {
                DefiniçãoL.Visibility = Visibility.Hidden;
                DefiniçãoLTexto.Visibility = Visibility.Hidden;
                DefiniçãoHb.Visibility = Visibility.Hidden;
                DefiniçãoHbTexto.Visibility = Visibility.Hidden;
            }

            if (VertedorEscolhido.Text == "Triangular 120°")
            {
                DefiniçãoL.Visibility = Visibility.Hidden;
                DefiniçãoLTexto.Visibility = Visibility.Hidden;
                DefiniçãoHb.Visibility = Visibility.Hidden;
                DefiniçãoHbTexto.Visibility = Visibility.Hidden;
            }


            if (VertedorEscolhido.Text == "Triangular 135°")
            {
                DefiniçãoL.Visibility = Visibility.Hidden;
                DefiniçãoLTexto.Visibility = Visibility.Hidden;
                DefiniçãoHb.Visibility = Visibility.Hidden;
                DefiniçãoHbTexto.Visibility = Visibility.Hidden;
            }


            if (VertedorEscolhido.Text == "Triangular 90° truncado")
            {
                DefiniçãoL.Visibility = Visibility.Hidden;
                DefiniçãoLTexto.Visibility = Visibility.Hidden;
                DefiniçãoHb.Visibility = Visibility.Visible;
                DefiniçãoHbTexto.Visibility = Visibility.Visible;
            }

            if (VertedorEscolhido.Text == "Triangular 120° truncado")
            {
                DefiniçãoL.Visibility = Visibility.Hidden;
                DefiniçãoLTexto.Visibility = Visibility.Hidden;
                DefiniçãoHb.Visibility = Visibility.Visible;
                DefiniçãoHbTexto.Visibility = Visibility.Visible;
                MessageBox.Show("Para o vertedor escolhido, onde H > Hb, preencha o valor da altura Hb em METROS.");
            }

            if (VertedorEscolhido.Text == "Triangular trapezoidal")
            {
                DefiniçãoL.Visibility = Visibility.Hidden;
                DefiniçãoLTexto.Visibility = Visibility.Hidden;
                DefiniçãoHb.Visibility = Visibility.Visible;
                DefiniçãoHbTexto.Visibility = Visibility.Visible;
                MessageBox.Show("Para o vertedor escolhido, onde H > Hb, preencha o valor da altura Hb em METROS.");
            }

            if (VertedorEscolhido.Text == "Triangular trapezoidal (Tipo Cipoletti)")
            {
                DefiniçãoHb.Visibility = Visibility.Hidden;
                DefiniçãoHbTexto.Visibility = Visibility.Hidden;
                DefiniçãoL.Visibility = Visibility.Visible;
                DefiniçãoLTexto.Visibility = Visibility.Visible;
                MessageBox.Show("Para o vertedor escolhido, preencha o valor da largura da soleira em METROS.");
            }

            if (VertedorEscolhido.Text == "Triangular trapezoidal de 30°")
            {
                DefiniçãoHb.Visibility = Visibility.Hidden;
                DefiniçãoHbTexto.Visibility = Visibility.Hidden;
                DefiniçãoL.Visibility = Visibility.Visible;
                DefiniçãoLTexto.Visibility = Visibility.Visible;
                MessageBox.Show("Para o vertedor escolhido, preencha o valor da largura da soleira em METROS.");
            }

        }

    private void ConfVertedor_Click(object sender, RoutedEventArgs e)
        {
            Conexao con = new Conexao();

            try
            {
                con.conectar();

                if (Convert.ToInt16(RecebeVazao.Text) > 0)
                {

                    double VazaoLida = Double.Parse(RecebeVazao.Text)/1000;


                    if (VertedorEscolhido.Text == "Triangular 90°")
                    {
                        //double radicando = Math.Pow(VazaoLida / 1.420, 25);
                        var H1 = Math.Pow(VazaoLida / 1.420, 0.4);

                        var centrimetros = H1 * 100;

                        Teste1.Text = Convert.ToString(centrimetros);
                        var window1 = new Window1(Teste1.Text);
                    }

                    if (VertedorEscolhido.Text == "Triangular 120°")
                    {
                        var H2 = Math.Pow(VazaoLida / 2.302, 0.408329930584);

                        var centrimetros = H2 * 100;

                        Teste1.Text = Convert.ToString(centrimetros);
                        var window1 = new Window1(Teste1.Text);

                    }

                    if (VertedorEscolhido.Text == "Triangular 135°")
                    {
                        var H3 = Math.Pow(VazaoLida / 3.187, 0.404858299595);
                        var centrimetros = H3 * 100;

                        Teste1.Text = Convert.ToString(centrimetros);
                        var window1 = new Window1(Teste1.Text);

                    }

                    if (VertedorEscolhido.Text == "Triangular 90° truncado")
                    {
                        double RecebeHb1 = double.Parse(DefiniçãoHb.Text);

                        var H4 = (VazaoLida + (1.32 * Math.Pow(RecebeHb1, 2.47))) / (RecebeHb1 * 3.260);
                       // var H4 = 1 - (VazaoLida / (1.32 * Math.Pow(RecebeHb1, 2.47))) - 1;
                        var centrimetros = H4 * 100;

                        Teste1.Text = Convert.ToString(centrimetros);
                        var window1 = new Window1(Teste1.Text);

                    }

                    if (VertedorEscolhido.Text == "Triangular 120° truncado")
                    {
                        double RecebeHb2 = double.Parse(DefiniçãoHb.Text);

                        var H5= (VazaoLida + (2.302 * Math.Pow(RecebeHb2, 2.449))) / (RecebeHb2 * 5.6375);
                        //var H5 = (VazaoLida / (2.302 * Math.Pow(RecebeHb2, 2.449))) - 1;
                        var centrimetros = H5 * 100;

                        Teste1.Text = Convert.ToString(centrimetros);
                        var window1 = new Window1(Teste1.Text);

                    }

                    if (VertedorEscolhido.Text == "Triangular trapezoidal (Tipo Cipoletti)")
                    {
                        double RecebeL1 = double.Parse(DefiniçãoL.Text);

                        var H8 = Math.Sqrt(Math.Pow((VazaoLida / (1.86 * RecebeL1)), 3));
                        var centrimetros = H8 * 100;

                        Teste1.Text = Convert.ToString(centrimetros);
                        var window1 = new Window1(Teste1.Text);

                    }


                    if (VertedorEscolhido.Text == null)
                    {
                        var H7 = Math.Pow(VazaoLida / 1.420, 0.4);
                        var centrimetros = H7 * 100;

                        Teste1.Text = Convert.ToString(centrimetros);
                        var window1 = new Window1(Teste1.Text);

                    }

                    String sqlite = "INSERT INTO AtualizacaoVariaveis(Vertedor, Hb, L, CargaHidraulica) " +
                                "VALUES ('" + VertedorEscolhido.Text + "', '" + DefiniçãoHb.Text + "', '" + DefiniçãoL.Text + "', '" + Teste1.Text + "' )";
                    SQLiteCommand comando = new SQLiteCommand(sqlite, con.conn2);
                    _ = comando.ExecuteNonQuery();
                }


                else
                {
                    double VazaoLida = Double.Parse(RecebeVazao.Text);


                    if (VertedorEscolhido.Text == "Triangular 90°")
                    {

                        Teste1.Text = "Sem Leitura";
                        var window1 = new Window1(Teste1.Text);
                    }

                    if (VertedorEscolhido.Text == "Triangular 120°")
                    {
                        Teste1.Text = "Sem Leitura";
                        var window1 = new Window1(Teste1.Text);
                    }

                    if (VertedorEscolhido.Text == "Triangular 135°")
                    {
                        Teste1.Text = "Sem Leitura";
                        var window1 = new Window1(Teste1.Text);
                    }

                    if (VertedorEscolhido.Text == "Triangular 90° truncado")
                    {
                        Teste1.Text = "Sem Leitura";
                        var window1 = new Window1(Teste1.Text);
                    }

                    if (VertedorEscolhido.Text == "Triangular 120° truncado")
                    {
                        Teste1.Text = "Sem Leitura";
                        var window1 = new Window1(Teste1.Text);
                    }

                    if (VertedorEscolhido.Text == "Triangular trapezoidal (Tipo Cipoletti)")
                    {
                        Teste1.Text = "Sem Leitura";
                        var window1 = new Window1(Teste1.Text);
                    }

                    if (VertedorEscolhido.SelectedText == null)
                    {
                        var H1 = Math.Pow(VazaoLida / 1.420, 0.4);

                        Teste1.Text = Convert.ToString(H1);
                        var window1 = new Window1(Teste1.Text);

                    }


                    //con.conectar();
                    String sqlite = "INSERT INTO AtualizacaoVariaveis(Vertedor, Vazao, CargaHidraulica) " +
                                "VALUES ('" + VertedorEscolhido.Text + "', '" + RecebeVazao.Text + "', '" + Teste1.Text + "')";
                    SQLiteCommand comando = new SQLiteCommand(sqlite, con.conn2);
                    _ = comando.ExecuteNonQuery();
                }
            }
            catch (Exception E)
            {
                MessageBox.Show(E.Message.ToString(), "Erro", MessageBoxButton.OK, MessageBoxImage.Error);

            }

                con.desconectar();
            
        }

        #endregion Escolha Vertedor

    }
}
