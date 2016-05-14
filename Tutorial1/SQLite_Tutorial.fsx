//Display charts ---
// On Mac OSX use FSharp.Charting.Gtk.fsx
#I "../packages/FSharp.Charting.0.90.14"
#load "FSharp.Charting.fsx"

open FSharp.Charting
open System

//Display grid ---
#load @"DisplayGrid.fs"  

//Data ---
#r @"..\packages\System.Data.SQLite.Core.1.0.101.0\lib\net451\System.Data.SQLite.dll"
#r @"..\SQLAccess\bin\Debug\SQLData.dll"


open System
open System.Runtime.InteropServices
open System.IO
open System.Data
open System.Data.SQLite
open Mahamudra.System.Railway
open Mahamudra.System.Data.Sql 
open Mahamudra.System.Data.Sql.SQLData
open Mahamudra.System.Drawing

 
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

type Supplier = { SupplierID:Int64;CompanyName:string;ContactName:string; ContactTitle:string; Address:string}

type NumberOfOrdersByEmployee= { NumberOfOrders:Int64;  EmployeeName:string}

let display supplier =
            printfn "Id: %A - Name: %s - Contact Title:%s" supplier.SupplierID  supplier.CompanyName supplier.ContactTitle

let toSuppliers (reader: IDataReader ) =
    { 
        SupplierID =  unbox(reader.["SupplierID"])
        CompanyName = unbox(reader.["CompanyName"]) 
        ContactTitle = if DBNull.Value.Equals(reader.["ContactTitle"]) then String.Empty else unbox(reader.["ContactTitle"]) 
        ContactName= if DBNull.Value.Equals(reader.["ContactName"]) then String.Empty else unbox(reader.["ContactName"]) 
        Address = if DBNull.Value.Equals(reader.["Address"]) then String.Empty else unbox(reader.["Address"]) 
    }  

let countOrders(reader: IDataReader ) =
    { 
        NumberOfOrders =  unbox(reader.["NumberOfOrders"])
        EmployeeName = unbox(reader.["LastName"]) + " " + unbox(reader.["FirstName"]) 
    }

       
let [<Literal>] DBPATH = __SOURCE_DIRECTORY__ + @"\data\northwind.db" 
//let [<Literal>] DBPATH = __SOURCE_DIRECTORY__ + @"\data\northwind3.db" 
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
let suppliers = SQLData.uQuery (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            "Select SupplierID,CompanyName,ContactTitle,ContactName,Address from [Suppliers]", CommandType.Text, Seq.empty, 
                            toSuppliers)
try
    //---------
    Seq.iter display suppliers //print data on interactive windows
    //---------
    δ.draw (suppliers|>Seq.toList) "Display a list of suppliers" //print in grid windows form
with | ex -> ex.Message |>ignore

//railway pattern query without parameters
let result= SQLData.query (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            "Select * from [Suppliers]", CommandType.Text, Seq.empty, 
                            toSuppliers)
match result with
| Success x -> Seq.iter display x
| Failure y ->  printfn "errore: %s" y

match result with
| Success x -> δ.draw (suppliers|>Seq.toList) "Display a list of suppliers"
| Failure y ->  printfn "errore: %s" y

//query using parameters
let Süßwaren = SQLData.uQuery (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            "Select * from [Suppliers] WHERE SupplierID=@SupplierID", CommandType.Text, [("@SupplierID", 11)] , 
                            toSuppliers)
 
Seq.iter display Süßwaren
//--
δ.draw (Süßwaren|>Seq.toList) "Display Süßwaren"



let [<Literal>] sql_orders = @"SELECT COUNT(Employees.EmployeeID) as NumberOfOrders, Employees.LastName, Employees.FirstName  FROM orders INNER JOIN Employees 
                              ON orders.EmployeeID = employees.EmployeeID 
                              GROUP BY Employees.EmployeeID"

let [<Literal>] sql_orders2 = @"SELECT COUNT(Employees.EmployeeID) as NumberOfOrders, Employees.LastName, Employees.FirstName  FROM orders INNER JOIN Employees 
                              ON orders.EmployeeID = employees.EmployeeID 
                              GROUP BY Employees.EmployeeID
                              ORDER BY COUNT(Employees.EmployeeID) DESC"

let NumberOfOrders = SQLData.uQuery (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            sql_orders, CommandType.Text, Seq.empty, 
                            countOrders)

let NumberOfOrders2 = SQLData.uQuery (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            sql_orders2, CommandType.Text, Seq.empty, 
                            countOrders)
//display of rows
δ.draw (NumberOfOrders|>Seq.toList) "Display NumberOfOrders"
δ.draw (NumberOfOrders2|>Seq.toList) "Display NumberOfOrders"

// Drawing graph of the orders
Chart.Bar (NumberOfOrders |>Seq.map (fun sup-> sup.EmployeeName, sup.NumberOfOrders))

//display ordered list of rows
δ.draw (NumberOfOrders|> Seq.sort|>Seq.toList) "Display NumberOfOrders"
Chart.Column (NumberOfOrders|> Seq.sortDescending |>Seq.map (fun sup-> sup.EmployeeName, sup.NumberOfOrders))


//railway query with parameters
let Süßwaren3 = SQLData.query (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            "Select * from [Suppliers] WHERE SupplierID=@SupplierID", CommandType.Text, [("@SupplierID", 11)] , 
                            toSuppliers)

match Süßwaren3 with
| Success x -> Seq.iter display x
| Failure y ->  printfn "errore: %s" y

match Süßwaren3 with
| Success x -> δ.draw (x|>Seq.toList) "Display Süßwaren"
| Failure y ->  printfn "errore: %s" y

//let rnd = Random().Next passing a function, not a value!!!!
let rnd = Random().Next()

let JohnDoe = SQLData.nonQuery (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            "INSERT into Suppliers(CompanyName,ContactName) VALUES(@CompanyName,@ContactName)", CommandType.Text, [("@CompanyName", ("Random Company " + rnd.ToString()));("@ContactName", "John Doe")])

match JohnDoe with
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

let SupplierID_Dee = SQLData.mixedQuery (SQLite, 
                            sprintf @"Data Source=%s;Version=3;" DBPATH,
                            "INSERT into Suppliers(CompanyName,ContactName) VALUES(@CompanyName,@ContactName);SELECT last_insert_rowid()", CommandType.Text, [("@CompanyName", ("Random Company " + rnd.ToString()));("@ContactName", "Dee")])

//select seq from sqlite_sequence where name="table_name"

match SupplierID_Dee with
| Success x -> printfn "Id inserted (autoincrement): %A" x
| Failure y ->  printfn "errore: %s" y


//If a separate thread performs a new INSERT on the same database connection while the sqlite3_last_insert_rowid() function is running 
//and thus changes the last insert rowid, then the value returned by sqlite3_last_insert_rowid() is unpredictable 
//and might not equal either the old or the new last insert rowid.

//************* using a mini orm for mapping
open MappingData


//only to trigger the interop SQLite check for x64 architecture
let result4 = checkConnection (SQLite, sprintf @"Data Source=%s;Version=3;" DBPATH)

match result4 with
| Success x -> printfn "connessione ok"
| Failure y ->  printfn "errore: %s" y

//F# immutable records are not POCO types; they do not have default constructors,
// or have setters for the properties. 
type Supplier2 = { SupplierID:int64;CompanyName:string;ContactName:string; ContactTitle:string; Address:string; Age:int; Sex:string}

//the F# compiler emits a default constructor and property setters into the generated IL 
//for this type (though those features are not exposed to F# code).  
[<CLIMutable>] 
type Supplier3 = { SupplierID:int64;Name:string;Title:string; Address:string; Age:int; Sex:string}

// query without parameters    
let listOfSuppliers3 = uQuery<Supplier3> (SQLite, 
                                  sprintf @"Data Source=%s;Version=3;" DBPATH,
                                  "Select * from [Suppliers]",
                                  null)
//inference type supplier3
let display3 supplier =
            printfn "%A - %s %s" supplier.SupplierID  supplier.Name supplier.Title

Seq.iter display3 listOfSuppliers3

listOfSuppliers3|> Seq.iter (fun x  ->  printfn "%s" x.Name) 

//query with parameters
[<CLIMutable>] 
type UserSelectArgs = { SelectedSupplierId:int64}

let singleSupplier4 =
        uQuery<Supplier3> (SQLite, 
                        sprintf @"Data Source=%s;Version=3;" DBPATH,
                        "SELECT * From [Suppliers] WHERE SupplierID= @SelectedSupplierId",
                        {SelectedSupplierId=11L})
        |> Seq.head

printfn "%s" singleSupplier4.Title

//result with null values
//val singleSupplier4 : Supplier3 = {SupplierID = 11L;
//                                   Name = null;
//                                   Title = null;
//                                   Address = "Tiergartenstraße 5";
//                                   Age = 0;
//                                   Sex = null;}


//result with type mismatch
[<CLIMutable>] 
type NewSupplier = { MyId:int ; MyName:string; MyContact:string; ContactTitle:string}

let listOfNewSuppliers = uQuery< NewSupplier> (SQLite, 
                                  sprintf @"Data Source=%s;Version=3;" DBPATH,
                                  "SELECT * From [Suppliers]",
                                  null)

listOfNewSuppliers |> Seq.iter (fun x  ->  printfn "%s %s %A %A" x.MyName x.MyContact x.ContactTitle x.MyId) 

DisplayData.draw (listOfNewSuppliers|>Seq.toArray) "Display a list of suppliers"
//using aliases  "SELECT SupplierID as MyId, CompanyName as MyName, ContactName as MyContact, ContactTitle From [Suppliers]"