PROJECT_NAME := id
TAG_PREFIX := registry.annium.com/$(PROJECT_NAME)

db-up api-up:
	$(call up,$(subst -up,,$@))

db-down api-down:
	$(call down,$(subst -down,,$@))

db-log api-log:
	$(call log,$(subst -log,,$@))


db-drop db-update migrations-add migrations-list migrations-remove:
	@cd server && tools/ef-$@.sh

publish-api:
	@cp $$(find $$(dirname $$(realpath $$(which dotnet)))/sdk -type f -name ef.dll | grep netcoreapp2.2) server/src
	$(call publish,api,server/src,Annium.Id.Api/Dockerfile)
	@rm -f server/src/ef.dll


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