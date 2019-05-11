PROJECT_NAME := backuper
TAG_PREFIX := registry.annium.com/$(PROJECT_NAME)

api-up:
	$(call up,$(subst -up,,$@))

api-down:
	$(call down,$(subst -down,,$@))

api-log:
	$(call log,$(subst -log,,$@))


publish-api:
	$(call publish,api,src,Backuper.Api/Dockerfile)


define publish
	@$(eval image := $(1))
	@$(eval context := $(2))
	@$(eval dockerfile := $(3))
	@docker build -t $(TAG_PREFIX)/$(image) -f $(context)/$(dockerfile) $(context)
	@docker push $(TAG_PREFIX)/$(image)
endef

define up
	@$(eval components := $(1))
	@$(eval files := $(foreach component,$(components),-f setup/$(env)/$(component).yml))
	$(call net-up)
	@COMPOSE_IGNORE_ORPHANS=true docker-compose -p $(PROJECT_NAME) $(files) up --build -d --force-recreate
endef

define down
	@$(eval components := $(1))
	@$(eval files := $(foreach component,$(components),-f setup/$(env)/$(component).yml))
	@COMPOSE_IGNORE_ORPHANS=true docker-compose -p $(PROJECT_NAME) $(files) stop
	@COMPOSE_IGNORE_ORPHANS=true docker-compose -p $(PROJECT_NAME) $(files) rm -f -v
	$(call net-down)
endef

define log
	@$(eval component := $(1))
	@docker logs -f $(PROJECT_NAME)_$(component)
endef

define net-up
	@if [[ $$(docker network ls -q -f "name=$(PROJECT_NAME)_net" | wc -l) -eq 0 ]]; then \
		docker network create --attachable --driver bridge --scope local $(PROJECT_NAME)_net; \
	fi
endef

define net-down
	@if [[ $$(docker network ls -q -f "name=$(PROJECT_NAME)_net" | wc -l) -gt 0 ]]; then \
		if [[ $$(docker network inspect -f "{{len .Containers}}" $(PROJECT_NAME)_net) -eq 0 ]]; then \
			docker network rm $(PROJECT_NAME)_net; \
		fi \
	fi
endef

.PHONY: $(MAKECMDGOALS)