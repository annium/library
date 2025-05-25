setup:
	dotnet tool restore

format:
	dotnet csharpier format .
	xx format -sc -ic

format-full: format
	dotnet format style
	dotnet format analyzers

update:
	xx update all -sc -ic

clean:
	xx clean -sc -ic
	find . -type f -name '*.nupkg' | xargs rm

buildNumber?=0
build:
	dotnet build -c Release --nologo -v q -p:BuildNumber=$(buildNumber)

test:
	dotnet test -c Release --no-build --nologo -v q

pack:
	dotnet pack --no-build -o . -c Release -p:SymbolPackageFormat=snupkg

publish:
	dotnet nuget push "*.nupkg" --source https://api.nuget.org/v3/index.json --api-key $(shell cat .xx.credentials)
	find . -type f -name '*.nupkg' | xargs rm

demo-blazor-ant:
	cd web/demo/Demo.Blazor.Ant && dotnet watch run

demo-blazor-ant-prod:
	cd web/demo/Demo.Blazor.Ant && rm -rf dist && dotnet publish -c Release -o dist && dotnet serve --directory dist/wwwroot -p 5004 -q

demo-blazor-interop:
	cd web/demo/Demo.Blazor.Interop && dotnet watch run

demo-blazor-interop-prod:
	cd web/demo/Demo.Blazor.Interop && rm -rf dist && dotnet publish -c Release -o dist && dotnet serve --directory dist/wwwroot -p 5002 -q

demo-blazor-charts:
	cd web/demo/Demo.Blazor.Charts && dotnet watch run

demo-blazor-charts-prod:
	cd web/demo/Demo.Blazor.Charts && rm -rf dist && dotnet publish -c Release -o dist && dotnet serve --directory dist/wwwroot -p 5003 -q

.PHONY: $(MAKECMDGOALS)
