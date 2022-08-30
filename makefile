TFM := net6.0
BIN_DEBUG := bin/Debug/$(TFM)

demo-blazor-charts:
	cd web/demo/Demo.Blazor.Charts && ../../tools/run_blazor.sh

.PHONY: $(MAKECMDGOALS)