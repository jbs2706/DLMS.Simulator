FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Gurux.DLMS.Simulator.Net.csproj", "./"]
RUN dotnet restore "Gurux.DLMS.Simulator.Net.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "Gurux.DLMS.Simulator.Net.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Gurux.DLMS.Simulator.Net.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .

# Add the arguments directly to the ENTRYPOINT
ENTRYPOINT ["dotnet", "Gurux.DLMS.Simulator.Net.dll"]
CMD [ "-p", "1000", "-x", "./meter_template.xml", "-t", "Verbose", "-N", "10", "-i", "WRAPPER", "-P", "foobar" ]
