module Nacara.Commands.Clean

open Nacara
open Nacara.Core
open Nacara.Evaluator
open System.IO

let execute () =
    let context = Shared.loadConfigOrExit ()

    Directory.Delete(AbsolutePath.toString context.OutputPath, true)

    0
