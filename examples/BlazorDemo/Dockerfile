FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Prompt2Plot/ src/Prompt2Plot/
COPY src/Prompt2Plot.ClickHouse/ src/Prompt2Plot.ClickHouse/
COPY src/Prompt2Plot.OpenAI/ src/Prompt2Plot.OpenAI/
COPY examples/BlazorDemo/ examples/BlazorDemo/
RUN dotnet publish ./examples/BlazorDemo/BlazorDemo.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "BlazorDemo.dll"]