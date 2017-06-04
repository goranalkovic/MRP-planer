using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// ReSharper disable PossibleNullReferenceException

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MRP_planer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ItemBuilder
    {
        public ObservableCollection<GrossNeedItem> Grsnds = new ObservableCollection<GrossNeedItem>();
        public ObservableCollection<PlannedInputItem> PlndIns = new ObservableCollection<PlannedInputItem>();

        public MrpItem FirstItem;
        private List<MrpItem> _undoHelper = new List<MrpItem>();
        private int _undoItemCounter;

        public MrpItem ObjectItems = new MrpItem();
        public ObservableCollection<MrpItem> ListViewItems = new ObservableCollection<MrpItem>();

        public bool IsInitialised = false;

        public ItemBuilder()
        {
            InitializeComponent();

            Grossneedslist.ItemsSource = Grsnds;
            PlannedInputList.ItemsSource = PlndIns;

            if (IsInitialised) return;

            var mainitem = App.GlobalItem;

            ObjectItems = App.GlobalItem;

            InitListView(mainitem);

            LstObjectTree.ItemsSource = ListViewItems;

            LstObjectTree.SelectedIndex = 0;
        }

        private void InitListView(MrpItem item)
        {
            ListViewItems.Clear();

            FillListView(item);
        }

        private void FillListView(MrpItem l)
        {
            ListViewItems.Add(l);
            if (l.ItemChildren != null)
            {
                foreach (var subitem in l.ItemChildren)
                {
                    FillListView(subitem);
                }
            }
        }

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
                ItemSelectedQty.Text = $"{selected.Quantity} komad(a)";
                ItemSelectedAcqTime.Text = $"{selected.AcquireDays} dan(a)";
                ItemSelectedAvailable.Text = $"{selected.AvailableInStorage}";

                if (selected.LotSize != "Lot for lot (L4L)")
                {
                    ItemSelectedLotSize.Text = selected.LotSize + selected.LotSizeNum;
                }
                else
                {
                    ItemSelectedLotSize.Text = selected.LotSize;
                }

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
            var par = FindParent(ObjectItems, l);

            if (par.ItemChildren.Count > 1)
            {
                par.ItemChildren.Remove(l);
                return;
            }


            if (par.ItemChildren.Count == 1)
                par.ItemChildren = null;
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

        private void BtnDeleteCurrent_Click(object sender, RoutedEventArgs e)
        {
            _undoHelper.Clear();
            _undoItemCounter = 1;

            foreach (var t in ObjectItems.ItemChildren)
            {
                _undoHelper.Add(t);
                _undoItemCounter++;
            }

            DeleteItem(LstObjectTree.SelectedItem as MrpItem);

            InitListView(ObjectItems);

            LstObjectTree.SelectedIndex = 0;

            BtnDeleteCurrent.IsEnabled = false;

            var itemsWord = "";

            switch (_undoItemCounter)
            {
                case 1:
                case 21:
                case 31:
                case 41:
                case 51:
                    itemsWord = "stavka";
                    break;
                case 2:
                case 3:
                case 4:
                case 22:
                case 23:
                case 24:
                case 32:
                case 33:
                case 34:
                case 42:
                case 52:
                case 53:
                case 54:
                    itemsWord = "stavke";
                    break;
                default:
                    itemsWord = "stavki";
                    break;
            }

            LblUndoMsg.Text = $"Obrisano {_undoItemCounter} {itemsWord}.";

            DeletedBanner.Y = 0;


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

        private void CdAddNewItem_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var crntLvl = (LstObjectTree.SelectedItem as MrpItem).Level;

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

            //AddItem(mainItem, (MrpItem)LstObjectTree.SelectedItem);
            AddItem(mainItem, sel);

            InitListView(ObjectItems);
        }

        private void CdAddNewItem_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
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

            //if (ListViewItems.Count == 1)
            //    btnRemoveGrossNeed.IsEnabled = false;
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

            var plndin = new PlannedInputItem()
            {
                Days = dys,
                Quantity = amnt,
                Textual = $"{amnt} komad(a) {dys}. dan(a)"
            };

            PlndIns.Add(plndin);
            PlannedInputList.ItemsSource = PlndIns;
            BtnRemovePlanedInput.IsEnabled = true;
            //TxtPlnInAmount.Text = "";
            //TxtPlnInDay.Text = "";
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

            var grossneed = new GrossNeedItem()
            {
                Days = dys,
                Quantity = amnt,
                Textual = $"{amnt} komad(a) potrebno {dys}. dan(a)"
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ObjectItems.ItemChildren.Clear();
            ObjectItems.ItemChildren.AddRange(_undoHelper);

            InitListView(ObjectItems);

            LstObjectTree.ItemsSource = ListViewItems;


            LstObjectTree.SelectedIndex = 0;

            BtnDeleteCurrent.IsEnabled = false;

            DeletedBanner.Y = 60;
        }
    }
}