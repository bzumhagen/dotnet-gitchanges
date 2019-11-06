git config commit.template .\git\.gitmessage
Copy-Item -Path .\git\hooks\prepare-commit-msg -Destination .\.git\hooks\prepare-commit-msg -Force