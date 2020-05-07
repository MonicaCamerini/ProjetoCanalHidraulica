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

namespace ProjetoCanaldeHidraulica
{
    /// <summary>
    /// Lógica interna para Ajuda.xaml
    /// </summary>
    public partial class Ajuda : Window
    {
        private ScrollViewer myScrollViewer;

        public Ajuda()
        {
            InitializeComponent();


            // Define a ScrollViewer
            myScrollViewer = new ScrollViewer();
            myScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;


        }


    }
}
