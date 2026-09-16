.PHONY: help build build-discord build-all clean test mutate compose stop stop-volumes logs token restart

# Variables
COMPOSE_FILE = deploy/docker-compose.yaml

help: ## Show this help message
	@echo "Available targets:"
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  %-20s %s\n", $$1, $$2}'

# Build
build: ## Build the Solution in Release mode
	dotnet build --configuration Release

build-discord: ## Build Yappr.Discord Docker image (yappr-discord:latest)
	docker build -f src/Yappr.Discord/Dockerfile -t yappr-discord:latest .

build-mcp: ## Build Yappr.Mcp.Ollama Docker image (yappr-mcp:latest)
	docker build -f src/Yappr.Mcp.Ollama/Dockerfile -t yappr-mcp:latest .

build-all: build-discord build-mcp ## Builds all Docker images

# Test
test: ## Run Unit and Integration Tests
	dotnet test --project tests/unit/Yappr.Unit.Tests/Yappr.Unit.Tests.csproj --configuration Release --no-build
	dotnet test --project tests/unit/Yappr.Discord.Unit.Tests/Yappr.Discord.Unit.Tests.csproj --configuration Release --no-build
	dotnet test --project tests/unit/Yappr.Mcp.Ollama.Unit.Tests/Yappr.Mcp.Ollama.Unit.Tests.csproj --configuration Release --no-build
	dotnet test --project tests/integration/Yappr.Integration.Tests/Yappr.Integration.Tests.csproj --configuration Release --no-build
	dotnet test --project tests/integration/Yappr.Mcp.Ollama.Integration.Tests/Yappr.Mcp.Ollama.Integration.Tests.csproj --configuration Release --no-build

mutate: ## Run Stryker Mutation Testing
	dotnet stryker --config-file tests/unit/Yappr.Unit.Tests/stryker-config.json

# Docker
compose: ## Composes Yappr Docker images
	docker compose --file $(COMPOSE_FILE) up --detach

stop: ## Stops Yappr Docker images
	docker compose --file $(COMPOSE_FILE) down

stop-volumes: ## Stops Yappr Docker images and removes volumes
	docker compose --file $(COMPOSE_FILE) down --volumes

logs: ## Shows logs for Yappr Docker images
	@docker compose --file $(COMPOSE_FILE) logs --follow

restart: stop-volumes build-all compose ## Rebuild Yappr Docker images and restart Containers

clean: stop-volumes ## Clean Yappr build artifacts and remove Yappr Docker images
	docker rmi yappr-discord:latest 2>/dev/null || true
	dotnet clean
	rm -rf **/bin **/obj TestResults

# Utility
token: ## Generate a random token for API keys (.env file)
	@pwsh -Command '-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | ForEach-Object {[char]$$_})'
