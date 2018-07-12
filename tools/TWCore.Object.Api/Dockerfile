FROM microsoft/dotnet:2.1-runtime-deps
COPY ./app/. ./App/.

ENV TWCORE_FORCE_ENVIRONMENT=Docker

EXPOSE 50999/tcp
EXPOSE 28905/tcp

VOLUME /App/logs
VOLUME /App/settings
VOLUME /App/assemblies

WORKDIR /App
ENTRYPOINT [ "./TWCore.Object.Api" ]