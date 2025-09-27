# satin-suite

A collection of supporting utilities for satin software.

## Overview

This repository contains following command-line utilities :

- **satin-interface-tv.exe** - synchronize hotel TV check-in/check-out status with PMS
- **satin-mq-recv.exe** - Receives and processes messages from the queue
- **satin-mq-send.exe** - Sends messages to the queue

## Quick Start

### synchronize hotel TV
```bash
.\satin-interface-tv.exe
```

### Receiving Messages
```bash
.\satin-mq-recv.exe
```

### Sending Messages
```bash
.\satin-mq-send.exe "your message here"
.\satin-mq-send.exe "txt1|txt2|txt3"
```

## Documentation
- [satin-interface-tv.exe Manual](docs/satin-interface-tv.md)
- [satin-mq-recv.exe Manual](docs/satin-mq-recv.md)
- [satin-mq-send.exe Manual](docs/satin-mq-send.md)

## Directory Structure

```
.
├── README.md
├── docs\
│   ├── satin-interface-tv.md
│   ├── satin-mq-recv.md
│   └── satin-mq-send.md
```

## Requirements

- Windows environment
- Access to message queue system
- Write permissions to `.\home\` directory

## Configuration

Message queue configuration is managed through `satin-mq-send.ini`. Ensure this file is properly configured before using the send utility.

## Support

For detailed usage instructions, refer to the individual command documentation in the `docs\` folder.