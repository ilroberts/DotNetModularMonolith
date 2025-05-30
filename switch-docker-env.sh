#!/bin/bash

# Set the symlink targets for Docker Desktop and Rancher Desktop
DOCKER_DESKTOP_PATH="/Applications/Docker.app/Contents/Resources/cli-plugins"
RANCHER_DESKTOP_PATH="$HOME/.rd/bin"
CLI_PLUGINS_PATH="$HOME/.docker/cli-plugins"

# Make sure the CLI plugins directory exists
mkdir -p "$CLI_PLUGINS_PATH"

# Helper function
switch_symlinks() {
  local target_path=$1

  ln -sf "${target_path}/docker-buildx" "${CLI_PLUGINS_PATH}/docker-buildx"
  ln -sf "${target_path}/docker-compose" "${CLI_PLUGINS_PATH}/docker-compose"

  echo "Symlinks updated to point to ${target_path}"
  ls -l "${CLI_PLUGINS_PATH}"
}

if [[ "$1" == "docker" ]]; then
  switch_symlinks "$DOCKER_DESKTOP_PATH"
elif [[ "$1" == "rancher" ]]; then
  switch_symlinks "$RANCHER_DESKTOP_PATH"
else
  echo "Usage: $0 [docker|rancher]"
  exit 1
fi

