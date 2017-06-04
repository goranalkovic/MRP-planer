namespace MRP_planer
{
    public class TableColumn
    {
        public int Period { get; set; }

        public string GrossNeed { get; set; }

        public string PlannedInput { get; set; }

        public string AvailableQty { get; set; }

        public string NetNeeds { get; set; }

        public string PlannedGet { get; set; }

        public string PlannedSend { get; set; }

        public TableColumn(int period, string grsnd = "", string plndin = "", string availq = "", string netnds = "", string plndget = "", string plndsnd = "")
        {
            Period = period;
            GrossNeed = grsnd;
            PlannedInput = plndin;
            AvailableQty = availq;
            NetNeeds = netnds;
            PlannedGet = plndget;
            PlannedSend = plndsnd;
        }
    }
}