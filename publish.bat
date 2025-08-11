dotnet publish satin-mq-recv\satin-mq-recv.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/
dotnet publish satin-mq-send\satin-mq-send.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/
