FROM microsoft/dotnet:2.1-sdk AS builder

WORKDIR /src
COPY ./src/LakseBot .
RUN dotnet publish -c Release -o /app

FROM microsoft/dotnet-nightly:2.1-aspnetcore-runtime-alpine

WORKDIR /app
COPY --from=builder /app .