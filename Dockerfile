# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory in the container
WORKDIR /app

# Copy the solution file to the root of the WORKDIR
COPY Chrika.Api.sln .

# Create the source directory structure inside the container
RUN mkdir -p src/Chrika.Api

# Copy the project file to its correct location inside the container
COPY src/Chrika.Api/Chrika.Api.csproj src/Chrika.Api/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code to the correct location
COPY src/Chrika.Api/ src/Chrika.Api/

# Build the application
RUN dotnet build src/Chrika.Api/Chrika.Api.csproj -c Release --no-restore

# Publish the application
RUN dotnet publish src/Chrika.Api/Chrika.Api.csproj -c Release -o /app/publish --no-restore

# Use the official .NET 8 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose the port that the app runs on
EXPOSE 5000

# Set environment variables
# ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "chrika.Api.dll"]
