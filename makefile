include ./lib.mk

setup:
	$(call header)
	dotnet tool restore

format:
	$(call header)
	dotnet tool run csharpier format . --config-path $(shell pwd)/.editorconfig
	dotnet tool run xs format -sc -ic

format-full: format
	$(call header)
	dotnet format style
	dotnet format analyzers

ensure-no-changes:
	$(call header)
	@if [[ -n "$$(git status --porcelain)" ]]; then \
		echo "Changes detected:"; \
		git status; \
		git --no-pager diff --no-color --exit-code; \
	fi

update:
	$(call header)
	dotnet tool list --format json | jq -r '.data[] | "\(.packageId)"' | xargs -I% dotnet tool install %
	dotnet tool run xs update all -sc -ic

clean:
	$(call header)
	dotnet tool run xs clean -sc -ic
	find . -type f -name '*.nupkg' | xargs -I% rm %

build:
	$(call header)
	$(call get-package-version)
	dotnet build -c Release --nologo -v q -p:PackageVersion=$(packageVersion)

test:
	$(call header)
	dotnet test -c Release --no-build --nologo --logger "trx;LogFilePrefix=test-results.trx"

publish: publish-doclint publish-versioning publish-xrest

publish-doclint publish-versioning publish-xrest:
	$(call header)
	$(call get-package-version)
	make -C $(subst publish-,,$@) publish packageVersion=$(packageVersion)

install: install-doclint install-versioning install-xrest

install-doclint install-versioning install-xrest:
	$(call header)
	make -C $(subst install-,,$@) install

uninstall: uninstall-doclint uninstall-versioning uninstall-xrest

uninstall-doclint uninstall-versioning uninstall-xrest:
	$(call header)
	make -C $(subst uninstall-,,$@) uninstall

publish-backuper:
	$(call header)
	$(call publish-image,Backuper/src,Backuper.Api/app.dockerfile,backuper)

publish-mbus-proxy:
	$(call header)
	$(call publish-image,MessageBus/src/MessageBus.Proxy,app.dockerfile,mbus.proxy)

publish-mbus-sink:
	$(call header)
	$(call publish-image,MessageBus/src/MessageBus.Sink,app.dockerfile,mbus.sink)

# CI
ci-merge-request-short:
	$(call header)
	make setup
	make format
	make ensure-no-changes
	make clean
	make build

ci-merge-request-full:
	$(call header)
	make setup
	make format
	make ensure-no-changes
# 	make docs-lint
	make clean
	make build
	make test
# 	make docs-build

ci-release:
	$(call header)
	make setup
	make format
	make ensure-no-changes
	make ci-set-package-version
	make clean
	make build
# 	make docs-build
	make publish apiKey=$(apiKey)
	make ci-push-tag repository=$(repository) githubToken=$(githubToken)
	echo "Release complete"

ci-set-package-version:
	$(call header)
#	git config user.name "it"
#	git config user.email "it@annium.com"
	dotnet tool run versioning set-version -v $(shell cat version)

ci-push-tag:
	$(call header)
	$(call get-package-version)
#	git remote set-url origin https://x-access-token:$(githubToken)@github.com/$(repository).git
	git push origin v$(packageVersion)


define header
	@echo "=== $@ ==="
endef

define get-package-version
	$(eval packageVersion := $(shell dotnet tool run versioning get-version -v $(shell cat version)))
endef


.PHONY: $(MAKECMDGOALS)
