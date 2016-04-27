#r @"D:\PAKET_PRESENTATION\SQLAccess\packages\System.Data.SQLite.Core.1.0.99.0\lib\net451\System.Data.SQLite.dll"
#r @"D:\PAKET_PRESENTATION\SQLAccess\SQLAccess\bin\Debug\SQLAccess.dll"

open SQLAccess.DapperFSharp
open SQLAccess.SQLDataFSharp
open System.Data
open System.Data.SQLite
open System
open System.Runtime.InteropServices
open System.IO


module Kernel =
    [<DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
    extern IntPtr LoadLibrary(string lpFileName);

Kernel.LoadLibrary(Path.Combine(@"D:\PAKET_PRESENTATION\SQLAccess\packages\System.Data.SQLite.Core.1.0.99.0\lib\net451\", "System.Data.SQLite.dll"))
Kernel.LoadLibrary(Path.Combine(@"D:\PAKET_PRESENTATION\SQLAccess\packages\System.Data.SQLite.Core.1.0.99.0\build\net451\x64\", "SQLite.Interop.dll"))
Kernel.LoadLibrary(Path.Combine(@"D:\PAKET_PRESENTATION\SQLAccess\SQLAccess\bin\Debug\", "SQLAccess.dll"))


type User = { UserId:int64 ; Name:string; Surname:string}

let display user =
            printfn "%A - %s %s" user.UserId  user.Name  user.Surname

let toUser (reader: SQLiteDataReader ) =
    { 
        UserId = unbox(reader.["ID"])
        Name = unbox(reader.["Name"])
        Surname = unbox(reader.["SurName"])
    }  

printfn "%s" "query without a filter ----"
let users = queryTextSQLite @"Data Source=D:\PAKET_PRESENTATION\SQLAccess\Tutorial1\bin\Debug\test.db;Version=3;" "Select * from tbUsers" toUser
Seq.iter display users

printfn "%s" "query with a filter ----"

let single_user= queryParametrizedSQLite @"Data Source=test.db;Version=3;" "Select * from tbUsers WHERE ID=@id" CommandType.Text [("@id", 1)] toUser

Seq.iter display  single_user

printfn "%s" "insert query with a filter ----"

//let rnd = Random().Next passing a function
let rnd = Random().Next()

//let single_user= nonQueryParametrizedSQLite @"Data Source=test.db;Version=3;" "INSERT into tbUsers(Name, Surname) VALUES(@Name, @Surname)" CommandType.Text [("@Name", ("pippo" + rnd.ToString()));("@Surname", "di paperinia")] toUser
