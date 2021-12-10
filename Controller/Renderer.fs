
namespace Metaballs

[<RequireQualifiedAccess>]
module Renderer =

    open System
    open System.Windows
    open System.Windows.Threading
    open System.Windows.Media.Imaging
    open System.Threading.Tasks
    open System.Runtime.CompilerServices
    open Microsoft.FSharp.NativeInterop

    
    let internal renderScene (dispatcher: Dispatcher) (bitmap: WriteableBitmap) =
        let sceneWidth = bitmap.Width
        let sceneHeight = bitmap.Height
        let sceneRect = Int32Rect (0, 0, int sceneWidth, int sceneHeight)
        let rowStride = bitmap.BackBufferStride / 4
        let imageByteSize: uint32 = uint32 <| rowStride * (int sceneHeight) * 4
        let backBuffer = NativePtr.ofNativeInt <| bitmap.BackBuffer
    
        fun lastRenderTime renderTime (circles: Model.Circle list) ->
            Task.Run (fun () -> 
                try
                    dispatcher.Invoke (bitmap.Lock)

                    Unsafe.InitBlock (NativePtr.toVoidPtr backBuffer, 0uy, imageByteSize)

                    if not circles.IsEmpty then
                        for y = 0 to (int sceneHeight - 1) do
                            let y' = (double y) / sceneHeight

                            let rowBuffer =
                                NativePtr.add backBuffer (y * rowStride)

                            for x = 0 to (int sceneWidth - 1) do
                                let x' = (double x) / sceneWidth

                                let mutable cellValue = 0.0

                                for c in circles do
                                    cellValue <- cellValue + c.Radius / Math.Sqrt ((x' - c.X) * (x' - c.X) + (y' - c.Y) * (y' - c.Y))

                                if cellValue > 1.0 then
                                    let redContent =
                                       (int <| Math.Clamp (255.0 - 255.0 / cellValue, 50.0, 200.0)) <<< 16

                                    NativePtr.write (NativePtr.add rowBuffer x) redContent                                    

                    dispatcher.Invoke (fun () -> bitmap.AddDirtyRect sceneRect)

                finally
                    dispatcher.Invoke (bitmap.Unlock)

                let timeDiff =
                    ((renderTime - lastRenderTime): TimeSpan).TotalSeconds

                for c in circles do
                    let newX = c.X + c.DeltaX * timeDiff
                    let newY = c.Y + c.DeltaY * timeDiff

                    if (newX - c.Radius < 0.0 && c.DeltaX < 0.0) || (newX + c.Radius > 1.0 && c.DeltaX > 0.0) then
                        c.DeltaX <- -c.DeltaX

                    if (newY - c.Radius < 0.0 && c.DeltaY < 0.0) || (newY + c.Radius > 1.0 && c.DeltaY > 0.0) then
                        c.DeltaY <- -c.DeltaY

                    c.X <- newX
                    c.Y <- newY
            
                Model.RenderFinished renderTime)   
