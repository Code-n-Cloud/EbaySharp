using EbaySharp.Entities.Develop.SellingApps.ListingManagement.Inventory.Location;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EbaySharp.Tests
{
    public class GeoCoordinatesTests
    {
        [Test]
        public void GeoCoordinates_UseNullableDecimalProperties()
        {
            Assert.That(typeof(GeoCoordinates).GetProperty(nameof(GeoCoordinates.Latitude))?.PropertyType, Is.EqualTo(typeof(decimal?)));
            Assert.That(typeof(GeoCoordinates).GetProperty(nameof(GeoCoordinates.Longitude))?.PropertyType, Is.EqualTo(typeof(decimal?)));
        }

        [Test]
        public void GeoCoordinates_AssignsDecimalValues()
        {
            GeoCoordinates geoCoordinates = new()
            {
                Latitude = -37.0000m,
                Longitude = 145.0000m
            };

            Assert.That(geoCoordinates.Latitude, Is.EqualTo(-37.0000m));
            Assert.That(geoCoordinates.Longitude, Is.EqualTo(145.0000m));
        }

        [Test]
        public void GeoCoordinates_SerializesAsJsonNumbers()
        {
            GeoCoordinates geoCoordinates = new()
            {
                Latitude = -37.0000m,
                Longitude = 145.0000m
            };

            JsonSerializerOptions options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            options.Converters.Add(new JsonStringEnumConverter());

            string json = JsonSerializer.Serialize(geoCoordinates, options);

            Assert.That(json, Is.EqualTo("{\"latitude\":-37.0000,\"longitude\":145.0000}"));
        }
    }
}
