REGISTRY := registry.annium.com

install: install-xa install-xc install-xdomains install-xlink install-xmg install-xrest install-xws

uninstall: uninstall-xa uninstall-xc uninstall-xdomains uninstall-xlink uninstall-xmg uninstall-xrest uninstall-xws

install-xa install-xc install-xdomains install-xlink install-xmg install-xrest install-xws:
	./$(subst install-,,$@)/scripts/nix_install.sh

uninstall-xa uninstall-xc uninstall-xdomains uninstall-xlink uninstall-xmg uninstall-xrest uninstall-xws:
	./$(subst uninstall-,,$@)/scripts/nix_uninstall.sh


publish: publish-backuper publish-mbus-proxy publish-mbus-sink

publish-backuper:
	$(call publish,Backuper/src,Backuper.Api/app.dockerfile,backuper)

publish-mbus-proxy:
	$(call publish,MessageBus/src/MessageBus.Proxy,app.dockerfile,mbus.proxy)

publish-mbus-sink:
	$(call publish,MessageBus/src/MessageBus.Sink,app.dockerfile,mbus.sink)

define publish
	@$(eval context := $(1))
	@$(eval dockerfile := $(2))
	@$(eval tag := $(3))
	@docker build -t $(REGISTRY)/tools/$(tag) -f $(context)/$(dockerfile) $(context)
	@docker push $(REGISTRY)/tools/$(tag)
endef

.PHONY: $(MAKECMDGOALS)