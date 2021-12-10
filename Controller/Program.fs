
namespace Metaballs


module Controller =

    open System
    open System.Diagnostics
    open Elmish
    open Elmish.WPF


    let private newRandomCircle =
        let random = new Random ()

        fun () ->
            { X = 0.5; Y = 0.5
              DeltaX = 0.1 + 0.5 * random.NextDouble ()
              DeltaY = 0.1 + 0.5 * random.NextDouble ()
              Radius = 0.05 + 0.05 * random.NextDouble () } : Circle


    let private init () =
        ({ Circles = []; RunState = Running; LastRenderTime = DateTime.UtcNow } : AppState), Cmd.none


    let private update renderer msg (m: AppState) =
        match msg with
        | AddCircle ->
            { m with Circles = newRandomCircle () :: m.Circles }, Cmd.none

        | RemoveCircle when m.Circles.IsEmpty ->
            m, Cmd.none

        | RemoveCircle ->
            { m with Circles = m.Circles |> List.tail }, Cmd.none

        | Pause ->
            { m with RunState = Paused }, Cmd.none

        | Resume ->
            { m with RunState = Running; LastRenderTime = DateTime.UtcNow }, Cmd.none

        | Render renderStart when m.RunState = Running ->
            { m with RunState = Rendering}, Cmd.OfTask.result (renderer m.LastRenderTime renderStart m.Circles)

        | Render _ ->
            if m.RunState = Rendering then
                Trace.WriteLine "Frame dropped."

            m, Cmd.none

        | RenderFinished renderStart ->
            { m with
                RunState = (if m.RunState = Paused then Paused else Running)
                LastRenderTime = renderStart }, Cmd.none


    let private bindings () = [
        "IsPaused" |> Binding.oneWay (fun m -> m.RunState = Paused)
        "AddCircle" |> Binding.cmd (fun _ -> AddCircle)
        "RemoveCircle" |> Binding.cmd (fun _ -> RemoveCircle)
        "PauseOrResume" |> Binding.cmd (fun m ->
            match m.RunState with
            | Running
            | Rendering -> Pause
            | Paused -> Resume)
    ]
        


    [<EntryPoint>]
    [<STAThread>]
    let main argv =
        let mainWindow = new Main ()

        let timerTick interval =
            let timer = new Timers.Timer (interval)
            timer.Start()

            fun dispatch ->
              timer.Elapsed.Add (fun _ -> dispatch (Render DateTime.Now))
              
        let renderer =
            Renderer.renderScene mainWindow.Dispatcher mainWindow.Bitmap

        Program.mkProgramWpf init (update renderer) bindings
        |> Program.withSubscription (fun _ -> Cmd.ofSub (timerTick 50.0))
        |> Program.runWindow mainWindow