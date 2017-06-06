using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml;
using Newtonsoft.Json;


namespace MRP_planer 
{
    public  class MrpItem
    {
        public  string Name { get; set; }

        public  int AcquireDays { get; set; }

        public  string LotSize { get; set; }

        public  int LotSizeNum { get; set; }

        public  int AvailableInStorage { get; set; }

        public  List<PlannedInputItem> PlannedInput { get; set; }

        public  List<GrossNeedItem> GrossNeeds { get; set; }

        public  Thickness IndentSize { get; set; }

        public  string ItemIcon { get; set; }
        
        public  List<MrpItem> ItemChildren { get; set; }

        public  int Quantity { get; set; }

        public  int Level { get; set; }

        public List<KeyValuePair<double, int>> PlannedOuts = new List<KeyValuePair<double, int>>();

        //public  void CloneTo(MrpItem destination)
        //{
        //    if (destination == null) return;
        //    destination.ItemChildren.Clear();
        //    var curr = new MrpItem(this);
        //    destination.Name = curr.Name;
        //    destination.AcquireDays = curr.AcquireDays;
        //    destination.LotSize = curr.LotSize;
        //    destination.LotSizeNum = curr.LotSizeNum;
        //    destination.AvailableInStorage = curr.AvailableInStorage;
        //    destination.PlannedInput = curr.PlannedInput;
        //    destination.GrossNeeds = curr.GrossNeeds;
        //    destination.IndentSize = curr.IndentSize;
        //    destination.ItemIcon = curr.ItemIcon;
        //    destination.ItemChildren = curr.ItemChildren;
        //    destination.Quantity = curr.Quantity;
        //    destination.Level = curr.Level;
        //}

        //public MrpItem() { }

        //public MrpItem (MrpItem source)
        //{
        //    Name = source.Name;
        //    AcquireDays = source.AcquireDays;
        //    LotSize = source.LotSize;
        //    LotSizeNum = source.LotSizeNum;
        //    AvailableInStorage = source.AvailableInStorage;
        //    PlannedInput = source.PlannedInput;
        //    GrossNeeds = source.GrossNeeds;
        //    IndentSize = source.IndentSize;
        //    ItemIcon = source.ItemIcon;
        //    ItemChildren = source.ItemChildren;
        //    Quantity = source.Quantity;
        //    Level = source.Level;
        //}

        public MrpItem Clone()
        {
            if (ReferenceEquals(this, null))
                return default(MrpItem);
            return JsonConvert.DeserializeObject<MrpItem>(JsonConvert.SerializeObject(this));
        }
     
    }
}