using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace MRP_planer
{
    public class MrpItem
    {
        public string Name { get; set; }

        public int AcquireDays { get; set; }

        public string LotSize { get; set; }

        public int LotSizeNum { get; set; }

        public int AvailableInStorage { get; set; }

        public List<PlannedInputItem> PlannedInput { get; set; }

        public List<GrossNeedItem> GrossNeeds { get; set; }

        public Thickness IndentSize { get; set; }

        public string ItemIcon { get; set; }
        
        public List<MrpItem> ItemChildren { get; set; }

        public int Quantity { get; set; }

        public int Level { get; set; }
    }

    //public class MainMrpItem
    //{
    //    public MrpItem GlobalObjectItems;

    //    public void Update(MrpItem input)
    //    {
    //        GlobalObjectItems = input;
    //    }

    //    public MrpItem GetItem()
    //    {
    //        return GlobalObjectItems;
    //    }
    //}

    public class GrossNeedItem
    {
        public string Textual { get; set; }

        public int Days { get; set; }

        public int Quantity { get; set; }
    }

    public class PlannedInputItem : GrossNeedItem {}
    public sealed partial class MainPage
    {
        public ObservableCollection<GrossNeedItem> Grsnds = new ObservableCollection<GrossNeedItem>();
        public ObservableCollection<PlannedInputItem> PlndIns = new ObservableCollection<PlannedInputItem>();
        public MrpItem FirstItem;
        private bool AddTestChildren = false;
        public MainPage()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ForegroundColor = Colors.Black;
            titleBar.ButtonForegroundColor = Colors.Black;

            ApplicationView.PreferredLaunchViewSize = new Size(1000, 700);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(1000, 700));

            Window.Current.SizeChanged += delegate
            {
                if (Window.Current.Bounds.Height < 700 || Window.Current.Bounds.Width < 1000)
                {
                    ApplicationView.GetForCurrentView().TryResizeView(new Size(1000, 700));
                }
            };
            
            InitializeComponent();

            Grossneedslist.ItemsSource = Grsnds;
            PlannedInputList.ItemsSource = PlndIns;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
        
            var soundFile = await StorageFile.GetFileFromApplicationUriAsync(
                new Uri("ms-appx:///Assets/error_tone.wav"));

            var inputValid = false;
            
            if (MainPartName.Text.Length < 1)
            {
                ValidationNameGrid.Width = 160;
                ValidationErrText.Text = "Niste unijeli ime";
                ValidationName.ShowAt(MainPartName);
                MainPartName.Focus(FocusState.Programmatic);
            }
            else if (MainPartDays.Text.Length < 1)
            {
                ValidationNameGrid.Width = 260;
                ValidationErrText.Text = "Nije odabran broj dana dobave";
                ValidationName.ShowAt(MainPartDays);
                MainPartDays.Focus(FocusState.Programmatic);
            }
            else if (MainPartAvailableQty.Text.Length < 1)
            {
                ValidationNameGrid.Width = 360;
                ValidationErrText.Text = "Nije postavljena količina raspoloživa u skladištu";
                ValidationName.ShowAt(MainPartAvailableQty);
                MainPartAvailableQty.Focus(FocusState.Programmatic);
            }
            else if (MainPartQuantity.Text.Length < 1)
            {
                ValidationNameGrid.Width = 280;
                ValidationErrText.Text = "Nije postavljena količina proizvoda";
                ValidationName.ShowAt(MainPartQuantity);
                MainPartQuantity.Focus(FocusState.Programmatic);
            }
            else if (RadioLotSize1.IsChecked == false && RadioLotSize2.IsChecked == false && RadioLotSize3.IsChecked == false)
            {
                ValidationNameGrid.Width = 270;
                ValidationErrText.Text = "Nije odabrana vrsta šarže";
                ValidationName.ShowAt(LblLotSize);
            }
            else if (RadioLotSize2.IsChecked == true && TxtRadio2.Text.Length < 1)
            {
                ValidationNameGrid.Width = 270;
                ValidationErrText.Text = "Nije postavljena količina";
                ValidationName.ShowAt(TxtRadio2);
                TxtRadio2.Focus(FocusState.Programmatic);
            }
            else if (RadioLotSize3.IsChecked == true && TxtRadio3.Text.Length < 1)
            {
                ValidationNameGrid.Width = 270;
                ValidationErrText.Text = "Nije postavljena količina";
                ValidationName.ShowAt(TxtRadio3);
                TxtRadio3.Focus(FocusState.Programmatic);
            }
            else
                inputValid = true;

            if (!inputValid)
            {
                await PlayAudioAsync(soundFile);
            }
            else
            {
                var icon = MainPartIcon.SelectedItem as ComboBoxItem;

                var mainItem = new MrpItem
                {
                    Name = MainPartName.Text,
                    AcquireDays = int.Parse(MainPartDays.Text),
                    AvailableInStorage = int.Parse(MainPartAvailableQty.Text),
                    PlannedInput = PlndIns.ToList(),
                    ItemIcon = icon.Content.ToString(),
                    IndentSize = new Thickness(0, 0, 0, 0),
                    GrossNeeds = Grsnds.ToList(),
                    Quantity = int.Parse(MainPartQuantity.Text),
                    Level = 0
                };

                if (RadioLotSize1.IsChecked == true)
                {
                    mainItem.LotSize = "Lot for lot (L4L)";
                    mainItem.LotSizeNum = 1;
                }

                if (RadioLotSize2.IsChecked == true)
                {
                    mainItem.LotSize = "Mult ";
                    mainItem.LotSizeNum = int.Parse(TxtRadio2.Text);
                }

                if (RadioLotSize3.IsChecked == true)
                {
                    mainItem.LotSize = "Minimalno ";
                    mainItem.LotSizeNum = int.Parse(TxtRadio3.Text);
                }

                //validationNameGrid.Width = 600;
                //validationNameGrid.Height = 400;
                //validationErrText.Text =
                //    $"name: {mainItem.Name}, lotsize: {mainItem.LotSize} {mainItem.LotSizeNum}\nacqdays: {mainItem.AcquireDays}, planned: {mainItem.PlannedInput}\navail: {mainItem.AvailableInStorage}";
                //validationName.ShowAt(btnFirstStep);

                if (AddTestChildren)
                {
                    mainItem.ItemChildren = new List<MrpItem>()
                    {
                        new MrpItem()
                        {
                            AcquireDays = 5,
                            AvailableInStorage = 10,
                            GrossNeeds = new List<GrossNeedItem>(),
                            IndentSize = new Thickness(30, 0, 0 ,0),
                            ItemIcon = "",
                            LotSize = "Mult ",
                            LotSizeNum = 100,
                            Name = "Korice",
                            PlannedInput = new List<PlannedInputItem>(){ new PlannedInputItem(){Days = 1, Quantity = 100, Textual = "100 komad(a) 1. dan(a)"}},
                            ItemChildren = new List<MrpItem>(),
                            Quantity = 2,
                            Level = 1
                        },
                        new MrpItem()
                        {
                            AcquireDays = 1,
                            AvailableInStorage = 0,
                            GrossNeeds = new List<GrossNeedItem>(),
                            IndentSize = new Thickness(30, 0, 0, 0),
                            ItemChildren = new List<MrpItem>()
                            {
                                new MrpItem()
                                {
                                    AcquireDays = 2,
                                    AvailableInStorage = 20,
                                    GrossNeeds = new List<GrossNeedItem>(),
                                    IndentSize = new Thickness(60, 0, 0, 0),
                                    ItemIcon = "",
                                    LotSize = "Mult ",
                                    LotSizeNum = 10,
                                    Name = "Listovi 0,15 mm",
                                    PlannedInput = new List<PlannedInputItem>() { new PlannedInputItem() { Days = 1, Quantity = 10, Textual = "10 komad(a) 1. dan(a)" } },
                                    ItemChildren = new List<MrpItem>(),
                                    Quantity = 2,
                                    Level = 2
                                },
                                new MrpItem()
                                {
                                    AcquireDays = 1,
                                    AvailableInStorage = 300,
                                    GrossNeeds = new List<GrossNeedItem>(),
                                    IndentSize = new Thickness(60, 0, 0, 0),
                                    ItemIcon = "",
                                    LotSize = "Mult ",
                                    LotSizeNum = 10,
                                    Name = "Listovi 0,05 mm",
                                    PlannedInput = new List<PlannedInputItem>() { new PlannedInputItem() { Days = 1, Quantity = 10, Textual = "10 komad(a) 1. dan(a)" } },
                                    ItemChildren = new List<MrpItem>(),
                                    Quantity = 200,
                                    Level = 2
                                }
                            },
                            ItemIcon = "",
                            LotSize = "Lot for lot (L4L)",
                            LotSizeNum = 1,
                            Name = "Listovi",
                            PlannedInput = new List<PlannedInputItem>(),
                            Quantity = 1
                        }
                    };
                }
                else
                {
                    mainItem.ItemChildren = new List<MrpItem>();
                }

                App.GlobalItem = mainItem;

                Frame.Navigate(typeof(MainApp));
            }
            
        }
        
        public static async Task PlayAudioAsync(IStorageFile mediaFile, bool looping = false)
        {
            var stream = await mediaFile.OpenAsync(FileAccessMode.Read).AsTask();
            var mediaControl = new MediaElement() { IsLooping = looping };
            mediaControl.SetSource(stream, mediaFile.ContentType);
            mediaControl.Play();
        }

        private void RadioLotSize1_Checked(object sender, RoutedEventArgs e)
        {
            TxtRadio2.IsEnabled = false;
            TxtRadio3.IsEnabled = false;
        }

        private void RadioLotSize2_Checked(object sender, RoutedEventArgs e)
        {
            TxtRadio2.IsEnabled = true;
            TxtRadio3.IsEnabled = false;
        }

        private void RadioLotSize3_Checked(object sender, RoutedEventArgs e)
        {
            TxtRadio2.IsEnabled = false;
            TxtRadio3.IsEnabled = true;
        }
        private static string OnlyNums(string input)
        {
            return string.Join("", input.Where(char.IsDigit));
        }

        private void MainPartDays_LostFocus(object sender, RoutedEventArgs e)
        {
            MainPartDays.Text = OnlyNums(MainPartDays.Text);
        }

        private void MainPartAvailableQty_LostFocus(object sender, RoutedEventArgs e)
        {
            MainPartAvailableQty.Text = OnlyNums(MainPartAvailableQty.Text);
        }


        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            MainPartIcon.SelectedIndex = 27;
            MainPartName.Text = "Knjiga";
            MainPartDays.Text = "2";
            MainPartAvailableQty.Text = "0";
            RadioLotSize1.IsChecked = true;
            MainPartQuantity.Text = "1";

            var tstneed = new GrossNeedItem()
            {
                Days = 6,
                Quantity = 35,
                Textual = "35 komad(a) potrebno 6. dan(a)"
            };

            var tstneed2 = new GrossNeedItem()
            {
                Days = 8,
                Quantity = 25,
                Textual = "25 komad(a) potrebno 8. dan(a)"
            };

            Grsnds.Add(tstneed);
            Grsnds.Add(tstneed2);

            BtnRemoveGrossNeed.IsEnabled = true;

            AddTestChildren = true;
        }

      

        private async void BtnNewGrossNeed_Click(object sender, RoutedEventArgs e)
        {
            TxtGrsNdAmount.Text = "";
            TxtGrsNdDay.Text = "";
            await CdAddGrsNeed.ShowAsync();
        }

        private void BtnRemoveGrossNeed_Click(object sender, RoutedEventArgs e)
        {
            if (Grsnds.Count < 1)
            {
                BtnRemoveGrossNeed.IsEnabled = false;
                return;
            }

            if (Grossneedslist.SelectedIndex == -1)
                return;

            Grsnds.Remove(Grossneedslist.SelectedItem as GrossNeedItem);

            if (Grsnds.Count < 1)
            {
                BtnRemoveGrossNeed.IsEnabled = false;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            TxtGrsNdAmount.Text = OnlyNums(TxtGrsNdAmount.Text);
            TxtGrsNdDay.Text = OnlyNums(TxtGrsNdDay.Text);

            var dys = int.Parse(TxtGrsNdDay.Text);
            var amnt = int.Parse(TxtGrsNdAmount.Text);

            var grossneed = new GrossNeedItem()
            {
                Days = dys,
                Quantity = amnt,
                Textual = $"{amnt} komad(a) potrebno {dys}. dan(a)"
            };

            Grsnds.Add(grossneed);
            BtnRemoveGrossNeed.IsEnabled = true;
        }

        private void MainPartQuantity_LostFocus(object sender, RoutedEventArgs e)
        {
            MainPartQuantity.Text = OnlyNums(MainPartQuantity.Text);
        }

        private async void BtnNewPlannedInput_OnClick(object sender, RoutedEventArgs e)
        {
            TxtPlnInAmount.Text = "";
            TxtPlnInDay.Text = "";
            await CdAddPlannedInput.ShowAsync();
        }

        private void BtnRemovePlanedInput_OnClick(object sender, RoutedEventArgs e)
        {
            if (PlndIns.Count < 1)
            {
                BtnRemovePlanedInput.IsEnabled = false;
                return;
            }

            if (PlannedInputList.SelectedIndex == -1)
                return;

            PlndIns.Remove(PlannedInputList.SelectedItem as PlannedInputItem);

            if (Grsnds.Count < 1)
            {
                BtnRemovePlanedInput.IsEnabled = false;
            }
        }

        private void CdAddPlannedInput_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            TxtPlnInAmount.Text = OnlyNums(TxtPlnInAmount.Text);
            TxtPlnInDay.Text = OnlyNums(TxtPlnInDay.Text);

            var dys = int.Parse(TxtPlnInDay.Text);
            var amnt = int.Parse(TxtPlnInAmount.Text);

            var plndin = new PlannedInputItem()
            {
                Days = dys,
                Quantity = amnt,
                Textual = $"{amnt} komad(a) {dys}. dan(a)"
            };

            PlndIns.Add(plndin);
            BtnRemovePlanedInput.IsEnabled = true;
        }
    }
}