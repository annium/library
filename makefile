REGISTRY := registry.annium.com

format:
	xs format -sc -ic
	dotnet csharpier .

setup:
	xs remote restore -user $(user) -password $(pass)
	dotnet tool restore

update:
	xs update all -sc -ic

clean:
	xs clean -sc -ic

build:
	dotnet build -c Release --nologo -v q

test:
	dotnet test -c Release --no-build --nologo -v q

publish:
	make publish-tools

install-all: install-xa install-xc install-xdb install-xdomains install-xfiles install-xlink install-xlog install-xmg install-xrest install-xws

uninstall-all: uninstall-xa uninstall-xc uninstall-xdb uninstall-xdomains uninstall-xfiles uninstall-xlink uninstall-xlog uninstall-xmg uninstall-xrest uninstall-xws

install-xa install-xc install-xdb install-xdomains install-xfiles install-xlink install-xlog install-xmg install-xrest install-xws:
	./$(subst install-,,$@)/scripts/nix_install.sh

uninstall-xa uninstall-xc uninstall-xdb uninstall-xdomains uninstall-xfiles uninstall-xlink uninstall-xlog uninstall-xmg uninstall-xrest uninstall-xws:
	./$(subst uninstall-,,$@)/scripts/nix_uninstall.sh

publish-all: publish-images publish-tools

publish-images: publish-backuper publish-mbus-proxy publish-mbus-sink

publish-backuper:
	$(call publish,Backuper/src,Backuper.Api/app.dockerfile,backuper)

publish-mbus-proxy:
	$(call publish,MessageBus/src/MessageBus.Proxy,app.dockerfile,mbus.proxy)

publish-mbus-sink:
	$(call publish,MessageBus/src/MessageBus.Sink,app.dockerfile,mbus.sink)

publish-tools:
	$(call publish-package,xdb/src/Xdb.Core/Xdb.Core.csproj)
	$(call publish-package,xdb/src/Xdb.PostgreSql/Xdb.PostgreSql.csproj)
	$(call publish-package,xrest/src/XRest.Core/XRest.Core.csproj)
	$(call publish-package,xrest/src/sources/XRest.Sources.AspNetCore/XRest.Sources.AspNetCore.csproj)
	$(call publish-package,xrest/src/sources/XRest.Sources.Shared/XRest.Sources.Shared.csproj)

define publish
	@$(eval context := $(1))
	@$(eval dockerfile := $(2))
	@$(eval tag := $(3))
	@docker build -t $(REGISTRY)/tools/$(tag) -f $(context)/$(dockerfile) $(context)
	@docker push $(REGISTRY)/tools/$(tag)
endef

define publish-package
	@$(eval project := $(1))
	@$(eval registry := $(2))
	dotnet pack $(project) --no-build -o . -c Release -p:PackageVersion=0.1.0 -p:SymbolPackageFormat=snupkg
	dotnet nuget push "*.nupkg" --source https://dotnet.pkg.annium.com/v3/index.json --api-key $(shell cat .xs.credentials)
	find . -type f -name '*.nupkg' | xargs rm
endef

.PHONY: $(MAKECMDGOALS)
