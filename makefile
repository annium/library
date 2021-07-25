REGISTRY := registry.annium.com

install:
	./tcplog/scripts/nix_install.sh
	./xc/scripts/nix_install.sh
	./xdomains/scripts/nix_install.sh
	./xlink/scripts/nix_install.sh
	./xmg/scripts/nix_install.sh
	./xrest/scripts/nix_install.sh
	./xws/scripts/nix_install.sh

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