# satin-mq-recv(1)

## NAME

**satin-mq-recv** - receive and process messages from message queue

## SYNOPSIS

```
satin-mq-recv.exe
```

## DESCRIPTION

**satin-mq-recv** is a message queue receiver utility that retrieves the oldest message from the configured message queue and processes it according to the following behavior:

## BEHAVIOR

### Message Processing

When executed, **satin-mq-recv** performs the following operations:

1. **Queue Check**: Checks the configured message queue for available messages
2. **Message Retrieval**: If messages are present, retrieves the oldest message
3. **File Creation**: Creates an output file with the retrieved message content (Message Files)
4. **Logging**: Appends operation results to the log file (Log Files)

### Output Files

#### Message Files
- **Location**: `.\home\mq\`
- **Naming Pattern**: `satin-mq-recv_<timestamp>.txt`
- **Content**: Contains the complete text of the received message
- **Creation Condition**: File is created **only** if a message was available in the queue

#### Log Files
- **Location**: `.\home\log\`
- **Purpose**: Records all execution attempts and their outcomes
- **Behavior**: Log entries are **always** appended, regardless of message availability

### Timestamp Format

The timestamp in the output filename follows the format used by the system at execution time, ensuring unique filenames for each message received.

## EXIT STATUS

The command returns standard exit codes:

- **0**: Success (message received and processed)

## FILES

### Configuration
- `satin-mq-recv.ini` - Queue configuration file (required)
```
home\
├── satin-mq-recv.exe
├── satin-mq-recv.ini
│   ├── [MQ]
│        Host=127.0.0.1
│        Port=5672
│        Username=satin
│        Password=satin
│        Queue=my/test/topic
│
```

### Output
- `.\home\mq\satin-mq-recv_<timestamp>.txt` - Message content (created only when message available)
- `.\home\log\*` - Log files (always appended)

### Dependencies
- System message queue configuration
- Write permissions to `.\home\mq\` and `.\home\log\` directories

## EXAMPLES

### Basic Usage
```powershell
# Receive oldest message from queue
.\satin-mq-recv.exe
```

### Expected Output Structure
```
home\
├── mq\
│   ├── satin-mq-recv_20240819_143022.txt    # Message content
│   └── satin-mq-recv_20240819_143045.txt    # Another message
└── log\
    └── logfilename.log                      # Execution logs (name in .ini)
```

## NOTES

- The utility processes **only one message per execution**
- If the queue is empty, no message file is created, but logging still occurs
- Message files are created with unique timestamps to prevent overwrites
- The utility retrieves messages in FIFO (First In, First Out) order

## SEE ALSO

**satin-mq-send**(1)

## AUTHOR
Pradeep
Created as part of the satin-suite.