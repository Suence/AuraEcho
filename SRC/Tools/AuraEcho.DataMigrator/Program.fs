open Microsoft.EntityFrameworkCore
open AuraEcho.Core.Data

[<EntryPoint>]
let main _ =

    use auraEchoDbContext = DbContextFactory.CreateDbContext()

    let pending = auraEchoDbContext.Database.GetPendingMigrations()

    if Seq.isEmpty pending |> not then
        auraEchoDbContext.Database.Migrate() |> ignore

    0
