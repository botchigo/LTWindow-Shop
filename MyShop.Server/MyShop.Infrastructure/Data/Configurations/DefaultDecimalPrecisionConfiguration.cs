using Microsoft.EntityFrameworkCore;

namespace MyShop.Infrastructure.Data.Configurations
{
    public class DefaultDecimalPrecisionConfiguration
    {
        public static void Configure(ModelBuilder buider)
        {
            foreach (var entityType in buider.Model.GetEntityTypes())
            {
                var decimalProperties = entityType
                    .GetProperties()
                    .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

                foreach (var decimalProperty in decimalProperties)
                {
                    decimalProperty.SetPrecision(18);
                    decimalProperty.SetScale(2);
                }
            }
        }
    }
}
