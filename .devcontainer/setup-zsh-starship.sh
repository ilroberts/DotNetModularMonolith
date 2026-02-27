#!/usr/bin/env bash
set -euo pipefail

USER_HOME="${HOME:-/home/vscode}"

echo "Configuring zsh + starship for user at ${USER_HOME}"

# Ensure config directory exists
mkdir -p "${USER_HOME}/.config"

STARSHIP_CONFIG="${USER_HOME}/.config/starship.toml"
ZSHRC="${USER_HOME}/.zshrc"

#######################################
# Write a default starship config if none exists
#######################################
if [ ! -f "${STARSHIP_CONFIG}" ]; then
  cat > "${STARSHIP_CONFIG}" << 'EOF'
# Minimal-ish Starship config example – tweak as you like

add_newline = true

[character]
success_symbol = "[❯](bold green)"
error_symbol = "[❯](bold red)"

[git_branch]
symbol = " "
truncation_length = 32

[dotnet]
symbol = " "
detect_extensions = ["csproj", "fsproj", "vbproj", "sln"]

[cmd_duration]
min_time = 2000
show_milliseconds = true
EOF
  echo "Created ${STARSHIP_CONFIG}"
else
  echo "Existing ${STARSHIP_CONFIG} detected; leaving it alone."
fi

#######################################
# Ensure .zshrc exists and initializes starship
#######################################
if [ ! -f "${ZSHRC}" ]; then
  cat > "${ZSHRC}" << 'EOF'
# Basic zsh config – extend as needed

export EDITOR="nvim"

# History settings
HISTSIZE=5000
SAVEHIST=5000
setopt hist_ignore_dups share_history

# Aliases
alias ll='ls -alF'
alias la='ls -A'
alias l='ls -CF'

# Init starship prompt
eval "$(starship init zsh)"
EOF
  echo "Created ${ZSHRC}"
else
  # Append starship init if not already present
  if ! grep -q 'starship init zsh' "${ZSHRC}"; then
    cat >> "${ZSHRC}" << 'EOF'

# Init starship prompt (added by setup-zsh-starship.sh)
eval "$(starship init zsh)"
EOF
    echo "Updated ${ZSHRC} to include starship init."
  else
    echo ".zshrc already contains starship init; leaving as-is."
  fi
fi

echo "zsh + starship setup complete."
