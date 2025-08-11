include ../lib.mk

publish:
	$(call publish-package,src/Annium.Versioning,$(packageVersion))

install:
	$(call nix-install)

uninstall:
	$(call nix-uninstall)

.PHONY: $(MAKECMDGOALS)