#Sets the build-env with sdk

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /src
#Creates a folder for every project in the solution
COPY SeguridadApp.NetCoreSolution.sln ./
RUN mkdir LibraryTester
COPY LibraryTester/*.csproj ./LibraryTester
RUN mkdir LibraryTesterConsoleApp
COPY LibraryTesterConsoleApp/*.csproj ./LibraryTesterConsoleApp
RUN mkdir ./EntityLibrary
COPY EntityLibrary/*.csproj ./EntityLibrary
RUN mkdir ./DataAccessContracts
COPY DataAccessContracts/*.csproj           ./DataAccessContracts
RUN mkdir ./DataAccessSecurity
COPY DataAccessSecurity/*.csproj            ./DataAccessSecurity
RUN mkdir ./SecurityLogicLibraryContracts
COPY SecurityLogicLibraryContracts/*.csproj ./SecurityLogicLibraryContracts
RUN mkdir ./SecurityLogicLibrary
COPY SecurityLogicLibrary/*.csproj          ./SecurityLogicLibrary
RUN mkdir SecurityServicesContracts
COPY SecurityServicesContracts/*.csproj ./SecurityServicesContracts
RUN mkdir SeguridadAppAPI
COPY SeguridadAppAPI/*.csproj               ./SeguridadAppAPI
#restore all the packages
RUN dotnet restore
#copy all the files to the working folder
COPY . ./
#Sample how to build and publish every dependant project
# WORKDIR /src/EntityLibrary
# RUN dotnet build -c Release -o /app

# WORKDIR /src/DataAccessContracts
# RUN dotnet build -c Release -o /app

# WORKDIR /src/DataAccessSecurity
# RUN dotnet build -c Release -o /app

# WORKDIR /src/SecurityLogicLibraryContracts
# RUN dotnet build -c Release -o /app

# WORKDIR /src/SecurityLogicLibrary
# RUN dotnet build -c Release -o /app

# WORKDIR /src/SecurityServicesContracts
# RUN dotnet build -c Release -o /app


#publish to the /app/out directory the full project output
WORKDIR /src/SeguridadAppAPI
RUN dotnet publish -c Release -o /app/out



# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "SeguridadAppAPI.dll"]