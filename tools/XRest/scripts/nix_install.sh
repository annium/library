#!/usr/bin/env bash
set -e

dir=$(dirname $(dirname "${BASH_SOURCE[0]}"))

echo "Compile."
dotnet pack $dir --configuration release --output .

if [ $(dotnet tool list -g | tail -n +3 | grep xrest | wc -l) -eq 1 ]; then
    echo "Uninstall."
    dotnet tool uninstall -g xrest
fi

echo "Install."
dotnet tool install -g xrest --add-source .

echo "Cleanup."
find . -type f -name '*.nupkg' | xargs rm -f
