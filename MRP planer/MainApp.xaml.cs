using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MRP_planer
{
    public class MenuItem
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public Color SelectedMarker { get; set; }
    }
   
    public sealed partial class MainApp : Page
    {
        public ObservableCollection<MenuItem> MenuItems = new ObservableCollection<MenuItem>()
        {
            new MenuItem { Name = "Sastavnica", Icon = "" },
            new MenuItem { Name = "MRP tablice", Icon = "" },
            new MenuItem { Name = "Planirani nalozi", Icon = "" }
        };
        public MainApp()
        {
            InitializeComponent();
            
            LstMenu.ItemsSource = MenuItems;
            LstMenu.SelectedIndex = 0;
        }

        private void LstMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstMenu.SelectedIndex == 0)
                FrmMain.Navigate(typeof(ItemBuilder));

            if (LstMenu.SelectedIndex == 1)
                FrmMain.Navigate(typeof(MrpCalc));

            if (LstMenu.SelectedIndex == 2)
                FrmMain.Navigate(typeof(ReportPlannedItems));

            //for (var i = 0; i < MenuItems.Count; i++)
            //    if (LstMenu.SelectedIndex == i)
            //        MenuItems[i].SelectedMarker = Colors.White;
            //    else
            //        MenuItems[i].SelectedMarker = Colors.Transparent;

            //LstMenu.ItemsSource = MenuItems;
        }
    }
}
