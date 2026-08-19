registry := "registry.annium.com"

_publish-image context dockerfile tag:
    @docker build -t {{registry}}/tools/{{tag}} -f {{context}}/{{dockerfile}} {{context}}
    @docker push {{registry}}/tools/{{tag}}

# packs and pushes one or more projects of a subproject, computing the package version once
_publish-packages +projects:
    #!/usr/bin/env bash
    set -e
    packageVersion=$(dotnet tool run versioning get-version -v $(cat version))
    # read before cd: the credentials file lives next to this justfile, not next to the project
    apiKey=$(cat .xs.credentials)
    for project in "$@"; do
        echo "--- publish $project @ $packageVersion ---"
        (
            cd "$project"
            dotnet pack --no-build -o . -c Release -p:SymbolPackageFormat=snupkg -p:PackageVersion=$packageVersion
            dotnet nuget push "*.nupkg" --source https://api.nuget.org/v3/index.json --api-key "$apiKey" --skip-duplicate
            find . -type f \( -name '*.nupkg' -o -name '*.snupkg' \) -delete
        )
    done

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
