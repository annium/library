#!/usr/bin/env bash

dir=$(dirname $(dirname "${BASH_SOURCE[0]}"))
root=/usr/local/share/xdomains
entry=/usr/local/bin/xdomains

echo "Compile."
dotnet publish -c release -r osx-x64 -o $root $dir

# prepare launcher
echo "Write launcher."
rm -f $entry
echo '#!/usr/bin/env sh' > $entry
echo $root'/xdomains $@' >> $entry
chmod +x $entry