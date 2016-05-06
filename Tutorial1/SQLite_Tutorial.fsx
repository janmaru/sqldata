#r @"..\packages\System.Data.SQLite.Core.1.0.101.0\lib\net451\System.Data.SQLite.dll"
#r @"..\SQLAccess\bin\Debug\SQLData.dll"

open System
open System.Runtime.InteropServices
open System.IO
open System.Data
open System.Data.SQLite
open SQLData
open SQLData.Railway
open SQLData.SQLData


module Kernel =
    [<DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)>]
    extern IntPtr LoadLibrary(string lpFileName);

Kernel.LoadLibrary(Path.Combine(@"..\packages\System.Data.SQLite.Core.1.0.101.0\lib\net451\", "System.Data.SQLite.dll"))
Kernel.LoadLibrary(Path.Combine(@"..\packages\System.Data.SQLite.Core.1.0.101.0\build\net451\x64\", "SQLite.Interop.dll"))
Kernel.LoadLibrary(Path.Combine(@"..\SQLAccess\bin\Debug\", "SQLData.dll"))

//In SQLite, table rows normally have a 64-bit signed integer ROWID which is unique among all rows in the same table. 
//(WITHOUT ROWID tables are the exception.)
//The rowid is always available as an undeclared column named ROWID, OID, or _ROWID_ as long as those names are not also used by explicitly declared columns.
// If the table has a column of type INTEGER PRIMARY KEY then that column is another alias for the rowid.

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

//https://www.sqlite.org/c3ref/last_insert_rowid.html
//https://www.sqlite.org/autoinc.html
//1. The AUTOINCREMENT keyword imposes extra CPU, memory, disk space, and disk I/O overhead and should be avoided if not strictly needed. It is usually not needed.
//
//2. In SQLite, a column with type INTEGER PRIMARY KEY is an alias for the ROWID (except in WITHOUT ROWID tables) which is always a 64-bit signed integer.
//
//3. On an INSERT, if the ROWID or INTEGER PRIMARY KEY column is not explicitly given a value, then it will be filled automatically with an unused integer, usually one more than the largest ROWID currently in use. This is true regardless of whether or not the AUTOINCREMENT keyword is used.
//
//4. If the AUTOINCREMENT keyword appears after INTEGER PRIMARY KEY, that changes the automatic ROWID assignment algorithm to prevent the reuse of ROWIDs over the lifetime of the database. In other words, the purpose of AUTOINCREMENT is to prevent the reuse of ROWIDs from previously deleted rows

let result5 = SQLData.mixedQuery (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            "INSERT into tbUsers(Name, Surname) VALUES(@Name, @Surname);SELECT last_insert_rowid()", CommandType.Text, [("@Name", ("pippo" + rnd.ToString()));("@Surname", "di paperinia")])

//select seq from sqlite_sequence where name="table_name"

match result5 with
| Success x -> printfn "Id inserted (autoincrement): %A" x
| Failure y ->  printfn "errore: %s" y


//If a separate thread performs a new INSERT on the same database connection while the sqlite3_last_insert_rowid() function is running 
//and thus changes the last insert rowid, then the value returned by sqlite3_last_insert_rowid() is unpredictable 
//and might not equal either the old or the new last insert rowid.

//************* using a mini orm for mapping
open SQLData.MappingData

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