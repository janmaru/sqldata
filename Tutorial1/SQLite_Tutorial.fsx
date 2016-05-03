#r @"..\packages\System.Data.SQLite.Core.1.0.101.0\lib\net451\System.Data.SQLite.dll"
#r @"..\SQLAccess\bin\Debug\SQLAccess.dll"

open System
open System.Runtime.InteropServices
open System.IO
open System.Data
open System.Data.SQLite
open SQLAccess
open SQLAccess.Railway
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
//let [<Literal>] DBPATH = __SOURCE_DIRECTORY__ + @"\bin\Debug\test666.db" 
// connection string
    //    //Basic
    //    //Data Source=c:\mydb.db;Version=3;
    //    //Version 2 is not supported by this class library.	SQLite
    //    //In-Memory Database
    //    //An SQLite database is normally stored on disk but the database can also be stored in memory. Read more about SQLite in-memory databases here.
    //    //Data Source=:memory:;Version=3;New=True;
    //    //SQLite
    //    //Using UTF16
    //    //Data Source=c:\mydb.db;Version=3;UseUTF16Encoding=True;
    //    //SQLite9
    //    //With password
    //    //Data Source=c:\mydb.db;Version=3;Password=myPassword;

//unsafe query without parameters
let users = SQLData.uQuery (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            "Select * from tbUsers", CommandType.Text, Seq.empty, 
                            toUser)
Seq.iter display users

//railway pattern query without parameters
let result= SQLData.query (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            "Select * from tbUsers", CommandType.Text, Seq.empty, 
                            toUser)
match result with
| Success x -> Seq.iter display x
| Failure y ->  printfn "errore: %s" y

//query using parameters
let single_user = SQLData.uQuery (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            "Select * from tbUsers WHERE id=@id", CommandType.Text, [("@id", 1)] , 
                            toUser)
 
Seq.iter display  single_user

//railway query with parameters
let result3 = SQLData.query (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            "Select * from tbUsers WHERE id=@id", CommandType.Text, [("@id", 1)] , 
                            toUser)

match result3 with
| Success x -> Seq.iter display x
| Failure y ->  printfn "errore: %s" y

//let rnd = Random().Next passing a function, not a value!!!!
let rnd = Random().Next()

let result2 = SQLData.nonQuery (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            "INSERT into tbUsers(Name, Surname) VALUES(@Name, @Surname)", CommandType.Text, [("@Name", ("pippo" + rnd.ToString()));("@Surname", "di paperinia")])

match result2 with
| Success x -> printfn "number of records affected: %A" x
| Failure y ->  printfn "errore: %s" y

//************* using a mini orm for mapping
open SQLAccess.MappingData

//only to trigger the interop SQLite check for x64 architecture
let result4 = checkConnection (SQLite, sprintf @"Data Source=%s;Version=3;" DBPATH)

match result4 with
| Success x -> printfn "connessione ok"
| Failure y ->  printfn "errore: %s" y

//F# immutable records are not POCO types; they do not have default constructors,
// or have setters for the properties. 
type User2 = { Id:int ; Name:string; Surname:string; Age:int}

//the F# compiler emits a default constructor and property setters into the generated IL 
//for this type (though those features are not exposed to F# code).  
[<CLIMutable>] 
type User3 = { Id:int ; Name:string; Surname:string; Age:int}

// query without parameters    
let listOfUsers2 = uQuery<User3> (SQLite, 
                                  sprintf @"Data Source=%s;Version=3;" DBPATH,
                                  "SELECT * From tbUsers",
                                  null)
//inference type user3
let display2 user =
            printfn "%A - %s %s" user.Id  user.Name  user.Surname

Seq.iter  display2 listOfUsers2 

listOfUsers2 |> Seq.iter (fun x  ->  printfn "%s" x.Name) 

//query with parameters
[<CLIMutable>] 
type UserSelectArgs = { SelectedUserId:int}

let singleUser =
        uQuery<User3> (SQLite, 
                        sprintf @"Data Source=%s;Version=3;" DBPATH,
                        "SELECT ID, Surname From tbUsers WHERE ID = @SelectedUserId",
                        {SelectedUserId=1})
        |> Seq.head

printfn "%s" singleUser.Surname

//result with null values
//val singleUser : User3 = {Id = 1;
//                          Name = null;
//                          Surname = "Janus";
//                          Age = 0;}


//result with type mismatch
[<CLIMutable>] 
type User666 = { MyId:int ; MyName:string; Surname:string; MyAge:int}

let listOfUsers666 = uQuery<User666> (SQLite, 
                                  sprintf @"Data Source=%s;Version=3;" DBPATH,
                                  "SELECT * From tbUsers",
                                  null)

listOfUsers666 |> Seq.iter (fun x  ->  printfn "%s %s %A %A" x.MyName x.Surname x.MyAge x.MyId) 

//using aliases  "SELECT ID as MyId, Name as MyName, Surname From tbUsers"