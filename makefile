run:
	@dotnet clean -c Release
	@dotnet build -c Release
	@dotnet clean -c Release
	@dotnet publish -c Release --os linux --arch x64 /t:PublishContainer
	@docker compose -f compose.yml up

stop:
	@docker compose -f compose.yml down