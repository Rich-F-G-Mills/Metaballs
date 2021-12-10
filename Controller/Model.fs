
namespace Metaballs

[<AutoOpen>]
module Model =

    open System

    type internal AppEvent =
        | AddCircle
        | RemoveCircle
        | Pause
        | Resume
        | Render of StartTime: DateTime
        | RenderFinished of StartTime: DateTime

    type internal RunState =
        | Running
        | Rendering
        | Paused

    type internal Circle =
        { mutable X: double
          mutable Y: double
          mutable DeltaX: double
          mutable DeltaY: double
          Radius: double }

    type internal AppState =
        { Circles: Circle list
          RunState: RunState
          LastRenderTime: DateTime }
