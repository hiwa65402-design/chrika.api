# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory in the container
WORKDIR /app

# Copy the solution file to the root of the WORKDIR
COPY chrika.api.sln .

# Copy the actual project directory (chrika.api - lowercase) to the WORKDIR
COPY chrika.api/ chrika.api/

# Restore dependencies for the solution
RUN dotnet restore

# Build the application
RUN dotnet build chrika.api/chrika.api.csproj -c Release --no-restore

# Publish the application
RUN dotnet publish chrika.api/chrika.api.csproj -c Release -o /app/publish --no-restore

# Use the official .NET 8 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose the port that the app runs on (Railway will map its own port to this)
EXPOSE 80

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
# Correct the casing for the .dll file name
ENTRYPOINT ["dotnet", "Chrika.Api.dll"]
