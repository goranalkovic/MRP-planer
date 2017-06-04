using System.Collections.Generic;
using Windows.UI.Xaml;


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
}