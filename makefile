TFM := net7.0
BIN_DEBUG := bin/Debug/$(TFM)

format:
	xs format -sc -ic

setup:
	xs remote restore -user $(user) -password $(pass)

update:
	xs update all dotnet -debug -sc -ic

clean:
	xs clean -sc -ic

build:
	dotnet build --nologo -v q

test:
	dotnet test --nologo -v q

publish:
	xs publish all 0.1.0 -p 1

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
