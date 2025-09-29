using Microsoft.Maui.Devices.Sensors;

namespace OtChaim.Presentation.MAUI.Mappers;
public static class LocationMapper
{
    public static Domain.Common.Location? ToDomainLocation(Location? location)
    {
        return location != null ? new Domain.Common.Location(location.Latitude, location.Longitude) : null;
    }

    public static Location? ToMauiLocation(Domain.Common.Location? location)
    {
        return location != null ? new Location(location.Latitude, location.Longitude) : null;
    }
}
