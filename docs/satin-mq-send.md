# satin-mq-send(1)

## NAME

**satin-mq-send** - send messages (max-length-bytes:1048576) to message queue

## SYNOPSIS

```
satin-mq-send.exe "message"
satin-mq-send.exe "txt1|txt2|txt3..."
```

## DESCRIPTION

**satin-mq-send** is a message queue sender utility that pushes messages to a configured message queue system. The utility reads its configuration from `satin-mq-send.ini` and sends the provided message to the specified queue.

## ARGUMENTS

### message
A string containing the message to be sent to the queue. The message can contain various formats:

- **Simple text**: Any plain text message
- **Pipe-separated values**: Multiple values separated by pipe characters (|)
- **Complex content**: Any string content that fits within system message size limits

## BEHAVIOR

### Message Sending Process

When executed, **satin-mq-send** performs the following operations:

1. **Configuration Loading**: Reads queue configuration from `satin-mq-send.ini`
2. **Queue Connection**: Establishes connection to the specified message queue
3. **Message Transmission**: Pushes the provided message to the queue
4. **Logging**: Appends operation results to the log file

### Configuration

The utility relies on `satin-mq-send.ini` for queue configuration. This file must be present in the same directory as the executable and contain the necessary queue connection parameters.

### Logging

All operations are logged to files in the `.\home\log\` directory:
- **Location**: `.\home\log\`
- **Behavior**: Log entries are **always** appended
- **Content**: Includes timestamp, operation status, and message details

## OPTIONS

The utility currently accepts the message as a positional argument. Configuration options are managed through the `satin-mq-send.ini` file.

## EXIT STATUS

The command returns standard exit codes:

- **0**: Success (message sent successfully)
- **1**: Error (configuration error, queue connection failure, etc.)

## FILES

### Configuration
- `satin-mq-send.ini` - Queue configuration file (required)
```
home\
├── satin-mq-send.exe
├── satin-mq-send.ini
│   ├── [MQ]
│        Host=127.0.0.1
│        Port=5672
│        Username=satin
│        Password=satin
│        Queue=my/test/topic
│
```

### Output
- `.\home\log\*` - Log files (always appended)

### Dependencies
- Valid `satin-mq-send.ini` configuration file
- Network access to message queue system
- Write permissions to `.\home\log\` directory

## EXAMPLES

### Send Simple Message
```powershell
.\satin-mq-send.exe "Hello World"
```

### Send Pipe-Separated Data
```powershell
.\satin-mq-send.exe "txt1|txt2|txt3"
```

### Send Complex Message
```powershell
.\satin-mq-send.exe "Process ID: 1234| Status: Complete| Timestamp: 2024-08-19"
```

### Multiple Message Example
```powershell
# Send several messages
.\satin-mq-send.exe "First message"
.\satin-mq-send.exe "Second message"  
.\satin-mq-send.exe "data1|data2|data3"
```

## CONFIGURATION FILE

The `satin-mq-send.ini` file must contain the queue configuration. Ensure this file is properly configured with:

- Queue connection parameters
- Authentication details (if required)
- Queue name\identifier
- Any protocol-specific settings

## NOTES

- Messages are sent to the queue specified in `satin-mq-send.ini`
- The utility handles one message per execution
- Message content should be properly escaped if it contains special characters
- Large messages may be subject to queue system limitations

## ERROR HANDLING

Common error conditions:

- **Missing configuration file**: Ensure `satin-mq-send.ini` exists and is readable
- **Queue connection failure**: Verify network connectivity and queue system status
- **Permission denied**: Check write permissions for log directory
- **Invalid message format**: Verify message content meets queue system requirements

## SEE ALSO

**satin-mq-recv**(1)

## AUTHOR
Pradeep
Created as part of the satin-suite.