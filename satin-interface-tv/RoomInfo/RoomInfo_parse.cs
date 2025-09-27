using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RoomInfo_parse
{
    public static List<RoomInfo> ParseFile(string filePath)
    {
        var roomInfos = new List<RoomInfo>();

        try
        {
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                    continue;

                var roomInfo = ParseLine(line);
                if (roomInfo != null)
                {
                    roomInfos.Add(roomInfo);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file {filePath}: {ex.Message}");
        }

        return roomInfos;
    }

    private static RoomInfo? ParseLine(string line)
    {
        try
        {
            // Split by pipe character
            var parts = line.Split('|');

            if (parts.Length < 4)
                return null;

            var roomInfo = new RoomInfo();

            // Parse each part
            foreach (var part in parts)
            {
                var trimmedPart = part.Trim();

                if (trimmedPart.StartsWith("tv id="))
                {
                    roomInfo.TVId = trimmedPart.Substring(6);
                }
                else if (trimmedPart.StartsWith("room="))
                {
                    roomInfo.Room = trimmedPart.Substring(5);
                }
                else if (trimmedPart.StartsWith("booking number="))
                {
                    roomInfo.BookingNumber = trimmedPart.Substring(15);
                }
                else if (trimmedPart.StartsWith("guest name="))
                {
                    roomInfo.GuestName = trimmedPart.Substring(11);
                }
            }

            return roomInfo;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing line: {line}. Error: {ex.Message}");
            return null;
        }
    }

    // Method to get detailed comparison showing what changed
    public static void AddRoomInfo(List<RoomInfo> roomList, RoomInfo roomInfo)
    {
        if (roomInfo != null)
        {
            roomList.Add(roomInfo);
        }
    }


}

