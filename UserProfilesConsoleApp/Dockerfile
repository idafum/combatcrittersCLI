# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy project files and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy pre-restored files and build the app
COPY . .
RUN dotnet publish -c Release -o /app

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Set the entry point
ENTRYPOINT ["dotnet", "UserProfilesConsoleApp.dll"]
