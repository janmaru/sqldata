#r @"D:\PAKET_PRESENTATION\SQLAccess\SQLAccess\bin\Debug\SQLAccess.dll"

open System
open System.IO
open System.Data
open SQLAccess
open SQLAccess.Railway
open SQLAccess.SQLData 

module AccessUser = 
    [<Literal>]
    let CN_STRING = @"Server=.\SQLEXPRESS;Database=test2;Trusted_Connection=True;"
 
     //BEGIN SQLACCESS -------------------------------------------

    //UserId is an int NOT an int64 -->SQLServer
    type User = { UserId:int ; Name:string; Surname:string}

    let display user =
            printfn "%A - %s %s" user.UserId  user.Name  user.Surname

    let toUser (reader: IDataReader) =
        { 
            UserId = unbox(reader.["ID"])
            Name = unbox(reader.["Name"])
            Surname = unbox(reader.["SurName"])
        }  

    
    let result = uQuery(SQLServer,
                        CN_STRING,
                        "Select * from tbUsers",
                        CommandType.Text, 
                        Seq.empty,
                        toUser)

//    match result with
//    | Success x -> Seq.iter display x
//    | Failure y ->  printfn "errore: %s" y

//    try
//        match result with
//        | Success x -> Seq.iter display x
//        | _ -> 0 |>ignore
//    with 
//      | ex-> printfn "errore: %s " ex.Message

    let result3 = query 
                       (SQLServer,
                        CN_STRING ,                       
                        "Select * from tbUsers",
                        CommandType.Text,
                        List.empty,
                        toUser)

    match result3 with
    | Success x -> Seq.iter display x
    | Failure y ->  printfn "errore: %s" y