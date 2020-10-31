#!/usr/bin/env bash
set -e

dir=$(dirname $(dirname "${BASH_SOURCE[0]}"))/src/Xc

echo "Compile."
dotnet pack $dir --configuration release --output .

if [ $(dotnet tool list -g | tail -n +3 | grep xc | wc -l) -eq 1 ]; then
    echo "Uninstall."
    dotnet tool uninstall -g xc
fi

echo "Install."
dotnet tool install -g xc --add-source .

echo "Cleanup."
find . -type f -name '*.nupkg' | xargs rm -f
