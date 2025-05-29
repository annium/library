REGISTRY := registry.annium.com

define publish-image
	@$(eval context := $(1))
	@$(eval dockerfile := $(2))
	@$(eval tag := $(3))
	@docker build -t $(REGISTRY)/tools/$(tag) -f $(context)/$(dockerfile) $(context)
	@docker push $(REGISTRY)/tools/$(tag)
endef

define publish-package
	@$(eval project := $(1))
	cd $(project) && dotnet pack --no-build -o . -c Release -p:SymbolPackageFormat=snupkg
	cd $(project) && dotnet nuget push "*.nupkg" --source https://api.nuget.org/v3/index.json --api-key $(shell cat ../.xs.credentials)
	cd $(project) && find . -type f -name '*.nupkg' | xargs rm
endef

define nix-install
	./scripts/nix_install.sh
endef

define nix-uninstall
	./scripts/nix_uninstall.sh
endef
