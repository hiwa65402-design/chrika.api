# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory in the container
WORKDIR /app

# Copy the solution file to the root of the WORKDIR
COPY Chrika.Api.sln .

# Copy the actual project directory (chrika.api) to the WORKDIR
# This assumes your project folder is named 'chrika.api' (lowercase) and is directly under the Dockerfile's location
COPY chrika.api/ chrika.api/

# Restore dependencies for the solution
RUN dotnet restore

# Build the application
RUN dotnet build chrika.api/Chrika.Api.csproj -c Release --no-restore

# Publish the application
RUN dotnet publish chrika.api/Chrika.Api.csproj -c Release -o /app/publish --no-restore

# Use the official .NET 8 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose the port that the app runs on
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "Chrika.Api.dll"]
