using System.ComponentModel;

namespace AutoBetterScrapper.Repositories.Data
{
    public enum EnumBetHistoryParams
    {
        [Description("@Cupon")]
        Cupon,

        [Description("@Apostado")]
        Apostado,

        [Description("@Estado")]
        Estado,

        [Description("@Fecha")]
        Fecha,

        [Description("@Cuota")]
        Cuota,

        [Description("@Pago")]
        Pago,

        [Description("@Cant")]
        CantidadFiltro
    }
}
