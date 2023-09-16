REGISTRY := registry.annium.com

format:
	xs format -sc -ic

setup:
	xs remote restore -user $(user) -password $(pass)

update:
	xs update all -sc -ic

clean:
	xs clean -sc -ic

build:
	dotnet build --nologo -v q

test:
	dotnet test --nologo -v q

publish:
	make publish-tools

install-all: install-xa install-xc install-xdb install-xdomains install-xlink install-xmg install-xrest install-xws

uninstall-all: uninstall-xa uninstall-xc uninstall-xdb uninstall-xdomains uninstall-xlink uninstall-xmg uninstall-xrest uninstall-xws

install-xa install-xc install-xdb install-xdomains install-xlink install-xmg install-xrest install-xws:
	./$(subst install-,,$@)/scripts/nix_install.sh

uninstall-xa uninstall-xc uninstall-xdb uninstall-xdomains uninstall-xlink uninstall-xmg uninstall-xrest uninstall-xws:
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
	xs publish xdb.core 0.1.0
	xs publish xrest.core 0.1.0
	xs publish xrest.sources 0.1.0

define publish
	@$(eval context := $(1))
	@$(eval dockerfile := $(2))
	@$(eval tag := $(3))
	@docker build -t $(REGISTRY)/tools/$(tag) -f $(context)/$(dockerfile) $(context)
	@docker push $(REGISTRY)/tools/$(tag)
endef

.PHONY: $(MAKECMDGOALS)
