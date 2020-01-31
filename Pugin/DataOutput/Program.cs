using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataOutput
{
    class Program
    {
       static System.Windows.Application app;
        [STAThread]
        static void Main(string[] args)
        {
            //Application a = new Application();
            //a.StartupUri = new Uri("MainWindow.xaml", System.UriKind.Relative);
            //a.Run();
            app = new System.Windows.Application();
            
            MainWindowClass window = new MainWindowClass();

           app.Run(window);
            //window.Show();
 
        }
    }
}
