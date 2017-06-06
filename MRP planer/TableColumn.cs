namespace MRP_planer
{
    public class TableColumn
    {
        public int Period { get; set; }

        public double GrossNeed { get; set; }

        public double PlannedInput { get; set; }

        public double AvailableQty { get; set; }

        public double NetNeeds { get; set; }

        public double PlannedGet { get; set; }

        public double PlannedSend { get; set; }

        //public TableColumn(int period, double grsnd = 0.0, double plndin = 0.0, double availq = 0.0, double netnds = 0.0, double plndget = 0.0, double plndsnd = 0.0)
        //{
        //    Period = period;
        //    GrossNeed = grsnd;
        //    PlannedInput = plndin;
        //    AvailableQty = availq;
        //    NetNeeds = netnds;
        //    PlannedGet = plndget;
        //    PlannedSend = plndsnd;
        //}
    }
}