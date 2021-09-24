REGISTRY := registry.annium.com

install: install-xa install-xc install-xdomains install-xlink install-xmg install-xrest install-xws

uninstall: uninstall-xa uninstall-xc uninstall-xdomains uninstall-xlink uninstall-xmg uninstall-xrest uninstall-xws

install-xa install-xc install-xdomains install-xlink install-xmg install-xrest install-xws:
	./$(subst install-,,$@)/scripts/nix_install.sh

uninstall-xa uninstall-xc uninstall-xdomains uninstall-xlink uninstall-xmg uninstall-xrest uninstall-xws:
	./$(subst uninstall-,,$@)/scripts/nix_uninstall.sh


publish:
	$(call publish,MessageBus/src/MessageBus.Proxy,mbus.proxy)
	$(call publish,MessageBus/src/MessageBus.Sink,mbus.sink)

define publish
	@$(eval context := $(1))
	@$(eval tag := $(2))
	@docker build -t $(REGISTRY)/tools/$(tag) -f $(context)/app.dockerfile $(context)
	@docker push $(REGISTRY)/tools/$(tag)
endef

.PHONY: $(MAKECMDGOALS)