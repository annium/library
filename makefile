include ./lib.mk

setup:
	dotnet tool restore

format:
	dotnet csharpier format .
	xs format -sc -ic

format-full: format
	dotnet format style
	dotnet format analyzers

update:
	xs update all -sc -ic

clean:
	xs clean -sc -ic
	find . -type f -name '*.nupkg' | xargs rm

buildNumber?=0
build:
	dotnet build -c Release --nologo -v q -p:BuildNumber=$(buildNumber)

test:
	dotnet test -c Release --no-build --nologo -v q

publish: publish-doclint publish-xrest

publish-doclint publish-xrest:
	make -C $(subst publish-,,$@) publish

install: install-doclint install-xrest

install-doclint install-xrest:
	make -C $(subst install-,,$@) install

uninstall: uninstall-doclint uninstall-xrest

uninstall-doclint uninstall-xrest:
	make -C $(subst uninstall-,,$@) uninstall

publish-backuper:
	$(call publish-image,Backuper/src,Backuper.Api/app.dockerfile,backuper)

publish-mbus-proxy:
	$(call publish-image,MessageBus/src/MessageBus.Proxy,app.dockerfile,mbus.proxy)

publish-mbus-sink:
	$(call publish-image,MessageBus/src/MessageBus.Sink,app.dockerfile,mbus.sink)

.PHONY: $(MAKECMDGOALS)
