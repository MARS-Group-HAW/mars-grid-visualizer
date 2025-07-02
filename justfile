default:
    just run

run:
    dotnet build && godot-mono .

clean:
    #!/usr/bin/env fish
    set dirs (fd --regex "(obj|bin)" --no-ignore -t d)
    if test (count $dirs) -gt 0
        echo -e "deleting:\n  $dirs"
        rm -r $dirs
    else
        echo "nothing found"
    end
