server:
	dotnet run --project demo/XRest.Demo.Server

gen:
	dotnet run \
		dotnet gen \
		-s http://localhost:5000 \
		-ns XRest.Demo.Client.Api \
		-o demo/XRest.Demo.Client/Api \
		-trace \
		--project src/XRest

.PHONY: $(MAKECMDGOALS)