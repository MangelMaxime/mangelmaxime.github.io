namespace Nacara

module Log =

    open System
    open Spectre.Console

    let timestamp () = "[" + DateTime.Now.ToString("HH:mm:ss") + "]"

    let info (msg : string) =
        AnsiConsole.MarkupLineInterpolated($"[blue]{timestamp ()} {msg}[/]")

    let error (msg : string) =
        AnsiConsole.MarkupLineInterpolated($"[red]{timestamp ()} {msg}[/]")

    let warn (msg : string) =
        AnsiConsole.MarkupLineInterpolated($"[yellow]{timestamp ()} {msg}[/]")

    let debug (msg : string) =
        AnsiConsole.MarkupLineInterpolated($"{timestamp ()} {msg}")

    let success (msg : string) =
        AnsiConsole.MarkupLineInterpolated($"[green]{timestamp ()} {msg}[/]")
