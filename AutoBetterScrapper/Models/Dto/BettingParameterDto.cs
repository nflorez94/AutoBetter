namespace AutoBetterScrapper.Models.Dto
{
    public class BettingParameterDto
    {
        public Guid Id { get; set; }
        public decimal MinimumBetAmount { get; set; }
        public decimal MaximumBetAmount { get; set; }
        public decimal MinimumBetCuota { get; set; }
        public decimal MaximumBetCuota { get; set; }
        public decimal IncreaseFactor { get; set; }
        public int SimultaneousBets { get; set; }
        public bool Active { get; set; }
    }
}
