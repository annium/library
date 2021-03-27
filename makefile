REGISTRY := registry.annium.com

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