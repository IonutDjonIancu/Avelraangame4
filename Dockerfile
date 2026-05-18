# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Avelraangame4/Avelraangame4.csproj Avelraangame4/
COPY Services/Services.csproj Services/
COPY Models/Models.csproj Models/
COPY Statics/Statics.csproj Statics/

RUN dotnet restore Avelraangame4/Avelraangame4.csproj

COPY . .

RUN dotnet publish Avelraangame4/Avelraangame4.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Avelraangame4.dll"]