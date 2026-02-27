# Use the official Dev Containers .NET image for .NET 10
ARG VARIANT=10.0
FROM mcr.microsoft.com/devcontainers/dotnet:${VARIANT}

# Become root to install certs/tools
USER root

# Ensure CA utilities are present (usually already are, but this is cheap)
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl git nodejs npm \
    && rm -rf /var/lib/apt/lists/*

# Copy Zscaler root cert into the system trust store BEFORE downloading anything
# Make sure this file exists in .devcontainer/certs/zscaler.crt
COPY .devcontainer/certs/kainos_zscaler_2026.pem /usr/local/share/ca-certificates/zscaler.crt

# Update the CA store so wget/curl/dotnet/git all trust Zscaler
RUN update-ca-certificates

# Install .NET 8 SDK alongside .NET 10 (now with Zscaler cert trusted)
RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh \
    && chmod +x dotnet-install.sh \
    && ./dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet \
    && rm dotnet-install.sh

# Verify both SDKs are installed
RUN dotnet --list-sdks

# Set zsh as the default shell for the vscode user
RUN chsh -s /usr/bin/zsh vscode

# Drop back to the non-root dev user used by the devcontainers images
USER vscode

WORKDIR /workspaces
