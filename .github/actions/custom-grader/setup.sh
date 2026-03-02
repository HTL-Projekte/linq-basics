#!/bin/bash
set -euo pipefail

# --- 1. Install .NET 9.x SDK ---
DOTNET_VERSION="9.0.305"
DOTNET_ROOT="/usr/share/dotnet"

echo "Setting up .NET SDK $DOTNET_VERSION..."

# Install dotnet using GitHub setup script
# This works in CI environments without preinstalled .NET
if ! command -v dotnet &>/dev/null || [[ "$(dotnet --version)" != 9.* ]]; then
    echo "Installing .NET SDK $DOTNET_VERSION..."
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh --version $DOTNET_VERSION --install-dir $DOTNET_ROOT
    export DOTNET_ROOT=$DOTNET_ROOT
    export PATH=$DOTNET_ROOT:$PATH
else
    echo ".NET SDK already installed: $(dotnet --version)"
fi

# Verify installation
dotnet --info

# --- 2. Restore NuGet packages ---
echo "Restoring NuGet packages..."
dotnet restore

# --- 3. Build the project ---
echo "Building the project..."
dotnet build --no-restore

echo "Setup and build completed successfully."
