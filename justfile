default:
    just run

run:
    dotnet build && godot-mono .

test:
    dotnet test -v quiet

clean:
    #!/usr/bin/env fish
    set dirs (fd --regex "(obj|bin)" --no-ignore -t d)
    if test (count $dirs) -gt 0
        echo -e "deleting:\n  $dirs"
        rm -r $dirs
    else
        echo "nothing found"
    end

# assert that layers don't know anything about layers above
check-architecture:
    #!/usr/bin/env fish
    set violations (rg -n --color=always '\bDomain\b|\bPresentation\b' src/Infrastructure/ --glob '*.cs' 2>/dev/null; rg -n --color=always '\bPresentation\b' src/Domain/ --glob '*.cs' 2>/dev/null | string collect)
    if test -n "$violations"
        echo $violations
        exit 1
    end
