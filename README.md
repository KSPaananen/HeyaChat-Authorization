# HeyaChat-Authorization
Authorization backend for Heya!Chat

## Running the application
### Development
1. Create and configure an `appsettings.json` file with this template:
```
{
  "dataprotection": {
    "applicationname": "HeyaChat-Authorization",
    "keystoragepath": "/app/volumes/keystorage/",
    "certificatepath": "/app/volumes/certificates/ExampleCertificate.pfx,
    "certificatepassword": "",
    "averagekeylifetime": "00:00:00"
  },
  "jwt": {
    "signingkey": "",
    "issuer": "",
    "audience": "",
    "lifetime": "00:00:00",
    "renewtime": "00:00:00"
  },
  "encryption": {
    "key": ""
  },
  "emailservice": {
    "sender": "",
    "password": "",
    "host": "",
    "port": "0"
  },
  "codes": {
    "lifetime": "00:00:00"
  },
  "ratelimiter": {
    "permitlimit": "0",
    "timewindow": "00:00:00",
    "queuelimit":  "0"
  },
  "ConnectionStrings": {
  "postgresqlserver": "",
  "Aws":  "",
  "Azure": ""
},
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "Console": {
      "LogLevel": {
        "Default": "Warning"
      }
    }
  }
}
```

2. Create a designated volume folder for docker and create folders `certificates` and `keystorage` inside it.

3. Create a dev certificate into `certificates` folder with these commands:
   - `dotnet dev-certs https -ep E:\Host\path\to\volume\folder\certificate.pfx -p yourpassword `
   - `dotnet dev-certs https --trust`

4. Add these arguments to `"DockerfileRunArguments"` in `Container (Dockerfile)` at `launchSettings.json`:
   - `-v host\\path\\to\\designated\\volume\\folder:/app/volumes` 
   - `-e ASPNETCORE_Kestrel__Certificates__Default__Password=\"yourpassword\"`
   - `-e ASPNETCORE_Kestrel__Certificates__Default__Path=/app/volumes/certificates/certificate.pfx`
