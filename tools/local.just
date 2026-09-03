# Helpers only the tools group needs. Imported by the root justfile.

# installs a tool project globally from local sources, replacing whatever is installed
_tool-install project:
    #!/usr/bin/env bash
    set -e
    toolId=$(basename {{project}})
    packageVersion=$(dotnet tool run versioning get-version -v $(cat version))
    out=$(mktemp -d)
    echo "--- install $toolId @ $packageVersion ---"
    dotnet pack {{project}} -c Release -o "$out" -p:PackageVersion=$packageVersion --nologo -v q
    if dotnet tool list -g | tail -n +3 | awk '{print $1}' | grep -qix "$toolId"; then
        dotnet tool uninstall -g "$toolId"
    fi
    # --source, not --add-source: this version is usually published too, and the local build must win
    dotnet tool install -g "$toolId" --source "$out" --version "$packageVersion"
    rm -rf "$out"

_tool-uninstall project:
    #!/usr/bin/env bash
    set -e
    toolId=$(basename {{project}})
    echo "--- uninstall $toolId ---"
    dotnet tool uninstall -g "$toolId"
