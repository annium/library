include ../lib.mk

publish:
	$(call publish-package,src/Annium.XRest.Core)
	$(call publish-package,src/Annium.XRest)
	$(call publish-package,src/sources/Annium.XRest.Sources.Shared)
	$(call publish-package,src/sources/Annium.XRest.Sources.AspNetCore)

install:
	$(call nix-install)

uninstall:
	$(call nix-uninstall)

server:
	dotnet run --project demo/Annium.XRest.Demo.Server

gen:
	dotnet run \
		cs gen \
		-s http://localhost:5000 \
		-ns Annium.XRest.Demo.Client.Api \
		-o demo/Annium.XRest.Demo.Client/Api \
		-trace \
		--project src/Annium.XRest

.PHONY: $(MAKECMDGOALS)