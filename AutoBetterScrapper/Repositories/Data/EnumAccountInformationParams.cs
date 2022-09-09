using System.ComponentModel;

namespace AutoBetterScrapper.Repositories.Data
{
    public enum EnumAccountInformationParams
    {
        [Description("@Id")]
        Id,
        
        [Description("@Email")]
        Email,
        
        [Description("@Password")]
        Password,
        
        [Description("@Balance")]
        Balance,
        
        [Description("@InitialDeposit")]
        InitialDeposit,
        
        [Description("@CountLost")]
        CountLost
  
    }
}
