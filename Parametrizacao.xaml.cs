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
using LiveCharts;
using LiveCharts.Wpf;
using System.Net;
using System.IO;
using System.Windows.Threading;
using LiveCharts.Defaults;
using LiveCharts.Configurations;
using System.ComponentModel;

namespace ProjetoCanaldeHidraulica
{
    /// <summary>
    /// Lógica interna para Parametrizacao.xaml
    /// </summary>
    public partial class Parametrizacao : Window
    {

        public Parametrizacao()
        {
            InitializeComponent();
            Ler_texto_salvo();
            
        }

        private void Salvar_Click(object sender, RoutedEventArgs e)
        {
            Salva_texto();

        }


        private void Salva_texto()
        {
            StreamWriter fluxoTexto;
            string Arquivo = @"C:\Relatorio\" + "ParametrizacaoInversordeFrequencia.txt";


            if (Arquivo == null)
            {
                MessageBox.Show("Arquivo Invalido", "Salvar Como", MessageBoxButton.OK, MessageBoxImage.Question);
            }
            else
            {
                fluxoTexto = new StreamWriter(Arquivo);
                fluxoTexto.Write(EscritaParametrizacao.Text);
                fluxoTexto.Close();
            }
        }


        private void Ler_texto_salvo()
        {
            StreamReader fluxoTexto;
            string linhaTexto;

            if (File.Exists("EscritaParametrizacao.txt"))
            {
                fluxoTexto = new StreamReader("EscritaParametrizacao.txt");
                linhaTexto = fluxoTexto.ReadLine();
                String Str_Leitura = "";


                while (linhaTexto != null)
                {
                    Str_Leitura += linhaTexto + Environment.NewLine;
                    linhaTexto = fluxoTexto.ReadLine();
                }

                fluxoTexto.Close();

                EscritaParametrizacao.Text = Str_Leitura;

            }
        }


        

    }
}
