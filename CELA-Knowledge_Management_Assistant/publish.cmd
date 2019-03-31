nuget restore
msbuild BasicBot.sln -p:DeployOnBuild=true -p:PublishProfile=knowmtest-Web-Deploy.pubxml -p:Password=P4NcGp0JpkhAjemhR9X9fhnzSqZPNpqdFNmFZpamWgrLxS5tNK9ANbYf86rn

