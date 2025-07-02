#!/bin/bash

# OPC DA Client GitHub Push Script
# Repository: https://github.com/backspace-xdw/OpcdaClient.git

echo "=== Pushing OPC DA Client to GitHub ==="
echo ""
echo "This script will push the OPC DA Client project to your GitHub repository."
echo ""
echo "Please follow these steps:"
echo ""
echo "1. Make sure you have created the repository on GitHub:"
echo "   https://github.com/backspace-xdw/OpcdaClient"
echo ""
echo "2. If you haven't created it yet, go to GitHub and create a new repository named 'OpcdaClient'"
echo "   - Do NOT initialize with README, .gitignore, or license"
echo ""
echo "3. Use your GitHub Personal Access Token for authentication"
echo ""
echo "To push manually, run:"
echo ""
echo "   git remote set-url origin https://<your-github-token>@github.com/backspace-xdw/OpcdaClient.git"
echo "   git push -u origin main"
echo ""
echo "Replace <your-github-token> with your actual token (e.g., ghp_xxxxxxxxxxxx)"
echo ""
echo "Current git status:"
git status
echo ""
echo "Remote repository:"
git remote -v