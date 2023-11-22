# Use the Microsoft's official .NET Core SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["dotnet_client_shuttle.csproj", "./"]
RUN dotnet restore "dotnet_client_shuttle.csproj"
COPY . .
RUN dotnet publish "dotnet_client_shuttle.csproj" -c Release -o /app/publish

# Use the official ASP.NET Core runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "dotnet_client_shuttle.dll"]
