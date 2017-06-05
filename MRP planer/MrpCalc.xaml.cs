using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace MRP_planer
{

    public sealed partial class MrpCalc
    {
        public MrpItem MainItem = App.GlobalItem;

        public ObservableCollection<Grid> Tables = new ObservableCollection<Grid>();

        public int NumberOfColumns;

        public MrpCalc()
        {
            InitializeComponent();

            NumberOfColumns = MainItem.GrossNeeds.Select(t => t.Days).Concat(new[] { 0 }).Max();

            TableItems.ItemsSource = Tables;

            AddTable(MainItem);
        }


        public void AddTable(MrpItem item)
        {
            // ### Glavni grid

            var grd = new Grid()
            {
                Height = 300,
                BorderThickness = new Thickness(1),
                //BorderBrush = new SolidColorBrush(Colors.DimGray),
                Background = new SolidColorBrush(Color.FromArgb(180, 240, 240, 240)),
                Margin = new Thickness(0, 0, 0, 32),
                Padding = new Thickness(8),
                ColumnDefinitions =
                {
                    // prvi stupac
                    new ColumnDefinition()
                    {
                        Width = new GridLength(200)
                    }
                },
                RowDefinitions =
                {
                    new RowDefinition(){Height = new GridLength(56)},
                    new RowDefinition(){Height = new GridLength(32)},
                    new RowDefinition(){Height = new GridLength(32)},
                    new RowDefinition(){Height = new GridLength(32)},
                    new RowDefinition(){Height = new GridLength(32)},
                    new RowDefinition(){Height = new GridLength(32)},
                    new RowDefinition(){Height = new GridLength(32)},
                    new RowDefinition(){Height = new GridLength(32)}
                }
            };

            // ### Prvi stupac

            var l = new Line()
            {
                StrokeThickness = 1.5,
                Stroke = new SolidColorBrush(Colors.Black),
                Opacity = 0.4,
                VerticalAlignment = VerticalAlignment.Bottom,
                X2 = 20000
            };
            Grid.SetColumn(l, 0);
            Grid.SetRow(l, 0);
            Grid.SetColumnSpan(l, 2);

            grd.Children.Add(l);
            //

            var r0Relative = new RelativePanel()
            {
                Width = 182,
                Height = 40
            };

            var r00 = new TextBlock()
            {
                Text = "Proizvod",
                FontWeight = FontWeights.SemiBold
            };
            RelativePanel.SetAlignTopWithPanel(r00, true);
            RelativePanel.SetAlignLeftWithPanel(r00, true);

            r0Relative.Children.Add(r00);

            var r01 = new TextBlock()
            {
                Text = item.Name,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)),
                FontSize = 14
            };
            RelativePanel.SetAlignBottomWithPanel(r01, true);
            RelativePanel.SetAlignLeftWithPanel(r01, true);

            r0Relative.Children.Add(r01);

            var r02 = new TextBlock()
            {
                Text = $"Razina {item.Level}",
                Opacity = 0.6,
                FontSize = 13
            };
            RelativePanel.SetAlignTopWithPanel(r02, true);
            RelativePanel.SetAlignRightWithPanel(r02, true);

            r0Relative.Children.Add(r02);

            Grid.SetColumn(r0Relative, 0);
            Grid.SetRow(r0Relative, 0);

            grd.Children.Add(r0Relative);

            //

            var r1Relative = new RelativePanel();

            Grid.SetRow(r1Relative, 1);

            var r10 = new TextBlock()
            {
                Text = "Šarža",
                FontWeight = FontWeights.SemiBold
            };
            RelativePanel.SetAlignLeftWithPanel(r10, true);
            RelativePanel.SetAlignVerticalCenterWithPanel(r10, true);

            r1Relative.Children.Add(r10);

            var r11 = new TextBlock()
            {
                Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)),
                Text = item.LotSize == "Lot for lot (L4L)" ? "L4L" : $"{item.LotSize}{item.LotSizeNum}"
            };
            
            RelativePanel.SetAlignRightWithPanel(r11, true);
            RelativePanel.SetAlignVerticalCenterWithPanel(r11, true);
            
            r1Relative.Children.Add(r11);

            grd.Children.Add(r1Relative);

            // ### Ostali stupci

            for (var i = 1; i <= NumberOfColumns; i++)
            {
                grd.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(100)
                });
            }

            Tables.Add(grd);
        }

    }
}
