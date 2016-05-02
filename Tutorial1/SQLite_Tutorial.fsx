#r @"..\packages\System.Data.SQLite.Core.1.0.101.0\lib\net451\System.Data.SQLite.dll"
#r @"..\SQLAccess\bin\Debug\SQLAccess.dll"

open System
open System.Runtime.InteropServices
open System.IO
open System.Data
open System.Data.SQLite
open SQLAccess
open SQLAccess.MappingData
open SQLAccess.SQLData


module Kernel =
    [<DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
    extern IntPtr LoadLibrary(string lpFileName);

Kernel.LoadLibrary(Path.Combine(@"..\packages\System.Data.SQLite.Core.1.0.101.0\lib\net451\", "System.Data.SQLite.dll"))
Kernel.LoadLibrary(Path.Combine(@"..\packages\System.Data.SQLite.Core.1.0.101.0\build\net451\x64\", "SQLite.Interop.dll"))
Kernel.LoadLibrary(Path.Combine(@"..\SQLAccess\bin\Debug\", "SQLAccess.dll"))


type User = { UserId:int64 ; Name:string; Surname:string}

let display user =
            printfn "%A - %s %s" user.UserId  user.Name  user.Surname

let toUser (reader: IDataReader ) =
    { 
        UserId = unbox(reader.["ID"])
        Name = unbox(reader.["Name"])
        Surname = unbox(reader.["SurName"])
    }  

let [<Literal>] DBPATH = __SOURCE_DIRECTORY__ + @"\bin\Debug\test.db" 

//query without parameters
let users = SQLData.uQuery (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            "Select * from tbUsers", CommandType.Text, Seq.empty, 
                            toUser)
Seq.iter display users

//query using parameters
let single_user = SQLData.uQuery (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            "Select * from tbUsers WHERE id=@id", CommandType.Text, [("@id", 1)] , 
                            toUser)
 
Seq.iter display  single_user

//let rnd = Random().Next passing a function, not a value!!!!
let rnd = Random().Next()

let result = SQLData.nonQuery (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            "INSERT into tbUsers(Name, Surname) VALUES(@Name, @Surname)", CommandType.Text, [("@Name", ("pippo" + rnd.ToString()));("@Surname", "di paperinia")])

 