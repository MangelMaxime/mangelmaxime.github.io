namespace Nacara

module Log =

    open System
    open Spectre.Console

    let info (msg : string) =
        AnsiConsole.MarkupLineInterpolated($"{msg}")

    let error (msg : string) =
        AnsiConsole.MarkupLineInterpolated($"[red]{msg}[/]")

    let warn (msg : string) =
        AnsiConsole.MarkupLineInterpolated($"[yellow]{msg}[/]")

    let debug (msg : string) =
        AnsiConsole.MarkupLineInterpolated($"[grey93]{msg}[/]")

    let success (msg : string) =
        AnsiConsole.MarkupLineInterpolated($"[green]{msg}[/]")
