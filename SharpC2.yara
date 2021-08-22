import "dotnet"

rule SharpC2_Traffic {

    meta:
        author = "Daniel Duggan <@_RastaMouse>"
        date = "22/08/2021"
        version = "0.1"
        reference = "https://github.com/SharpC2/SharpC2"

    strings:
        $header = {58 2d 4d 61 6c 77 61 72 65 3a 20 53 68 61 72 70 43 32}

    condition:
        $header
}

rule SharpC2_PE {

    meta:
        author = "Daniel Duggan <@_RastaMouse>"
        date = "22/08/2021"
        version = "0.1"
        reference = "https://github.com/SharpC2/SharpC2"

    condition:
        dotnet.assembly.name == "drone"
}