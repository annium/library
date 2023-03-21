TFM := net6.0
BIN_DEBUG := bin/Debug/$(TFM)

demo-blazor-ant:
	cd web/demo/Demo.Blazor.Ant && dotnet watch run

demo-blazor-interop:
	cd web/demo/Demo.Blazor.Interop && dotnet watch run

demo-blazor-interop-prod:
	cd web/demo/Demo.Blazor.Interop && rm -rf dist && dotnet publish -c Release -o dist && dotnet serve --directory dist/wwwroot -p 5002 -q

demo-blazor-charts:
	cd web/demo/Demo.Blazor.Charts && dotnet watch run

demo-blazor-charts-prod:
	cd web/demo/Demo.Blazor.Charts && rm -rf dist && dotnet publish -c Release -o dist && dotnet serve --directory dist/wwwroot -p 5003 -q

.PHONY: $(MAKECMDGOALS)