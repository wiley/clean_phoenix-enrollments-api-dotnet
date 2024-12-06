FROM mcr.microsoft.com/dotnet/aspnet:6.0-bookworm-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ADD --chmod=644 https://truststore.pki.rds.amazonaws.com/global/global-bundle.pem /cert/global-bundle.pem

WORKDIR /cert/

RUN cat global-bundle.pem|awk 'split_after==1{n++;split_after=0} /-----END CERTIFICATE-----/ {split_after=1} {print > "cert" n ""}' ;\
    for CERT in /cert/cert*; do mv $CERT /usr/local/share/ca-certificates/aws-rds-ca-$(basename $CERT).crt; done ;\
    update-ca-certificates

FROM mcr.microsoft.com/dotnet/sdk:6.0-bookworm-slim AS build
WORKDIR /src

ARG ART_USER
ARG ART_PASS
ARG ART_URL

RUN dotnet nuget add source --name crossknowledge/phoenix $ART_URL --username $ART_USER --password $ART_PASS --store-password-in-clear-text
RUN mkdir Enrollments.API
COPY ./Enrollments.API/Enrollments.API.csproj ./Enrollments.API/
RUN mkdir Enrollments.Domain
COPY ./Enrollments.Domain/Enrollments.Domain.csproj ./Enrollments.Domain/
RUN mkdir Enrollments.Infrastructure
COPY ./Enrollments.Infrastructure/Enrollments.Infrastructure.csproj ./Enrollments.Infrastructure/
RUN mkdir Enrollments.Infrastructure.Interface
COPY ./Enrollments.Infrastructure.Interface/Enrollments.Infrastructure.Interface.csproj ./Enrollments.Infrastructure.Interface/
RUN mkdir Enrollments.Services
COPY ./Enrollments.Services/Enrollments.Services.csproj ./Enrollments.Services/
RUN dotnet restore "Enrollments.API/Enrollments.API.csproj"
COPY . .
WORKDIR "/src/Enrollments.API"
RUN dotnet build "Enrollments.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Enrollments.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Enrollments.API.dll"]
