using OxyPlot;
using OxyPlot.Axes;
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
    /// Lógica interna para Teste.xaml
    /// </summary>
    public partial class Teste : Window
    {
        public Teste()
        {
            InitializeComponent();

            var model = new PlotModel { Title = "LinearAxis" };
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = -20, Maximum = 80 });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = -10, Maximum = 10 });
        }
    }
}


//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
//using System.Windows.Threading;
//using LiveCharts;
//using LiveCharts.Defaults;
//using LiveCharts.Helpers;
//using LiveCharts.Wpf;

//namespace ProjetoCanaldeHidraulica
//{
//    / <summary>
//    / Interaction logic for MaterialCards.xaml
//    / </summary>
//    public partial class Teste : Window //UserControl, INotifyPropertyChanged
//    {
//        private double _lastLecture;
//        private double _trend;

//        public Teste()
//        {
//            InitializeComponent();

//        }

//        public SeriesCollection LastHourSeries { get; set; }

//        public double LastLecture
//        {
//            get { return _lastLecture; }
//            set
//            {
//                _lastLecture = value;
//                OnPropertyChanged("LastLecture");
//            }
//        }

//        private async void SetLecture()
//        {
//            var target = ((ChartValues<ObservableValue>)LastHourSeries[0].Values).Last().Value;
//            var step = (target - _lastLecture) / 4;

//            await Task.Run(async () =>
//            {
//                while (true)
//                {

//                    this.Dispatcher.Invoke(() =>
//                    {
//                        for (var i = 0; i < 4; i++)
//                        {
//                            Thread.Sleep(100);
//                            LastLecture += step;
//                        }
//                        LastLecture = target;
//                    });


//                    await Task.Delay(100);
//                }
//            });

//        }

//        public event PropertyChangedEventHandler PropertyChanged;

//        protected virtual void OnPropertyChanged(string propertyName = null)
//        {
//            var handler = PropertyChanged;
//            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
//        }

//        private void UpdateOnclick(object sender, RoutedEventArgs e)
//        {
//            TimePowerChart.Update(true);
//        }

//        private async void Button_Click(object sender, RoutedEventArgs e)
//        {
//            LastHourSeries = new SeriesCollection
//            {
//                new LineSeries
//                {
//                    AreaLimit = -10,
//                    Values = new ChartValues<ObservableValue>
//                    {
//                        new ObservableValue(3),
//                        new ObservableValue(5),
//                        new ObservableValue(6),
//                        new ObservableValue(7),
//                        new ObservableValue(3),
//                        new ObservableValue(4),
//                        new ObservableValue(2),
//                        new ObservableValue(5),
//                        new ObservableValue(8),
//                        new ObservableValue(3),
//                        new ObservableValue(5),
//                        new ObservableValue(6),
//                        new ObservableValue(7),
//                        new ObservableValue(3),
//                        new ObservableValue(4),
//                        new ObservableValue(2),
//                        new ObservableValue(5),
//                        new ObservableValue(8)
//                    }
//                }
//            };
//            _trend = 8;


//            await Task.Run(async () =>
//           {
//               var r = new Random();
//               while (true)
//               {

//                   Thread.Sleep(500);
//                   _trend += (r.NextDouble() > 0.3 ? 1 : -1) * r.Next(0, 5);
//                   this.Dispatcher.Invoke(() =>
//                   {
//                       LastHourSeries[0].Values.Add(new ObservableValue(_trend));
//                       LastHourSeries[0].Values.RemoveAt(0);
//                       SetLecture();
//                   });
//               }

//           });


//            DataContext = this;
//        }
//    }
//}
