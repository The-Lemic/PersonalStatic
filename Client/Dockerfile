# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["Client/Client.csproj", "Client/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "Client/Client.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/Client"
RUN dotnet publish "Client.csproj" -c Release -o /app/publish

# Runtime stage - use nginx to serve static files
FROM nginx:alpine AS runtime
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY Client/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80