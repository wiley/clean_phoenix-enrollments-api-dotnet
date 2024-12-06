FROM mcr.microsoft.com/dotnet/sdk:6.0-bookworm-slim

RUN useradd -m -s $(which bash) developer
# Option '-m' to create home directory see https://askubuntu.com/a/393470
# How to set bash as shell, see comments in the same answer

# Create working directory and change ownership.
# Ownership will be preserved when mounting volume to /app
# See https://stackoverflow.com/a/56990338/411428
# and https://github.com/docker/compose/issues/3270#issuecomment-543603959a
RUN mkdir -p /app/run && chown developer:developer /app/run

COPY ./entrypoint.dev.sh /app/run/entrypoint.dev.sh

RUN apt-get update && apt-get install procps -y
RUN chmod +x /app/run/entrypoint.dev.sh

USER developer

ENTRYPOINT ["/app/run/entrypoint.dev.sh"]
