# Use the Microsoft's official .NET Core SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["neurocache-gateway.csproj", "./"]
RUN dotnet restore "neurocache-gateway.csproj"
COPY . .
RUN dotnet publish "neurocache-gateway.csproj" -c Release -o /app/publish

# Use the official ASP.NET Core runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "neurocache-gateway.dll"]
