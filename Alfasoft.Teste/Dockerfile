FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Alfasoft.Teste/Alfasoft.Teste.csproj", "Alfasoft.Teste/"]
RUN dotnet restore "Alfasoft.Teste/Alfasoft.Teste.csproj"
COPY . .
WORKDIR "/src/Alfasoft.Teste"
RUN dotnet build "Alfasoft.Teste.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Alfasoft.Teste.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Alfasoft.Teste.dll"]