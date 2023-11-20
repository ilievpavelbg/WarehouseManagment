# Use the official ASP.NET Core runtime image as the base image
FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /source

COPY . .

# Create a directory in the image to copy the project files
RUN dotnet restore "./WarehouseManagment.csproj" --disable-parallel
RUN dotnet publish "./WarehouseManagment.csproj" -c release -o /app --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal
WORKDIR /app
COPY --from=build /app ./

EXPOSE 5000

ENTRYPOINT ["dotnet", "WarehouseManagment.dll"]
