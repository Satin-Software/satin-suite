using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RoomInfo
{
    public string TVId { get; set; } = "";
    public string Room { get; set; } = "";
    public string BookingNumber { get; set; } = "";
    public string GuestName { get; set; } = "";

    public override string ToString()
    {
        return $"tv id={TVId}|room={Room}|booking number={BookingNumber}|guest name={GuestName}|";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not RoomInfo other)
            return false;

        return TVId == other.TVId &&
            Room == other.Room &&
            BookingNumber == other.BookingNumber &&
            GuestName == other.GuestName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TVId, Room, BookingNumber, GuestName);
    }
}

