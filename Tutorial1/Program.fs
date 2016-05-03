

open SQLAccess.MappingData
open SQLAccess.SQLDataFSharp
open System.Data
open System.Data.SQLite
open System

//type User = { UserId:int ; Name:string; Surname:string} this doesn't unbox!!!!
type User = { UserId:int64 ; Name:string; Surname:string}

[<EntryPoint>]
let main argv = 
        let display user =
            printfn "%A - %s %s" user.UserId  user.Name  user.Surname

        let toUser (reader: IDataReader ) =
            { 
                UserId = unbox(reader.["ID"])
                Name = unbox(reader.["Name"])
                Surname = unbox(reader.["SurName"])
            }  

        printfn "%s" "query without a filter ----"
        let users = queryTextSQLite @"Data Source=test.db;Version=3;" "Select * from tbUsers" toUser
        Seq.iter display users

        printfn "%s" "query with a filter ----"

        let single_user= queryParametrizedSQLite @"Data Source=test.db;Version=3;" "Select * from tbUsers WHERE ID=@id" CommandType.Text [("@id", 1)] toUser

        Seq.iter display  single_user

        printfn "%s" "insert query with a filter ----"

        //let rnd = Random().Next passing a function
        let rnd = Random().Next()

        //let single_user= nonQueryParametrizedSQLite @"Data Source=test.db;Version=3;" "INSERT into tbUsers(Name, Surname) VALUES(@Name, @Surname)" CommandType.Text [("@Name", ("pippo" + rnd.ToString()));("@Surname", "di paperinia")] toUser

        Console.ReadKey() |> ignore;
        0
     