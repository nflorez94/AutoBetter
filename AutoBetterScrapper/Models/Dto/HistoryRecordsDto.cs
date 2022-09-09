namespace AutoBetterScrapper.Models.Dto
{
    public class HistoryRecordDto
    {
        public string Cupon { get; set; }
        public string Estado { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Apostado { get; set; }
        public decimal Cuota { get; set; }
        public decimal Pago { get; set; }
    }
}
