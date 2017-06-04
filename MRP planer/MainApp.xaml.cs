using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MRP_planer
{
    public class MenuItem
    {
        public string Name { get; set; }
        public string Icon { get; set; }
    }
   
    public sealed partial class MainApp : Page
    {

        public MainApp()
        {
            InitializeComponent();

            lstMenu.ItemsSource = new List<MenuItem>()
            {
                new MenuItem { Name = "Sastavnica", Icon = "" },
                new MenuItem { Name = "MRP tablice", Icon = "" },
                new MenuItem { Name = "Planirani nalozi", Icon = "" }
            };

            lstMenu.SelectedIndex = 0;
        }

        private void LstMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstMenu.SelectedIndex == 0)
                frmMain.Navigate(typeof(ItemBuilder));

            if (lstMenu.SelectedIndex == 1)
                frmMain.Navigate(typeof(MrpCalc));
        }
    }
}
