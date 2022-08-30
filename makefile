TFM := net6.0
BIN_DEBUG := bin/Debug/$(TFM)

demo-blazor-charts:
	cd web/demo/Demo.Blazor.Charts && ../../tools/run_blazor.sh

demo-blazor-charts-prod:
	cd web/demo/Demo.Blazor.Charts && rm -rf dist && dotnet publish -c Release -o dist && dotnet serve --directory dist/wwwroot -p 5001 -q

.PHONY: $(MAKECMDGOALS)