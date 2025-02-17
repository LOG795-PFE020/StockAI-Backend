# To launch

- ensure you have updated your dotnet sdk to .net 8 https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- ensure you have the latest Visual Studio version installed. If not, download visual studio installer and update (make sure asp.net core is in)
- have docker desktop installed

**if you did one of the above, restart your computer**

in the root of folder (the one where this file is), open a terminal (powershell preferably)

run the following commands
```
dotnet dev-certs https --clean
dotnet dev-certs https --trust
dotnet dev-certs https -ep ./Configuration/certs/localhost.pfx -p "secret"
```
go in Configuration/certs/ manually and click on the localhost.pfx and install it on your system

run the following command
```
docker compose up --build
```

*Voila*

Next: launch the frontend!