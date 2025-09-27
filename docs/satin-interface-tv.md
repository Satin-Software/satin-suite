# satin-interface-tv(1)

## NAME

**satin-interface-tv** — synchronize hotel TV check-in/check-out status with PMS

## SYNOPSIS

```
satin-interface-tv.exe
```

## DESCRIPTION

**satin-interface-tv** is a utility that integrates hotel booking information with CMND.io.  
It reads booking data from an FTP source, determines check-in or check-out status, and sends the corresponding SOAP request to CMND.io.

## BEHAVIOR

### Booking Data Processing

When executed, **satin-interface-tv** performs the following operations:

1. **FTP Retrieval**: Reads booking details from the configured FTP source  
2. **Check-in Handling**:  
   - If a booking record exists, sends a SOAP *Check-in* request to CMND.io  
   - Example FTP data:  
     ```
     tv id=00101|room=101|booking number=12345|guest name=Dennis Ritchie|
     ```
3. **Check-out Handling**:  
   - If a booking record is missing, sends a SOAP *Check-out* request to CMND.io  
   - Example FTP data:  
     ```
     tv id=00101|room=101|booking number=|guest name=|
     ```

## OUTPUT FILES

### Log Files
**Logging**: Appends all execution results to the log file in `.\home\log\`
- **Location**: `.\home\log\`
- **Purpose**: Records every execution attempt and its outcome
- **Behavior**: Log entries are always appended

### Temporary Files
**Temporary Storage**: Saves the last processed request in the `.\home	mp\` folder
- **Location**: `.\home\tmp\`
- **Purpose**: Stores yjr ftp file from most recent request for future reference.
- **Behavior**: Overwritten on each execution


## FILES

### Configuration
- `satin-interface-tv.ini` — Configuration file specifying FTP and SOAP settings

```
home\
├── satin-interface-tv.exe
├── satin-interface-tv.ini
│   ├── [FTP]
│   │    Host=ftp.example.com
│   │    Port=ftpuser
│   │    Username=ftpuser
│   │    Password=ftppass
│   │
```

### Logs
- `.\home\log\*` — Log files, appended for each run

### Temporary
- `.\home\tmp\*` — Stores the last FTP request processed

## EXAMPLES

### Basic Usage
```powershell
# Run the interface to sync booking status with room TV
.\satin-interface-tv.exe
```

### Expected Output Structure
```
home\
├── log\
│   └── 20240927.log                    # Execution logs
└── tmp\
    └── satin-iptv-244416895_old.txt    # Last SOAP request sent
```

## NOTES

- **Check-in** requests are triggered when a booking number is present  
- **Check-out** requests are triggered when a booking number is missing  
- Logs are always appended, even if no update occurs  
- The utility maintains only the **last processed request** in `.\home	mp\`

## SEE ALSO

**satin-mq-recv**(1), **satin-mq-send**(1)

## AUTHOR
Pradeep  
Created as part of the *satin-suite*.
