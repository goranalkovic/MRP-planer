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

namespace MRP_planer
{
  public sealed partial class MrpCalc
    {
        public MrpItem MainItem = App.GlobalItem;

        public MrpCalc()
        {
            InitializeComponent();

            DrawFirstTable();
        }

        private void DrawFirstTable()
        {
            var cols = new List<TableColumn>();

            for (var i = 1; i <= MainItem.AcquireDays; i++)
            {
                cols.Add(new TableColumn(i));   
            }

            var lv = new StackPanel();

            

            Cols01.Children.Add(lv);
        }
    }
}
