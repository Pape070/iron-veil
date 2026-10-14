#!/usr/bin/env bash
# Creates the Iron Veil GitHub repository, pushes this branch, and (when the
# GitHub CLI is available and authenticated) creates a project board with the
# game's title. No credentials are stored in this repository: the script reads
# GH_TOKEN from the environment, or reuses an existing `gh auth login` session.
#
# Usage:
#   GH_TOKEN=ghp_xxx tools/publish_to_github.sh [--public] [--name iron-veil] [--owner <user|org>]
#
# What it does, in order:
#   1. Verifies the working tree is committed.
#   2. Creates the repository (default: private) if it does not exist yet.
#   3. Adds/updates the `origin` remote and pushes the current branch as `main`.
#   4. Creates a GitHub project board named after the game, when `gh` is present.

set -euo pipefail

REPO_NAME="iron-veil"
REPO_DESCRIPTION="Iron Veil - a 2D side-scrolling run-and-gun built on Unity 6.3 LTS"
PROJECT_TITLE="Iron Veil"
VISIBILITY="--private"
OWNER=""

while [[ $# -gt 0 ]]; do
    case "$1" in
        --public) VISIBILITY="--public"; shift ;;
        --name) REPO_NAME="$2"; shift 2 ;;
        --owner) OWNER="$2"; shift 2 ;;
        --project-title) PROJECT_TITLE="$2"; shift 2 ;;
        -h|--help) sed -n '2,20p' "$0"; exit 0 ;;
        *) echo "unknown argument: $1" >&2; exit 2 ;;
    esac
done

cd "$(dirname "$0")/.."
ROOT="$(pwd)"

if ! command -v git >/dev/null 2>&1; then
    echo "git is required" >&2
    exit 1
fi

if [[ -n "$(git status --porcelain)" ]]; then
    echo "There are uncommitted changes. Commit them first:" >&2
    git status --short >&2
    exit 1
fi

if [[ -z "$OWNER" ]]; then
    if command -v gh >/dev/null 2>&1; then
        OWNER="$(gh api user --jq .login 2>/dev/null || true)"
    fi
    if [[ -z "$OWNER" && -n "${GH_TOKEN:-}" ]]; then
        OWNER="$(curl -fsSL -H "Authorization: Bearer $GH_TOKEN" \
            -H "Accept: application/vnd.github+json" \
            https://api.github.com/user | sed -n 's/.*"login": *"\([^"]*\)".*/\1/p' | head -1)"
    fi
fi

if [[ -z "$OWNER" ]]; then
    echo "Could not determine the GitHub account." >&2
    echo "Either run 'gh auth login' or pass --owner <user> with GH_TOKEN set." >&2
    exit 1
fi

echo "Repository: $OWNER/$REPO_NAME"
echo "Branch:     $(git rev-parse --abbrev-ref HEAD) -> main"
echo

create_repo_with_gh() {
    gh repo view "$OWNER/$REPO_NAME" >/dev/null 2>&1 && return 1
    gh repo create "$OWNER/$REPO_NAME" $VISIBILITY \
        --description "$REPO_DESCRIPTION" >/dev/null
    return 0
}

create_repo_with_api() {
    if [[ -z "${GH_TOKEN:-}" ]]; then
        return 2
    fi
    local exists
    exists="$(curl -s -o /dev/null -w '%{http_code}' \
        -H "Authorization: Bearer $GH_TOKEN" \
        -H "Accept: application/vnd.github+json" \
        "https://api.github.com/repos/$OWNER/$REPO_NAME")"
    if [[ "$exists" == "200" ]]; then
        return 1
    fi

    local private="true"
    [[ "$VISIBILITY" == "--public" ]] && private="false"

    curl -fsSL -X POST \
        -H "Authorization: Bearer $GH_TOKEN" \
        -H "Accept: application/vnd.github+json" \
        https://api.github.com/user/repos \
        -d "{\"name\":\"$REPO_NAME\",\"description\":\"$REPO_DESCRIPTION\",\"private\":$private,\"auto_init\":false}" \
        >/dev/null
    return 0
}

CREATED=0
if command -v gh >/dev/null 2>&1; then
    if create_repo_with_gh; then CREATED=1; fi
else
    set +e
    create_repo_with_api
    status=$?
    set -e
    if [[ $status -eq 0 ]]; then CREATED=1; fi
    if [[ $status -eq 2 ]]; then
        echo "Install the GitHub CLI, or export GH_TOKEN so the repository can be created." >&2
        exit 1
    fi
fi

if [[ $CREATED -eq 1 ]]; then
    echo "Created $OWNER/$REPO_NAME"
else
    echo "$OWNER/$REPO_NAME already exists; pushing to it"
fi

if git remote get-url origin >/dev/null 2>&1; then
    git remote set-url origin "https://github.com/$OWNER/$REPO_NAME.git"
else
    git remote add origin "https://github.com/$OWNER/$REPO_NAME.git"
fi

git push -u origin "HEAD:main"
echo "Pushed to https://github.com/$OWNER/$REPO_NAME"

# Optional: a project board named after the game (needs the `project` scope).
if command -v gh >/dev/null 2>&1; then
    if gh project create --owner "$OWNER" --title "$PROJECT_TITLE" >/dev/null 2>&1; then
        echo "Created the GitHub project board \"$PROJECT_TITLE\""
    else
        echo "Skipped the project board (needs 'gh auth refresh -s project')."
    fi
fi

echo
echo "Done. Next: open the project in Unity and run"
echo "  Iron Veil > Setup > Apply Project Settings"
echo "  Iron Veil > Setup > Create Scenes"
