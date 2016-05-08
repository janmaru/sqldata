#r @"..\SQLAccess\bin\Debug\SQLData.dll"

open System
open System.IO
open System.Data
open System.Data.SqlClient
open Mahamudra.System.Railway
open Mahamudra.System.Data.Sql 
open Mahamudra.System.Data.Sql.SQLData


//BEGIN DAPPER -----------------------------------------------
[<Literal>]
let CN_STRING = @"Server=.\SQLEXPRESS;Database=test;Trusted_Connection=True;"
     
//the F# compiler emits a default constructor and property setters into the generated IL 
//for this type (though those features are not exposed to F# code).  
[<CLIMutable>] 
type User = { Id:int ; Name:string; Surname:string; Age:int}

let listOfUser =       
    uQuery<User> (SQLServer, CN_STRING ,"SELECT * From tbUsers",null)

listOfUser |> Seq.iter (fun x  ->  printfn "%s %s" x.Name x.Surname) 

[<CLIMutable>] 
type UserSelectArgs = { SelectedUserId:int}

let getUser userId =
    uQuery<User>  (SQLServer, CN_STRING ,"SELECT ID, Surname From tbUsers WHERE ID = @SelectedUserId",{SelectedUserId=userId})
    |> Seq.head

let singleUser = getUser 1  
printfn "%s" singleUser.Surname
// END DAPPER -----------------------------------------------


//BEGIN SQLData -------------------------------------------

//UserId is an int NOT an int64 -->SQLServer
type User2 = { UserId:int ; Name:string; Surname:string}

let display user =
        printfn "%A - %s %s" user.UserId  user.Name  user.Surname

let toUser (reader: IDataReader) =
    { 
        UserId = unbox(reader.["ID"])
        Name = unbox(reader.["Name"])
        Surname = unbox(reader.["SurName"])
    }  

 
//railway pattern query without parameters
let result= SQLData.query (SQLServer, 
                           CN_STRING,
                           "Select * from tbUsers", CommandType.Text,
                           Seq.empty, 
                           toUser)
match result with
| Success x -> Seq.iter display x
| Failure y ->  printfn "errore: %s" y


//END SQLData   -------------------------------------------

