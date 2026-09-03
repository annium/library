set shell := ["bash", "-cu"]
set positional-arguments

# tools/local.just holds the helpers only that group needs
import 'tools/local.just'

[private]
default:
    @just --list

# base

setup:
    @echo "=== $0 ==="
    dotnet tool restore

format:
    @echo "=== $0 ==="
    dotnet tool run csharpier format . --config-path $(pwd)/.editorconfig
    dotnet tool run xs format -sc -ic

format-full: format
    @echo "=== $0 ==="
    dotnet format style
    dotnet format analyzers

ensure-no-changes:
    #!/usr/bin/env bash
    set -e
    echo "=== ensure-no-changes ==="
    if [[ -n "$(git status --porcelain)" ]]; then
        echo "Changes detected:"
        git status --short
        git --no-pager diff --no-color HEAD
        exit 1
    fi

clean:
    @echo "=== $0 ==="
    dotnet tool run xs clean -sc -ic
    find . -type f \( -name '*.nupkg' -o -name '*.snupkg' \) -delete

# build

build:
    @just _build Annium

build-core:
    @just _build core/core

build-server:
    @just _build server/server

build-client:
    @just _build client/client

build-finance:
    @just _build finance/finance

build-integrations:
    @just _build integrations/integrations

build-tools:
    @just _build tools/tools

# test
#
# every recipe here runs with --no-build, so build first or you test a stale binary.

# the default run: every group, and in finance only the block that touches nothing outside the
# process. See test-finance-read / test-finance-write for the rest.

# every group; finance limited to its offline block
test: test-core test-server test-client test-finance test-integrations test-tools

test-core:
    @just _test core/core

test-server:
    @just _test server/server

test-client:
    @just _test client/client

test-integrations:
    @just _test integrations/integrations

test-tools:
    @just _test tools/tools

# a test belongs to a block by an xunit trait on its class or a base of it - see TestBlock. Absence
# means offline, which is the safe default in the direction that matters: a test nobody marked joins
# the block that always runs rather than the one that never does. The trait decides selection only;
# what keeps a trading test from reaching the exchange is its own SkipUnless gate.

# finance, offline block only - touches nothing outside the process
test-finance:
    @echo "=== test finance (offline) ==="
    dotnet test --solution finance/finance.slnx -c Release --no-build --report-xunit-trx \
        -- --filter-not-trait "block=read" --filter-not-trait "block=write"

# --ignore-exit-code 8 because most projects hold none of these tests, and a project that matched
# nothing otherwise fails the whole run with "zero tests ran" - a green run reporting failure.

# finance, read block - REAL exchanges and REAL accounts, mutates nothing
test-finance-read:
    @echo "=== test finance (read) ==="
    dotnet test --solution finance/finance.slnx -c Release --no-build --report-xunit-trx \
        -- --filter-trait "block=read" --ignore-exit-code 8

# Run it alone, on an account whose state you have just looked at, and never alongside anything else
# touching the same one. Check the position mode, that no position exists on the test symbol the
# fixture would close as cleanup, and the available margin.
#
# This recipe has no CI counterpart, deliberately: that pre-flight is something a person does, and a
# cancelled runner leaves the cleanup unrun and the position open.

# finance, write block - PLACES REAL ORDERS on a real account. Run it alone
test-finance-write:
    @echo "=== test finance (write) ==="
    dotnet test --solution finance/finance.slnx -c Release --no-build --report-xunit-trx \
        -- --filter-trait "block=write" --ignore-exit-code 8

# package

pack:
    #!/usr/bin/env bash
    set -e
    echo "=== pack ==="
    packageVersion=$(dotnet tool run versioning get-version -v $(cat version))
    dotnet pack Annium.slnx --no-build -o . -c Release -p:SymbolPackageFormat=snupkg -p:PackageVersion=$packageVersion

publish apiKey:
    @echo "=== $0 ==="
    dotnet nuget push "*.nupkg" --source https://api.nuget.org/v3/index.json --api-key "$1" --skip-duplicate
    find . -type f \( -name '*.nupkg' -o -name '*.snupkg' \) -delete

# docs

# Two exclusions, of different kinds.
#
# The Obsolete Telegram project is exempt permanently: it is IsPackable=false, kept only for existing
# consumers.
#
# The tools group is exempt temporarily. It never ran this lint - the tools repository had no docs
# pipeline and dropped the step from its ci-* recipes - so extending the check to it surfaced 640
# undocumented members at once. That is a backlog to work off, not a reason to weaken the rule
# everywhere; the exclusion goes when the backlog does.

# enforce XML documentation on every public member
docs-lint:
    @echo "=== $0 ==="
    dotnet tool run doclint lint -w . -i '**/*.cs' -e '**/obj/**/*.cs' \
        -e 'integrations/Social/src/Annium.Social.Telegram.Obsolete/**/*.cs' \
        -e 'tools/**/*.cs'

docs-clean:
    @echo "=== $0 ==="
    rm -rf _site api

docs-metadata:
    @echo "=== $0 ==="
    dotnet tool run docfx metadata docfx.json

docs-build:
    @echo "=== $0 ==="
    dotnet tool run docfx docfx.json

docs-serve:
    @echo "=== $0 ==="
    dotnet tool run docfx serve _site

docs-watch:
    @echo "=== $0 ==="
    dotnet tool run docfx docfx.json --serve

# ci
#
# The pipeline shape lives in .github/workflows; what each stage does lives here. The two test groups
# together cover all 259 projects, so there is no separate compile gate: whatever fails to build fails
# in the group that owns it.

# format, tree-is-clean guard and doc lint - needs no build at all
ci-check:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-check ==="
    just setup
    just format
    just ensure-no-changes
    just docs-lint

ci-test-framework:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-test-framework ==="
    just setup
    just build-core
    just build-server
    just build-client
    just test-core
    just test-server
    just test-client

ci-test-adapters:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-test-adapters ==="
    just setup
    just build-integrations
    just build-finance
    just build-tools
    just test-integrations
    just test-tools
    just test-finance

# nightly variants: the same groups, with the finance read block no longer filtered out. They differ
# from the day recipes in exactly that one line. There is no nightly variant of the write block.

# identical to ci-test-framework - no finance in this group
ci-test-framework-nightly: ci-test-framework

ci-test-adapters-nightly:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-test-adapters-nightly ==="
    just setup
    just build-integrations
    just build-finance
    just build-tools
    just test-integrations
    just test-tools
    echo "=== test finance (offline + read) ==="
    dotnet test --solution finance/finance.slnx -c Release --no-build --report-xunit-trx \
        -- --filter-not-trait "block=write"

ci-release apiKey repository githubToken:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-release ==="
    just setup
    just format
    just ensure-no-changes
    just ci-set-package-version
    just clean
    just build
    just docs-lint
    just pack
    just publish "$1"
    just ci-push-tag "$2" "$3"
    echo "Release complete"

ci-set-package-version:
    @echo "=== $0 ==="
    git config user.name "it"
    git config user.email "it@annium.com"
    dotnet tool run versioning set-version -v $(cat version)

ci-push-tag repository githubToken:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-push-tag ==="
    packageVersion=$(dotnet tool run versioning get-version -v $(cat version))
    git remote set-url origin https://x-access-token:"$2"@github.com/"$1".git
    if git ls-remote --exit-code --tags origin "v$packageVersion" >/dev/null 2>&1; then
        echo "tag v$packageVersion already published, skipping"
        exit 0
    fi
    git push origin v$packageVersion

# test keys

gen-rsa-keys:
    @echo "=== $0 ==="
    openssl req -x509 -noenc -days 3650 -keyout private.pem -out cert.pem
    openssl rsa -in private.pem -pubout -out public.pem
    openssl pkcs12 -export -inkey private.pem -in cert.pem -out cert.pfx
    rm cert.pem

copy-rsa-keys:
    @echo "=== $0 ==="
    cp private.pem core/Identity/tests/Annium.Identity.Tokens.Tests/keys/rsa_private.pem
    cp public.pem core/Identity/tests/Annium.Identity.Tokens.Tests/keys/rsa_public.pem
    cp private.pem core/Identity/tests/Annium.Identity.Tokens.Jwt.Tests/keys/rsa_private.pem
    cp public.pem core/Identity/tests/Annium.Identity.Tokens.Jwt.Tests/keys/rsa_public.pem
    rm private.pem public.pem
    mv cert.pfx core/Net/tests/Annium.Net.Sockets.Tests/keys/rsa_cert.pfx

gen-ec-keys:
    @echo "=== $0 ==="
    openssl req -new -newkey ec -pkeyopt ec_paramgen_curve:secp521r1 -x509 -noenc -days 3650 -keyout private.pem -out cert.pem
    openssl ec -in private.pem -pubout -out public.pem
    openssl pkcs12 -export -inkey private.pem -in cert.pem -out cert.pfx
    rm cert.pem

copy-ec-keys:
    @echo "=== $0 ==="
    cp private.pem core/Identity/tests/Annium.Identity.Tokens.Tests/keys/ecdsa_private.pem
    cp public.pem core/Identity/tests/Annium.Identity.Tokens.Tests/keys/ecdsa_public.pem
    cp private.pem core/Identity/tests/Annium.Identity.Tokens.Jwt.Tests/keys/ecdsa_private.pem
    cp public.pem core/Identity/tests/Annium.Identity.Tokens.Jwt.Tests/keys/ecdsa_public.pem
    rm private.pem public.pem
    mv cert.pfx core/Net/tests/Annium.Net.Sockets.Tests/keys/ecdsa_cert.pfx

# demo

demo-blazor-ant:
    @echo "=== $0 ==="
    cd client/Blazor/demo/Demo.Blazor.Ant && dotnet watch run

demo-blazor-ant-prod:
    @echo "=== $0 ==="
    cd client/Blazor/demo/Demo.Blazor.Ant && rm -rf dist && dotnet publish -c Release -o dist && dotnet serve --directory dist/wwwroot -p 5004 -q

demo-blazor-interop:
    @echo "=== $0 ==="
    cd client/Blazor/demo/Demo.Blazor.Interop && dotnet watch run

demo-blazor-interop-prod:
    @echo "=== $0 ==="
    cd client/Blazor/demo/Demo.Blazor.Interop && rm -rf dist && dotnet publish -c Release -o dist && dotnet serve --directory dist/wwwroot -p 5002 -q

demo-blazor-charts:
    @echo "=== $0 ==="
    cd client/Blazor/demo/Demo.Blazor.Charts && dotnet watch run

demo-blazor-charts-prod:
    @echo "=== $0 ==="
    cd client/Blazor/demo/Demo.Blazor.Charts && rm -rf dist && dotnet publish -c Release -o dist && dotnet serve --directory dist/wwwroot -p 5003 -q

# tools

install: install-doclint install-versioning install-xrest

install-doclint:
    @just _tool-install tools/DocLint/src/Annium.DocLint

install-versioning:
    @just _tool-install tools/Versioning/src/Annium.Versioning

install-xrest:
    @just _tool-install tools/XRest/src/Annium.XRest

uninstall: uninstall-doclint uninstall-versioning uninstall-xrest

uninstall-doclint:
    @just _tool-uninstall tools/DocLint/src/Annium.DocLint

uninstall-versioning:
    @just _tool-uninstall tools/Versioning/src/Annium.Versioning

uninstall-xrest:
    @just _tool-uninstall tools/XRest/src/Annium.XRest

xrest-server:
    @echo "=== $0 ==="
    dotnet run --project tools/XRest/demo/Annium.XRest.Demo.Server

xrest-gen:
    @echo "=== $0 ==="
    dotnet run --project tools/XRest/src/Annium.XRest -- \
        cs gen \
        -s http://localhost:5000 \
        -ns Annium.XRest.Demo.Client.Api \
        -o tools/XRest/demo/Annium.XRest.Demo.Client/Api \
        -trace

# private helpers

_build solution:
    #!/usr/bin/env bash
    set -e
    echo "=== build {{solution}} ==="
    packageVersion=$(dotnet tool run versioning get-version -v $(cat version))
    dotnet build {{solution}}.slnx -c Release --nologo -v q -p:PackageVersion=$packageVersion

# `dotnet test` wants --solution for a .slnx; a bare path is rejected
_test solution:
    @echo "=== test {{solution}} ==="
    dotnet test --solution {{solution}}.slnx -c Release --no-build --report-xunit-trx
