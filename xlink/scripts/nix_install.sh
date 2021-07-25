#!/usr/bin/env bash
set -e

dir=$(dirname $(dirname "${BASH_SOURCE[0]}"))/src/XLink

echo "Compile."
dotnet pack $dir --configuration release --output .

if [ $(dotnet tool list -g | tail -n +3 | grep xlink | wc -l) -eq 1 ]; then
    echo "Uninstall."
    dotnet tool uninstall -g xlink
fi

echo "Install."
dotnet tool install -g xlink --add-source .

echo "Cleanup."
find . -type f -name '*.nupkg' | xargs rm -f
