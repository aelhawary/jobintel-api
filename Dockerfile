# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY RecruitmentPlatformAPI/*.csproj ./RecruitmentPlatformAPI/
RUN dotnet restore RecruitmentPlatformAPI/RecruitmentPlatformAPI.csproj

# Copy everything and build
COPY . .
WORKDIR /src/RecruitmentPlatformAPI
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Create uploads directory
RUN mkdir -p /app/Uploads/ProfilePictures /app/Uploads/Resumes

COPY --from=build /app/publish .

# Expose port (Render uses PORT env variable)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "RecruitmentPlatformAPI.dll"]
