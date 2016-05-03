#r @"D:\PAKET_PRESENTATION\SQLAccess\SQLAccess\bin\Debug\SQLAccess.dll"

open System
open System.IO
open System.Data
open System.Data.SqlClient
open SQLAccess
open SQLAccess.SQLDataFSharp

module AccessUser = 
    //BEGIN DAPPER -----------------------------------------------
    [<Literal>]
    let CN_STRING = @"Server=.\SQLEXPRESS;Database=test;Trusted_Connection=True;"
     
    //the F# compiler emits a default constructor and property setters into the generated IL 
    //for this type (though those features are not exposed to F# code).  
    [<CLIMutable>] 
    type User = { Id:int ; Name:string; Surname:string; Age:int}

    let getUsers connection =        
        uQuery<User> (connection,"SELECT * From tbUsers",null)

    let listOfUser = getUsers (new SqlConnection(CN_STRING))
    listOfUser |> Seq.iter (fun x  ->  printfn "%s %s" x.Name x.Surname) 

    [<CLIMutable>] 
    type UserSelectArgs = { SelectedUserId:int}

    let getUser userId connection =
        uQuery<User>  (connection,"SELECT ID, Surname From tbUsers WHERE ID = @SelectedUserId",{SelectedUserId=userId})
        |> Seq.head

    let singleUser = getUser 1 (new SqlConnection(CN_STRING))
    printfn "%s" singleUser.Surname
    // END DAPPER -----------------------------------------------


    //BEGIN SQLACCESS -------------------------------------------

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

    printfn "%s" "query without a filter ----"
    let users = query2 
                    CN_STRING 
                    SQLServer 
                    "Select * from tbUsers" 
                    CommandType.Text 
                    Seq.empty 
                    toUser

    Seq.iter display users

    printfn "%s" "query with a filter ----"


    //END SQLACCESS   -------------------------------------------

