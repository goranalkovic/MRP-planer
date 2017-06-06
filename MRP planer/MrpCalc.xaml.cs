using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable SwitchStatementMissingSomeCases

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

            //AddTables(MainItem);
            AddTable(MainItem);
        }

        //public void AddTables(MrpItem item)
        //{
        //    AddTable(item);
        //    if (item.ItemChildren.Count > 0)
        //    {
        //        foreach (var c in item.ItemChildren)
        //            AddTables(c);
        //    }
        //}
        private static MrpItem FindParent(MrpItem l, MrpItem searchItem)
        {
            if (searchItem == l)
                return l;
            if (l.ItemChildren != null)
            {
                return l.ItemChildren.Contains(searchItem) ? l : l.ItemChildren.Select(s => FindParent(s, searchItem)).FirstOrDefault(f => f != null);
            }
            return null;
        }

        public void AddTable(MrpItem item)
        {
            // ##### Kalkulacije

            var colValues = new double[NumberOfColumns + 1, 7];

            for (var i = 0; i <= NumberOfColumns; i++)
            {
                colValues[i, 0] = i + 1;
                colValues[i, 1] = 0;
                colValues[i, 2] = 0;
                colValues[i, 3] = 0;
                colValues[i, 4] = 0;
                colValues[i, 5] = 0;
                colValues[i, 6] = 0;
            }

            // 1. Pripremi podatke

            if (item == MainItem)
            {
                if (item.GrossNeeds.Count > 0)
                    foreach (var gn in item.GrossNeeds)
                        colValues[gn.Days - 1, 1] = gn.Quantity;
            }
            else
            {
                var parentItem = FindParent(MainItem, item);

                if (parentItem.PlannedOuts.Count > 0)
                    foreach (var t in parentItem.PlannedOuts)
                    {
                        colValues[t.Value, 1] = t.Key * item.Quantity;
                    }
            }

            if (item.PlannedInput.Count > 0)
                    foreach (var pi in item.PlannedInput)
                        colValues[pi.Days - 1, 2] = pi.Quantity;

                for (var i = 0; i < NumberOfColumns; i++)
                {
                    // 2. Raspoloživo

                    // prvi stupac
                    if (i == 0)
                    {
                        colValues[i, 3] = (colValues[i, 2] + item.AvailableInStorage) - colValues[i, 1];
                        continue;
                    }

                    // samo popuna "raspoloživo" iz lijevog

                    if (colValues[i, 1] == 0 && i > 0)
                    {
                        colValues[i, 3] = colValues[i, 2] + colValues[i - 1, 3];
                        continue;
                    }

                    // jednostavna popuna (jednostupčana)
                    if (colValues[i, 1] > 0 && i < item.AcquireDays)
                    {
                        colValues[i, 3] = (colValues[i, 2] + colValues[i - 1, 3]) - colValues[i, 1];
                        continue;
                    }

                    // special case
                    if (i > 0 && colValues[i, 1] > 0 && colValues[i - 1, 3] > 0 &&
                        colValues[i - 1, 3] > colValues[i, 1])
                    {
                        colValues[i, 3] = Math.Abs(colValues[i - 1, 3] - colValues[i, 1]);
                        continue;
                    }

                    // else (s pomicanjem ulijevo)
                    if (colValues[i, 1] > 0 && i >= item.AcquireDays)
                    {
                        // 3. Neto potrebe
                        colValues[i, 4] = Math.Abs(colValues[i, 1] - colValues[i - 1, 3]);

                        // 4. Lot size & planirano izdavanje
                        switch (item.LotSize)
                        {
                            case "Lot for lot (L4L)":
                                colValues[i - item.AcquireDays, 6] = colValues[i, 4];
                                break;
                            case "Mult ":
                                colValues[i - item.AcquireDays, 6] =
                                    Math.Ceiling(colValues[i, 4] / item.LotSizeNum) * item.LotSizeNum;
                                break;
                            case "Minimalno ":
                                colValues[i - item.AcquireDays, 6] = colValues[i, 4] <= item.LotSizeNum
                                    ? item.LotSizeNum
                                    : colValues[i, 4];
                                break;
                        }

                        // 5. Planirani primitak
                        colValues[i, 5] = colValues[i - item.AcquireDays, 6];

                        // 6. Preostali resursi
                        colValues[i, 3] = (colValues[i - 1, 3] - colValues[i, 4] > 0)
                            ? colValues[i - 1, 3] - colValues[i, 4]
                            : 0;
                    }

                }




                // 7. spremi lokacije planiranih izdavanja
                for (var i = 0; i < NumberOfColumns; i++)
                {
                    if (colValues[i, 6] > 0)
                        item.PlannedOuts.Add(new KeyValuePair<double, int>(colValues[i, 6], i));
                }

                // ### Glavni grid

                var grdWidth = 190 + 48 + (NumberOfColumns * 100);

                var grd = new Grid()
                {
                    Width = grdWidth,
                    Height = 300,
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(14, 40, 40, 40)),
                    Background = new SolidColorBrush(Color.FromArgb(140, 240, 240, 240)),
                    Margin = new Thickness(0, 16, 0, 16),
                    Padding = new Thickness(24, 16, 24, 6),
                    RowDefinitions =
                    {
                        new RowDefinition() {Height = new GridLength(44)},
                        new RowDefinition() {Height = new GridLength(32)},
                        new RowDefinition() {Height = new GridLength(32)},
                        new RowDefinition() {Height = new GridLength(32)},
                        new RowDefinition() {Height = new GridLength(32)},
                        new RowDefinition() {Height = new GridLength(32)},
                        new RowDefinition() {Height = new GridLength(32)},
                        new RowDefinition() {Height = new GridLength(32)}
                    }
                };

                grd.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(190)
                });

                for (var i = 1; i <= NumberOfColumns; i++)
                    grd.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                        Width = new GridLength(100)
                    });

                // ### Prvi stupac

                var l = new Line()
                {
                    StrokeThickness = 1.25,
                    Stroke = new SolidColorBrush(Colors.Black),
                    Opacity = 0.3,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    X2 = 20000,
                    Margin = new Thickness(0, 2, 0, 0)
                };
                Grid.SetRow(l, 0);
                Grid.SetColumnSpan(l, NumberOfColumns + 1);

                grd.Children.Add(l);

                //

                var r0Relative = new RelativePanel()
                {
                    Height = 48
                };

                var r0I = new TextBlock()
                {
                    FontSize = 24,
                    FontFamily = new FontFamily("Segoe MDL2 Assets"),
                    Text = item.ItemIcon,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)),
                    Margin = new Thickness(0, 0, 0, 8)
                };
                RelativePanel.SetAlignVerticalCenterWithPanel(r0I, true);
                RelativePanel.SetAlignLeftWithPanel(r0I, true);

                r0Relative.Children.Add(r0I);


                var r01 = new TextBlock()
                {
                    Text = item.Name,
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)),
                    Margin = new Thickness(8, 2, 0, 0),
                    FontSize = 16
                };
                RelativePanel.SetAlignTopWithPanel(r01, true);
                RelativePanel.SetRightOf(r01, r0I);

                r0Relative.Children.Add(r01);

                var lotSizeText = item.LotSize == "Lot for lot (L4L)"
                    ? "LOT FOR LOT"
                    : $"{item.LotSize.ToUpper()}{item.LotSizeNum}";
                var qtyWord = item.Quantity.ToString().EndsWith("1") ? "KOMAD" : "KOMADA";

                var r02 = new TextBlock()
                {
                    Text =
                        $"{item.Quantity} {qtyWord}  -  RAZINA  {item.Level}  -  ŠARŽA {lotSizeText}  -  VRIJEME DOBAVE  {item.AcquireDays} DAN/A",
                    Opacity = 0.6,
                    FontSize = 10,
                    Margin = new Thickness(8, 0, 0, 10),
                    CharacterSpacing = 90
                };
                RelativePanel.SetAlignBottomWithPanel(r02, true);
                RelativePanel.SetRightOf(r02, r0I);

                r0Relative.Children.Add(r02);

                Grid.SetRow(r0Relative, 0);
                Grid.SetColumnSpan(r0Relative, NumberOfColumns + 1);

                grd.Children.Add(r0Relative);

                //

                var r1 = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = "PERIOD",
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold
                };
                Grid.SetRow(r1, 1);

                grd.Children.Add(r1);

                //

                var r2 = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = "BRUTO POTREBE",
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold
                };
                Grid.SetRow(r2, 2);

                grd.Children.Add(r2);

                //

                var r3 = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = "PLANIRANI ULAZI",
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold
                };
                Grid.SetRow(r3, 3);

                grd.Children.Add(r3);

                //

                var r4Relative = new RelativePanel();

                Grid.SetRow(r4Relative, 4);

                var r40 = new TextBlock()
                {
                    Text = "RASPOLOŽIVO",
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 12
                };
                RelativePanel.SetAlignLeftWithPanel(r40, true);
                RelativePanel.SetAlignVerticalCenterWithPanel(r40, true);

                r4Relative.Children.Add(r40);

                var r41 = new TextBlock()
                {
                    //Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215)),
                    Text = item.AvailableInStorage > 0 ? item.AvailableInStorage.ToString() : "",
                    FontSize = 14,
                    Margin = new Thickness(8, 0, 0, 0)
                };

                RelativePanel.SetAlignRightWithPanel(r41, true);
                RelativePanel.SetAlignVerticalCenterWithPanel(r41, true);

                r4Relative.Children.Add(r41);

                grd.Children.Add(r4Relative);

                //

                var r5 = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = "NETO POTREBE",
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold
                };
                Grid.SetRow(r5, 5);

                grd.Children.Add(r5);

                //

                var r6 = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = "PLANIRANI PRIMITAK NALOGA",
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold
                };
                Grid.SetRow(r6, 6);

                grd.Children.Add(r6);

                //

                var r7 = new TextBlock()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Text = "PLANIRANO IZDAVANJE NALOGA",
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold
                };
                Grid.SetRow(r7, 7);

                grd.Children.Add(r7);

                // ### Ostali stupci

                for (var i = 0; i < NumberOfColumns; i++)
                {
                    for (var j = 0; j < 7; j++)
                    {
                        var row = new TextBlock()
                        {
                            Width = 100,
                            TextAlignment = TextAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontSize = 14
                        };
                        Grid.SetRow(row, j + 1);
                        Grid.SetColumn(row, i + 1);

                        row.Text = $"{colValues[i, j]}";

                        if (j == 0)
                            row.FontSize = 12;
                        if (j == 0)
                            row.FontWeight = FontWeights.SemiBold;

                        grd.Children.Add(row);
                    }
                }

                Tables.Add(grd);

                if (item.ItemChildren.Count > 0)
                {
                    foreach (var ic in item.ItemChildren)
                        AddTable(ic);
                }
            }

        }
}