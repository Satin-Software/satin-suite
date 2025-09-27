using satin_common;
using System.Text;


namespace satin_interface_tv
{
    public class Program
    {
        private static string tv_file_url = "";
        private static string tv_fileame = "";
        private static string tv_cmnd_ip = "";
        private static string tv_cmnd_port = "";

        // Initialize logger and config
        private static Config? config;
        private static Logger? logger;

        static void Main()
        {
            config = new Config();
            logger = new Logger();

            tv_file_url = config.get("DownloadURL", "http://localhost/");
            tv_fileame = config.get("DownloadFileName", "roominfo.txt");
            tv_cmnd_ip = config.get("cmnd_ip", "localhost");
            tv_cmnd_port = config.get("cmnd_port", "8443");

            satin_interface_tv.Program program = new satin_interface_tv.Program();

            program.download_file();
        }

        public void download_file()
        {
            try
            {
                string file_path = Path.Combine(app_env.tmp_dir, tv_fileame);
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    var response = client.GetAsync(Path.Combine(tv_file_url, tv_fileame)).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsByteArrayAsync().Result;
                        File.WriteAllBytes(file_path, content);
                        logger?.write($"File downloaded successfully to {file_path}");

                        // Compare with old file
                        CompareFiles(file_path, get_old_filename(file_path));
                    }
                    else
                    {
                        logger?.write($"Failed to download file: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.write($"Error downloading file: {ex.Message}");
            }
        }

        public string get_old_filename(string file_name)
        {
            if (!File.Exists(file_name))
            {
                Console.WriteLine($"File not found: {file_name}");
                return "";
            }

            string directory = Path.GetDirectoryName(file_name) ?? "";
            string filenameWithoutExt = Path.GetFileNameWithoutExtension(file_name);
            string extension = Path.GetExtension(file_name);

            string newFileName = $"{filenameWithoutExt}_old{extension}";
            return Path.Combine(directory, newFileName);
        }

        public void CompareFiles(string new_file, string old_file)
        {
            var oldRooms = RoomInfo_parse.ParseFile(old_file);
            var newRooms = RoomInfo_parse.ParseFile(new_file);

            // Create verified room list
            var verifiedRoomList = new List<RoomInfo>();

            // Create dictionaries for quick lookup by TV ID
            var oldRoomDict = oldRooms.ToDictionary(r => r.TVId, r => r);
            var newRoomDict = newRooms.ToDictionary(r => r.TVId, r => r);

            // Process all rooms from new file
            foreach (var newRoom in newRooms)
            {
                if (oldRoomDict.TryGetValue(newRoom.TVId, out var oldRoom))
                {
                    // Room exists in both files
                    if (newRoom.Equals(oldRoom))
                    {
                        // Rows match - add to verified list
                        RoomInfo_parse.AddRoomInfo(verifiedRoomList, newRoom);
                    }
                    else
                    {
                        // Rows don't match - check further based on TVID
                        bool notificationSuccessful = false;

                        // Check if booking number changes from <value> to <empty>
                        if (!string.IsNullOrEmpty(oldRoom.BookingNumber) && string.IsNullOrEmpty(newRoom.BookingNumber))
                        {
                            notificationSuccessful = SendCheckoutNotification(newRoom.TVId);
                        }
                        // Check if booking number changes from <empty> to <value>
                        else if ((string.IsNullOrEmpty(oldRoom.BookingNumber) && !string.IsNullOrEmpty(newRoom.BookingNumber))
                            || newRoom.BookingNumber != oldRoom.BookingNumber)
                        {
                            notificationSuccessful = SendCheckinNotification(newRoom.TVId, newRoom.BookingNumber, newRoom.GuestName);
                        }
                        // Check if guest name is changed
                        else if (oldRoom.GuestName != newRoom.GuestName)
                        {
                            notificationSuccessful = SendModifyInfoNotification(newRoom.TVId, newRoom.BookingNumber, newRoom.GuestName);
                        }

                        if (notificationSuccessful)
                        {
                            // Notification successful - add new room info to verified list
                            logger?.write($"Updated: TV ID {oldRoom.TVId}|room {oldRoom.Room}|booking number {oldRoom.BookingNumber}|guest name {oldRoom.GuestName}");
                            logger?.write($"to     : TV ID {newRoom.TVId}|room {newRoom.Room}|booking number {newRoom.BookingNumber}|guest name {newRoom.GuestName}");
                            RoomInfo_parse.AddRoomInfo(verifiedRoomList, newRoom);
                        }
                        else
                        {
                            // Notification failed - log error and add old room info to verified list
                            logger?.write($"Error updating this record: TV ID {newRoom.TVId}|room {newRoom.Room}|booking number {newRoom.BookingNumber}|guest name {newRoom.GuestName}");
                            RoomInfo_parse.AddRoomInfo(verifiedRoomList, oldRoom);
                        }
                    }
                }
                else
                {
                    // Room only exists in new file - add to verified list
                    RoomInfo_parse.AddRoomInfo(verifiedRoomList, newRoom);
                }
            }

            // Handle rooms that only exist in old file
            foreach (var oldRoom in oldRooms)
            {
                if (!newRoomDict.ContainsKey(oldRoom.TVId))
                {
                    // Room only exists in old file - add to verified list
                    RoomInfo_parse.AddRoomInfo(verifiedRoomList, oldRoom);
                }
            }

            // Write verified room list back to old_file
            WriteVerifiedRoomsToFile(verifiedRoomList, old_file);

            // Print results
            logger?.write($"Verified room list contains {verifiedRoomList.Count} rooms:");
            logger?.write($"Written to {old_file}");
            logger?.write(new string('-', 50));
        }


        private bool SendCheckoutNotification(string roomId)
        {
            // string endpoint = "http://localhost:8080/SmartInstall/services/StayNotification?wsdl";
            string endpoint = $"https://{tv_cmnd_ip}:{tv_cmnd_port}/SmartInstall/services/StayNotification?wsdl";

            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string echoToken = Guid.NewGuid().ToString();

            string soapBody = $@"<?xml version=""1.0"" encoding=""UTF-8"" ?>
                            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" 
                                           xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
                                           xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
                                <soap:Header>
                                    <HTNGHeader xmlns=""http://htng.org/2.0/Header/"">
                                        <From>
                                            <systemId />
                                        </From>
                                    </HTNGHeader>
                                </soap:Header>
                                <soap:Body>
                                <HTNG_HotelCheckOutNotifRQ EchoToken=""{echoToken}"" 
                                                            TimeStamp=""{timestamp}"" 
                                                              Version=""2.0"" 
                                                              Target=""Production"" 
                                                              xmlns=""http://htng.org/2011B"" 
                                                              xmlns:ota=""http://www.opentravel.org/OTA/2003/05"">
                                    <PropertyInfo ChainCode=""Satin"" 
                                                    HotelName=""IPTV Command PC"" 
                                                    BrandCode=""SATIN"" 
                                                    HotelCode=""0000"" 
                                                    HotelCodeContext=""SATN""/>
                                    <Room RoomID=""{roomId}""/>
                                </HTNG_HotelCheckOutNotifRQ>
                              </soap:Body>
                            </soap:Envelope>";

            try
            {
                using HttpClient client = new HttpClient();
                var content = new StringContent(soapBody, Encoding.UTF8, "text/xml");

                HttpResponseMessage response = client.PostAsync(endpoint, content).Result;
                string responseXml = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    logger?.write($"[SOAP] Checkout sent for RoomID={roomId} - Success");
                }
                else
                {
                    logger?.write($"[SOAP] Checkout for RoomID={roomId} failed: {response.StatusCode}");
                    logger?.write($"[SOAP] Response: {responseXml}");
                }
                return true;
            }
            catch (Exception ex)
            {
                logger?.write($"[SOAP] Error sending checkout for RoomID={roomId}: {ex.Message}");
                return false;
            }
        }


        private bool SendCheckinNotification(string roomId, string bookingNumber, string guestName)
        {
            string endpoint = $"https://{tv_cmnd_ip}:{tv_cmnd_port}/SmartInstall/services/StayNotification?wsdl";
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string echoToken = Guid.NewGuid().ToString();

            string soapBody = $@"<?xml version=""1.0"" encoding=""UTF-8"" ?>
                            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" 
                                           xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" 
                                           xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
                                <soap:Header>
                                    <HTNGHeader xmlns=""http://htng.org/2.0/Header/"">
                                        <From>
                                            <systemId />
                                        </From>
                                    </HTNGHeader>
                                </soap:Header>
                                <soap:Body>
                                    <HTNG_HotelCheckInNotifRQ EchoToken=""33f07f05-84e5-48ff-b772-894119d5c194"" 
                                                              TimeStamp=""2011-08-24T09:30:47Z"" 
                                                              Version=""2.0"" 
                                                              Target=""Production"" 
                                                              xmlns=""http://htng.org/2011B"" 
                                                              xmlns:ota=""http://www.opentravel.org/OTA/2003/05"">
                                        <PropertyInfo ChainCode=""Satin"" 
                                                    HotelName=""IPTV Command PC"" 
                                                    BrandCode=""SATIN"" 
                                                    HotelCode=""0000"" 
                                                    HotelCodeContext=""SATN""/>
                                        <Room RoomID=""{roomId}"" />
                                        <HotelReservations>
                                            <ota:HotelReservation WalkInIndicator=""false"" 
                                                                  CreateDateTime=""2011-05-08T09:30:47Z"" 
                                                                  ResStatus=""Checked In"" 
                                                                  RoomStayReservation=""true"" 
                                                                  LastModifyDateTime=""2011-07-18T09:30:47Z"">
                                                <ota:UniqueID Type=""14"" ID=""RES123"" />
                                                <ota:RoomStays>
                                                    <ota:RoomStay>
                                                        <ota:TimeSpan End=""2020-05-29"" Start=""2020-06-01"" />
                                                    </ota:RoomStay>
                                                </ota:RoomStays>
                                                <ota:ResGuests>
                                                    <ota:ResGuest GroupEventCode=""GRP123"">
                                                        <ota:Profiles>
                                                            <ota:ProfileInfo>
                                                                <ota:UniqueID Type=""1"" ID=""GST123"" />
                                                                <ota:Profile CreateDateTime=""2001-12-17T09:30:47Z"" 
                                                                                ProfileType=""1"" 
                                                                                LastModifyDateTime=""2009-08-11T10:43:32Z"">
                                                                    <ota:Customer Language=""en-us"" CurrencyCode=""USD"" Gender=""Male"">
                                                                        <ota:PersonName>
                                                                            <ota:NamePrefix>Mr.</ota:NamePrefix>
                                                                            <ota:GivenName>""{guestName}""</ota:GivenName>
                                                                            <ota:Surname></ota:Surname>
                                                                        </ota:PersonName>
                                                                    </ota:Customer>
                                                                </ota:Profile>
                                                            </ota:ProfileInfo>
                                                        </ota:Profiles>
                                                    </ota:ResGuest>
                                                </ota:ResGuests>
                                            </ota:HotelReservation>
                                        </HotelReservations>
                                    </HTNG_HotelCheckInNotifRQ>
                                </soap:Body >
                            </soap:Envelope >";

            try
            {
                using HttpClient client = new HttpClient();
                var content = new StringContent(soapBody, Encoding.UTF8, "text/xml");
                HttpResponseMessage response = client.PostAsync(endpoint, content).Result;
                string responseXml = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                    logger?.write($"[SOAP] Checkin sent for RoomID={roomId} - Success ()");
                else
                {
                    logger?.write($"[SOAP] Checkin for RoomID={roomId} failed: {response.StatusCode}");
                    logger?.write($"[SOAP] Response: {responseXml}");
                }

                return true;
            }
            catch (Exception ex)
            {
                logger?.write($"[SOAP] Error sending checkin for RoomID={roomId}: {ex.Message}");
                return false;
            }
        }

        private bool SendModifyInfoNotification(string roomId, string bookingNumber, string guestName)
        {
            string endpoint = $"https://{tv_cmnd_ip}:{tv_cmnd_port}/SmartInstall/services/StayNotification?wsdl";
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string echoToken = Guid.NewGuid().ToString();

            string soapBody = $@"<?xml version=""1.0"" encoding=""UTF-8""?> 
                                <HTNG_HotelStayUpdateNotifRQ EchoToken=""adc01ae0-4b6d-4b2a-b265-d7ab096e6a11"" 
                                                            TimeStamp=""2011-08-24T09:30:47Z"" 
                                                            Version=""2.0"" 
                                                            Target=""Production"" 
                                                            xmlns=""http://htng.org/2011B"" 
                                                            xmlns:ota=""http://www.opentravel.org/OTA/2003/05""> 
                                    <PropertyInfo ChainCode=""Satin"" 
                                                    HotelName=""IPTV Command PC"" 
                                                    BrandCode=""SATIN"" 
                                                    HotelCode=""0000"" 
                                                    HotelCodeContext=""SATN""/>
                                    <AffectedGuests> 
                                        <UniqueID Type=""1"" ID=""GST123""/> 
                                        <UniqueID Type=""14"" ID=""RES123""/> 
                                    </AffectedGuests> 
                                    <Room RoomID=""{roomId}""> 
                                        <TelephoneExtensions> 
                                            <TelephoneExtention>00000</TelephoneExtention> 
                                        </TelephoneExtensions> 
                                    </Room> 
                                    <HotelReservations> 
                                        <ota:HotelReservation> 
                                            <ota:UniqueID Type=""14"" ID=""RES123""/> 
                                            <ota:ResGuests> 
                                                <ota:ResGuest> 
                                                    <ota:Profiles> 
                                                        <ota:ProfileInfo> 
                                                            <ota:UniqueID Type=""1"" ID=""GST123""/> 
                                                            <ota:Profile> 
                                                                <ota:Customer> 
                                                                    <ota:PersonName> 
                                                                        <ota:NamePrefix></ota:NamePrefix> 
                                                                        <ota:GivenName>""{guestName}""</ota:GivenName> 
                                                                        <ota:Surname></ota:Surname> 
                                                                    </ota:PersonName> 
                                                                </ota:Customer> 
                                                            </ota:Profile> 
                                                        </ota:ProfileInfo> 
                                                    </ota:Profiles> 
                                                </ota:ResGuest> 
                                            </ota:ResGuests> 
                                        </ota:HotelReservation> 
                                    </HotelReservations> 
                                </HTNG_HotelStayUpdateNotifRQ>";

            try
            {
                using HttpClient client = new HttpClient();
                var content = new StringContent(soapBody, Encoding.UTF8, "text/xml");
                HttpResponseMessage response = client.PostAsync(endpoint, content).Result;
                string responseXml = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                    logger?.write($"[SOAP] Checkin sent for RoomID={roomId} - Success ()");
                else
                {
                    logger?.write($"[SOAP] Checkin for RoomID={roomId} failed: {response.StatusCode}");
                    logger?.write($"[SOAP] Response: {responseXml}");
                }

                return true;
            }
            catch (Exception ex)
            {
                logger?.write($"[SOAP] Error sending modify notification for RoomID={roomId}: {ex.Message}");
                return false;
            }
        
        }

        public void WriteVerifiedRoomsToFile(List<RoomInfo> verifiedRoomList, string filePath)
        {
            try
            {
                var lines = new List<string>();

                // Convert each RoomInfo to the required format
                foreach (var room in verifiedRoomList)
                {
                    lines.Add(room.ToString());
                }

                // Write all lines to the file
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                logger?.write($"Error writing to file {filePath}: {ex.Message}");
            }
        }
    }
}
