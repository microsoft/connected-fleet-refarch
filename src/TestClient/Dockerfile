FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set the working directory
WORKDIR /app

# Copy everything
COPY src/TestClient ./

# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out


# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app
COPY --from=build-env /app/out .
COPY --from=build-env /app/SamplePayloads ./SamplePayloads
COPY infra/deployment/TelemetryPlatform/cert-gen/certs/*.cert.pem /certs/
COPY infra/deployment/TelemetryPlatform/cert-gen/certs/*.key.pem /certs/

# Set the environment variables for repeat as boolean and update time as a time in milliseconds
ENV REPEAT true
ENV UPDATE_INTERVAL 20000

#Set environment variables for the certificate path
ENV CERTS_PATH /certs/


# Set the entry point to run the compiled code
ENTRYPOINT ["./TestClient"]
