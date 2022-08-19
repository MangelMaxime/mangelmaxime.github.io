namespace FSharp.Static

open System
open Microsoft.Extensions.Logging
open System.Collections.Generic
open System.Collections.Concurrent
open System.Runtime.Versioning
open Microsoft.Extensions.Options
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.DependencyInjection.Extensions
open Microsoft.Extensions.Logging.Configuration

module Server =

    type FSharpStaticConsoleLoggerConfiguration() =

        let _logLevels = new Dictionary<LogLevel, ConsoleColor>()

        do _logLevels.Add(LogLevel.Information, ConsoleColor.Green)

        member _.EventId: int = 0

        member _.LogLevels: Dictionary<LogLevel, ConsoleColor> = _logLevels

    type FSharpStaticConsoleLogger
        (
            name: string,
            getCurrentConfig: Func<unit, FSharpStaticConsoleLoggerConfiguration>
        ) =

        member _.IsEnabled(logLevel: LogLevel) =
            getCurrentConfig.Invoke().LogLevels.ContainsKey(logLevel)

        interface ILogger with

            member _.BeginScope<'TState>(state: 'TState) = null

            member this.IsEnabled(logLevel: LogLevel) = this.IsEnabled logLevel

            member this.Log<'TState>
                (
                    logLevel: LogLevel,
                    eventId: EventId,
                    state: 'TState,
                    exn: Exception,
                    formatter: Func<'TState, Exception, string>
                ) =
                if this.IsEnabled(logLevel) then
                    let config = getCurrentConfig.Invoke()

                    if (config.EventId = 0 || config.EventId = eventId.Id) then
                        let logFunc =
                            match logLevel with
                            | LogLevel.Trace
                            | LogLevel.Debug -> Log.debug
                            | LogLevel.Information -> Log.info
                            | LogLevel.Warning -> Log.warn
                            | LogLevel.Error
                            | LogLevel.Critical -> Log.error
                            | LogLevel.None
                            | _ -> ignore

                        logFunc $"{formatter.Invoke(state, exn)}"

                else
                    ()

    [<UnsupportedOSPlatform("browser")>]
    [<ProviderAlias("ColorConsole")>]
    type ColorConsoleLoggerProvider
        (
            config: IOptionsMonitor<FSharpStaticConsoleLoggerConfiguration>
        ) =

        let mutable _currentConfig = config.CurrentValue

        let _onChangeToken =
            config.OnChange(fun updatedConfig -> _currentConfig <- updatedConfig
            )

        let _loggers =
            new ConcurrentDictionary<string, FSharpStaticConsoleLogger>(
                StringComparer.OrdinalIgnoreCase
            )

        member private _.GetCurrentConfig() = _currentConfig

        interface ILoggerProvider with

            member this.CreateLogger(categoryName: string) =
                _loggers.GetOrAdd(
                    categoryName,
                    fun name ->
                        new FSharpStaticConsoleLogger(
                            name,
                            Func<unit, FSharpStaticConsoleLoggerConfiguration>
                                this.GetCurrentConfig
                        )
                )

            member _.Dispose() =
                _loggers.Clear()
                _onChangeToken.Dispose()

    type ILoggingBuilder with

        member this.AddColorConsoleLogger() =
            this.AddConfiguration()

            this.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, ColorConsoleLoggerProvider>
                    ()
            )


            LoggerProviderOptions.RegisterProviderOptions<FSharpStaticConsoleLoggerConfiguration, ColorConsoleLoggerProvider>(
                this.Services
            )

            this

        member this.AddColorConsoleLogger
            (configure: Action<FSharpStaticConsoleLoggerConfiguration>)
            =
            this.AddColorConsoleLogger() |> ignore
            this.Services.Configure(configure) |> ignore

            this
