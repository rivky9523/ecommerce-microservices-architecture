# TEMPORARY WORKAROUND (Phase 1):
# NuGet restore fails inside Docker on this machine (network/proxy/certificate
# issue: NU1301 cannot reach api.nuget.org). So we do NOT build inside Docker.
# Instead, publish the app locally first, then this image just runs the output.
#
# Build flow:
#   dotnet publish ./src/ECommerce.Monolith.Api/ECommerce.Monolith.Api.csproj -c Release -o ./src/ECommerce.Monolith.Api/publish
#   docker compose up --build
#
# Once Docker has network access to NuGet again, this can be restored to a
# normal multi-stage build (SDK build stage + runtime stage).

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the locally-published output into the image.
COPY publish/ ./

# The app listens on port 8080 inside the container.
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ECommerce.Monolith.Api.dll"]
