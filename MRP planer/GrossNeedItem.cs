namespace MRP_planer
{
    public class GrossNeedItem
    {
        public string Textual { get; set; }

        public int Days { get; set; }

        public int Quantity { get; set; }
    }

    public class PlannedInputItem : GrossNeedItem { }
}