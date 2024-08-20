build:
	@dotnet build xdomains.csproj

run:
	@dotnet run -- query -f

zones:
	@cat raw.txt | cut -d$$'\t' -f 1 > zones.txt

.PHONY: $(MAKECMDGOALS)