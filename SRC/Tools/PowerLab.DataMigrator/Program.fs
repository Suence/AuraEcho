open System
open Microsoft.EntityFrameworkCore
open PowerLab.Core.Data

[<EntryPoint>]
let main _ =
    use powerLabDbContext = new PowerLabDbContext()

    let pending = powerLabDbContext.Database.GetPendingMigrations()

    if Seq.isEmpty pending |> not then
        powerLabDbContext.Database.Migrate() |> ignore

    0
