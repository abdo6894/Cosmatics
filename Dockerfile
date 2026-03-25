FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY Src/Cosmatics.API/Cosmatics.API.csproj ./Cosmatics.API/
COPY Src/Cosmatics.Application/Cosmatics.Application.csproj ./Cosmatics.Application/
COPY Src/Cosmatics.Domain/Cosmatics.Domain.csproj ./Cosmatics.Domain/
COPY Src/Cosmatics.Infrastructure/Cosmatics.Infrastructure.csproj ./Cosmatics.Infrastructure/


RUN dotnet restore ./Cosmatics.API/Cosmatics.API.csproj


COPY Src/. .


WORKDIR /src/Cosmatics.API
RUN dotnet build Cosmatics.API.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish Cosmatics.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cosmatics.API.dll"]