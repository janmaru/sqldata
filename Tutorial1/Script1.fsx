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

module AccessUser = 
    //only to trigger the interop SQLite check
    check @"Data Source=D:\PAKET_PRESENTATION\SQLAccess\Tutorial1\bin\Debug\test.db;Version=3;"
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


    //F# immutable records are not POCO types; they do not have default constructors,
    // or have setters for the properties. 
    type User2 = { Id:int ; Name:string; Surname:string; Age:int}
  
    //the F# compiler emits a default constructor and property setters into the generated IL 
    //for this type (though those features are not exposed to F# code).  
    [<CLIMutable>] 
    type User = { Id:int ; Name:string; Surname:string; Age:int}
    
    let getUsers connection =
        uQuery<User> (connection,"SELECT * From tbUsers",null)

    
    let listOfUser = getUsers (new SQLiteConnection(@"Data Source=D:\PAKET_PRESENTATION\SQLAccess\Tutorial1\bin\Debug\test.db;Version=3;"))
    listOfUser |> Seq.iter (fun x  ->  printfn "%s" x.Name) 

    [<CLIMutable>] 
    type UserSelectArgs = { SelectedUserId:int}

    let getUser userId connection =
        uQuery<User> (connection,"SELECT ID, Surname From tbUsers WHERE ID = @SelectedUserId",{SelectedUserId=userId})
        |> Seq.head

    let singleUser = getUser 1 (new SQLiteConnection(@"Data Source=D:\PAKET_PRESENTATION\SQLAccess\Tutorial1\bin\Debug\test.db;Version=3;"))
    printfn "%s" singleUser.Surname

//
//    // Values that are intended to be constants can be marked with the Literal attribute. 
//    // This attribute has the effect of causing a value to be compiled as a constant.
//    let [<Literal>] resolutionPath = __SOURCE_DIRECTORY__ + @"\data" 
//    let [<Literal>] connectionString = "Data Source=" + __SOURCE_DIRECTORY__ + @"\test.db;Version=3"

 