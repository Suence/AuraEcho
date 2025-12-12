open Microsoft.EntityFrameworkCore
open PowerLab.Core.Data

[<EntryPoint>]
let main _ =

    use powerLabDbContext = DbContextFactory.CreateDbContext()

    let pending = powerLabDbContext.Database.GetPendingMigrations()

    if Seq.isEmpty pending |> not then
        powerLabDbContext.Database.Migrate() |> ignore

    0
