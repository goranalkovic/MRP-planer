using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MRP_planer
{
    public sealed partial class ItemBuilder
    {
        // Liste za dodavanje objekata (bruto potrebe i planirani inputi)
        public ObservableCollection<GrossNeedItem> Grsnds = new ObservableCollection<GrossNeedItem>();
        public ObservableCollection<PlannedInputItem> PlndIns = new ObservableCollection<PlannedInputItem>();

        // Glavni objekt
        public MrpItem ObjectItems = new MrpItem();
        public ObservableCollection<MrpItem> ListViewItems = new ObservableCollection<MrpItem>();

        // Undo helperi
        public MrpItem UndoHelper;
        public int UndoItemCounter;
        public static CancellationTokenSource WaitCancelSrc = new CancellationTokenSource();
        public static CancellationToken WaitCancel = WaitCancelSrc.Token;

        // Init helper
        public bool IsInitialised = false;

        // Page init
        public ItemBuilder()
        {
            InitializeComponent();

            Grossneedslist.ItemsSource = Grsnds;
            PlannedInputList.ItemsSource = PlndIns;

            if (IsInitialised) return;

            ObjectItems = App.GlobalItem.Clone();

            InitListView(ObjectItems);

            LstObjectTree.ItemsSource = ListViewItems;

            LstObjectTree.SelectedIndex = 0;
        }

        // Save changes when switching from page
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            App.GlobalItem = ObjectItems.Clone();
        }

        // Utility functions
        private void InitListView(MrpItem item)
        {
            ListViewItems.Clear();

            FillListView(item);
        }

        private void FillListView(MrpItem l)
        {
            ListViewItems.Add(l);
            if (l.ItemChildren == null || l.ItemChildren.Count <= 0)
                return;

            foreach (var subitem in l.ItemChildren)
            {
                FillListView(subitem);
            }
        }

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

        private void DeleteItem(MrpItem l)
        {
            UndoHelper = ObjectItems.Clone();

            var par = FindParent(ObjectItems, l);

            if (par.ItemChildren.Count > 1)
            {
                par.ItemChildren.Remove(l);
                return;
            }

            if (par.ItemChildren.Count == 1)
                par.ItemChildren.Clear();
        }

        private void AddItem(MrpItem l, MrpItem parent)
        {
            if (parent == ObjectItems)
            {
                parent.ItemChildren.Add(l);
                return;
            }

            var par = FindParent(ObjectItems, parent);

            if (par.ItemChildren != null)
                foreach (var s in par.ItemChildren)
                {
                    if (s == parent)
                    {
                        s.ItemChildren.Add(l);
                    }
                }
        }

        // Right pane populating
        private void LstObjectTree_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnDeleteCurrent.IsEnabled = ListViewItems.Count != 1;

            var selected = LstObjectTree.SelectedItem as MrpItem;

            if (selected == ObjectItems)
                BtnDeleteCurrent.IsEnabled = false;

            if (selected != null)
            {
                ItemSelectedIcon.Text = selected.ItemIcon;
                ItemSelectedName.Text = $"{selected.Name}";
                var qtyWord = selected.Quantity.ToString().EndsWith("1") ? "komad" : "komada";
                //ItemSelectedQty.Text = $"{selected.Quantity} {qtyWord} p: {FindParent(ObjectItems, selected).Name}";  // For debug
                ItemSelectedQty.Text = $"{selected.Quantity} {qtyWord}";
                ItemSelectedAcqTime.Text = $"{selected.AcquireDays} dan(a)";
                ItemSelectedAvailable.Text = $"{selected.AvailableInStorage}";

                ItemSelectedLotSize.Text = selected.LotSize != "Lot for lot (L4L)"
                    ? selected.LotSize + selected.LotSizeNum
                    : selected.LotSize;

                ItemSelectedPlannedInput.Text = "";
                ItemSelectedGrossReq.Text = "";

                if (selected.GrossNeeds.Count > 0)
                    foreach (var i in selected.GrossNeeds)
                        ItemSelectedGrossReq.Text += $"{i.Textual} {Environment.NewLine}";
                else
                    ItemSelectedGrossReq.Text = "-";

                if (selected.PlannedInput.Count > 0)
                    foreach (var j in selected.PlannedInput)
                        ItemSelectedPlannedInput.Text += $"{j.Textual} {Environment.NewLine}";
                else
                    ItemSelectedPlannedInput.Text = "-";

            }

        }

        private static string OnlyNums(string input)
        {
            return string.Join("", input.Where(char.IsDigit));
        }


        // Event handlers
        private void BtnDeleteCurrent_Click(object sender, RoutedEventArgs e)
        {
            UndoItemCounter = 0;

            // ReSharper disable once UnusedVariable
            foreach (var t in ObjectItems.ItemChildren)
                UndoItemCounter++;

            DeleteItem(LstObjectTree.SelectedItem as MrpItem);

            InitListView(ObjectItems);

            LstObjectTree.SelectedIndex = 0;

            BtnDeleteCurrent.IsEnabled = false;

            string itemsWord;
            string numItemsWord;

            if ((LstObjectTree.SelectedItem as MrpItem).Level == 1)
                UndoItemCounter--;

            if (UndoItemCounter.ToString().EndsWith("1"))
            {
                itemsWord = "stavka";
                numItemsWord = "Obrisana";
            }
            else if (UndoItemCounter.ToString().EndsWith("2") || UndoItemCounter.ToString().EndsWith("3"))
            {
                itemsWord = "stavke";
                numItemsWord = "Obrisane";
            }
            else
            {
                itemsWord = "stavki";
                numItemsWord = "Obrisano";
            }

            LblUndoMsg.Text = $"{numItemsWord} {UndoItemCounter} {itemsWord}";

            DeletedBanner.Y = 0;

            var waitTaskFactory = new TaskFactory();
         
            waitTaskFactory.StartNew(async () =>
            {
                while (!WaitCancel.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        DeletedBanner.Y = 60;
                    });
                }
            }, WaitCancel);
        }

        private async void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            await CdAddNewItem.ShowAsync();

        }

        private void RadioLotSize1_Checked(object sender, RoutedEventArgs e)
        {
            TxtRadio2.IsEnabled = false;
            TxtRadio3.IsEnabled = false;
            TxtRadio2.Text = "";
            TxtRadio3.Text = "";

            CmdBarGrossNeeds.IsEnabled = true;
            CmdBarPlannedInputs.IsEnabled = true;
            TxtGrsNdAmount.IsEnabled = true;
            TxtGrsNdDay.IsEnabled = true;
            TxtPlnInAmount.IsEnabled = true;
            TxtPlnInDay.IsEnabled = true;
            CdAddNewItem.IsPrimaryButtonEnabled = true;
        }

        private void RadioLotSize2_Checked(object sender, RoutedEventArgs e)
        {
            TxtRadio2.IsEnabled = true;
            TxtRadio3.IsEnabled = false;
            TxtRadio3.Text = "";

            CmdBarGrossNeeds.IsEnabled = true;
            CmdBarPlannedInputs.IsEnabled = true;
            TxtGrsNdAmount.IsEnabled = true;
            TxtGrsNdDay.IsEnabled = true;
            TxtPlnInAmount.IsEnabled = true;
            TxtPlnInDay.IsEnabled = true;
            CdAddNewItem.IsPrimaryButtonEnabled = true;
        }

        private void RadioLotSize3_Checked(object sender, RoutedEventArgs e)
        {
            TxtRadio2.IsEnabled = false;
            TxtRadio3.IsEnabled = true;
            TxtRadio2.Text = "";

            CmdBarGrossNeeds.IsEnabled = true;
            CmdBarPlannedInputs.IsEnabled = true;
            TxtGrsNdAmount.IsEnabled = true;
            TxtGrsNdDay.IsEnabled = true;
            TxtPlnInAmount.IsEnabled = true;
            TxtPlnInDay.IsEnabled = true;
            CdAddNewItem.IsPrimaryButtonEnabled = true;
        }

        private void MainPartDays_LostFocus(object sender, RoutedEventArgs e)
        {
            MainPartDays.Text = OnlyNums(MainPartDays.Text);
        }

        private void MainPartAvailableQty_LostFocus(object sender, RoutedEventArgs e)
        {
            MainPartAvailableQty.Text = OnlyNums(MainPartAvailableQty.Text);
        }

        // Add new item
        private void CdAddNewItem_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var mrpItem = LstObjectTree.SelectedItem as MrpItem;
            if (mrpItem != null)
            {
                var crntLvl = mrpItem.Level;
        
                var sel = FindParent((MrpItem)LstObjectTree.SelectedItem, (MrpItem)LstObjectTree.SelectedItem);
            
                var icon = MainPartIcon.SelectedItem as ComboBoxItem;

                var mainItem = new MrpItem
                {
                    Name = MainPartName.Text,
                    AcquireDays = int.Parse(MainPartDays.Text),
                    AvailableInStorage = int.Parse(MainPartAvailableQty.Text),
                    PlannedInput = PlndIns.ToList(),
                    ItemIcon = icon.Content.ToString(),
                    GrossNeeds = Grsnds.ToList(),
                    ItemChildren = new List<MrpItem>(),
                    Quantity = int.Parse(MainPartQuantity.Text),
                    Level = crntLvl + 1
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

                var ind = sel.IndentSize;

                ind.Left += 30;

                mainItem.IndentSize = ind;

                AddItem(mainItem, sel);
            }

            InitListView(ObjectItems);
        }

        private void MainPartQuantity_LostFocus(object sender, RoutedEventArgs e)
        {
            MainPartQuantity.Text = OnlyNums(MainPartQuantity.Text);
        }

        private void BtnNewPlannedInput_OnClick(object sender, RoutedEventArgs e)
        {
            TxtPlnInAmount.Text = OnlyNums(TxtPlnInAmount.Text);
            TxtPlnInDay.Text = OnlyNums(TxtPlnInDay.Text);

            var dys = int.Parse(TxtPlnInDay.Text);
            var amnt = int.Parse(TxtPlnInAmount.Text);

            var qtyWord = amnt.ToString().EndsWith("1") ? "komad" : "komada";

            var plndin = new PlannedInputItem()
            {
                Days = dys,
                Quantity = amnt,
                Textual = $"{amnt} {qtyWord} {dys}. dana"
            };

            PlndIns.Add(plndin);
            PlannedInputList.ItemsSource = PlndIns;
            BtnRemovePlanedInput.IsEnabled = true;
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

        private void BtnNewGrossNeed_Click(object sender, RoutedEventArgs e)
        {
            TxtGrsNdAmount.Text = OnlyNums(TxtGrsNdAmount.Text);
            TxtGrsNdDay.Text = OnlyNums(TxtGrsNdDay.Text);

            var dys = int.Parse(TxtGrsNdDay.Text);
            var amnt = int.Parse(TxtGrsNdAmount.Text);

            var qtyWord = amnt.ToString().EndsWith("1") ? "komad" : "komada";

            var grossneed = new GrossNeedItem()
            {
                Days = dys,
                Quantity = amnt,
                Textual = $"{amnt} {qtyWord} potrebno {dys}. dana"
            };

            Grsnds.Add(grossneed);
            BtnRemoveGrossNeed.IsEnabled = true;
            TxtGrsNdAmount.Text = "";
            TxtGrsNdDay.Text = "";
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

        private void MainPartName_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainPartDays.IsEnabled = MainPartName.Text.Length > 0;

            if (MainPartDays.IsEnabled) return;
            MainPartAvailableQty.IsEnabled = false;
            MainPartQuantity.IsEnabled = false;
            RadioLotSize1.IsChecked = false;
            RadioLotSize2.IsChecked = false;
            RadioLotSize3.IsChecked = false;
            CmdBarGrossNeeds.IsEnabled = false;
            CmdBarPlannedInputs.IsEnabled = false;
            TxtGrsNdAmount.IsEnabled = false;
            TxtGrsNdDay.IsEnabled = false;
            TxtPlnInAmount.IsEnabled = false;
            TxtPlnInDay.IsEnabled = false;
            CdAddNewItem.IsPrimaryButtonEnabled = false;
        }

        private void MainPartQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            RadioLotSize1.IsEnabled = OnlyNums(MainPartQuantity.Text).Length > 0;
            RadioLotSize2.IsEnabled = OnlyNums(MainPartQuantity.Text).Length > 0;
            RadioLotSize3.IsEnabled = OnlyNums(MainPartQuantity.Text).Length > 0;

            if (OnlyNums(MainPartQuantity.Text).Length > 0) return;
            RadioLotSize1.IsChecked = false;
            RadioLotSize2.IsChecked = false;
            RadioLotSize3.IsChecked = false;
            CmdBarGrossNeeds.IsEnabled = false;
            CmdBarPlannedInputs.IsEnabled = false;
            TxtGrsNdAmount.IsEnabled = false;
            TxtGrsNdDay.IsEnabled = false;
            TxtPlnInAmount.IsEnabled = false;
            TxtPlnInDay.IsEnabled = false;
            CdAddNewItem.IsPrimaryButtonEnabled = false;
        }

        private void MainPartAvailableQty_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainPartQuantity.IsEnabled = OnlyNums(MainPartAvailableQty.Text).Length > 0;
        }

        private void MainPartDays_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainPartAvailableQty.IsEnabled = OnlyNums(MainPartDays.Text).Length > 0;

            if (MainPartAvailableQty.IsEnabled) return;
            MainPartQuantity.IsEnabled = false;
            RadioLotSize1.IsChecked = false;
            RadioLotSize2.IsChecked = false;
            RadioLotSize3.IsChecked = false;
            CmdBarGrossNeeds.IsEnabled = false;
            CmdBarPlannedInputs.IsEnabled = false;
            TxtGrsNdAmount.IsEnabled = false;
            TxtGrsNdDay.IsEnabled = false;
            TxtPlnInAmount.IsEnabled = false;
            TxtPlnInDay.IsEnabled = false;
            CdAddNewItem.IsPrimaryButtonEnabled = false;
        }

        private void UndoDelete(object sender, RoutedEventArgs e)
        {
            WaitCancelSrc.Cancel(true);

            ObjectItems = UndoHelper.Clone();

            InitListView(ObjectItems);

            //LstObjectTree.SelectedIndex = 0;

            BtnDeleteCurrent.IsEnabled = false;

            DeletedBanner.Y = 60;

            WaitCancelSrc.Dispose();
        }

        private void CdAddNewItem_OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            MainPartIcon.SelectedIndex = 0;
            MainPartName.Text = "";
            MainPartDays.Text = "";
            MainPartAvailableQty.Text = "";
            RadioLotSize2.IsChecked = false;
            RadioLotSize1.IsChecked = false;
            RadioLotSize3.IsChecked = false;
            TxtRadio2.Text = "";
            TxtRadio3.Text = "";
            MainPartQuantity.Text = "";

            Grsnds.Clear();
            PlndIns.Clear();
        }
    }
}