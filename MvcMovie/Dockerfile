# Stage 1: Base Runtime Env (Tiny footprint for minimal security exposure)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build Env (Heavy SDK used only to compile C# code)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project file first and restore dependencies (Optimizes Docker layer caching)
COPY ["MvcMovie.csproj", "./"]
RUN dotnet restore "MvcMovie.csproj"

# Copy the rest of the application files and build
COPY . .
WORKDIR "/src/."
RUN dotnet build "MvcMovie.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish Env (Composes finalized deployment binaries)
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MvcMovie.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final Target Env (Combines tiny runtime base with compiled app)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MvcMovie.dll"]
