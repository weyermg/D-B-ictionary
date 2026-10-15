FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["D(B)ictionary.csproj", "./"]
RUN dotnet restore "D(B)ictionary.csproj"

COPY . .
RUN dotnet publish "D(B)ictionary.csproj" --configuration Release --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "D(B)ictionary.dll"]