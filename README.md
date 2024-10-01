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

2. Add and configure line `"DockerfileRunArguments": "-v host\\path\\to\\volume\\folder:/app/volumes"` to `launchSettings.json` under `Container (Dockerfile)`.

3. Create two folders inside volume folder. `certificates` and `keystorage`.

4. Place your certificate in the `certificates` folder.
