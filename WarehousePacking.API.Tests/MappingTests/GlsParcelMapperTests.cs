using WarehousePacking.API.Integrations.Couriers.Mapping;
using WarehousePacking.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.API.Tests.MappingTests
{
    public class GlsParcelMapperTests
    {
        private readonly GlsParcelMapper _mapper = new GlsParcelMapper();

        [Fact, Trait("Category", "Unit")]
        public void Map_Should_Map_All_Fields_Correctly()
        {
            // Arrange
            var package = new PackageData
            {
                Id = 1,
                RecipientName = "John Doe",
                RecipientCity = "Warsaw",
                RecipientStreet = "Main 1",
                RecipientPostalCode = "00-001",
                RecipientCountry = "PL",
                RecipientPhone = "123456789",
                RecipientEmail = "john@example.com",
                Description = "Test package",
                References = "REF123",
                PackageQuantity = 2,
                Weight = 5.5m,
                ShipmentServices = new ShipmentServices
                {
                    POD = true,
                    EXW = false,
                    COD = true,
                    CODAmount = 100
                }
            };

            // Act
            var consign = _mapper.Map(package);

            // Assert
            Assert.NotNull(consign);
            Assert.Equal("John Doe", consign.rname1);
            Assert.Equal("Warsaw", consign.rcity);
            Assert.Equal(2, consign.quantity);
            Assert.True(consign.srv_bool.pod);
            Assert.False(consign.srv_bool.exw);
            Assert.True(consign.srv_bool.cod);
            Assert.Equal(100f, consign.srv_bool.cod_amount);
        }
    }
}