#!/usr/bin/env bash
set -e

dir=$(dirname $(dirname "${BASH_SOURCE[0]}"))/src/Annium.DocLint

echo "Compile."
dotnet pack $dir --configuration release --output .

if [ $(dotnet tool list -g | tail -n +3 | grep annium.doclint | wc -l) -eq 1 ]; then
    echo "Uninstall."
    dotnet tool uninstall -g annium.doclint
fi

echo "Install."
dotnet tool install -g annium.doclint --add-source .

echo "Cleanup."
find . -type f -name '*.nupkg' | xargs rm -f
