#r "../packages/Microsoft.Azure.DocumentDB.1.7.1/lib/net45/Microsoft.Azure.Documents.Client.dll"
#r "../packages/Newtonsoft.Json.8.0.3/lib/net45/Newtonsoft.Json.dll"

open System
open Microsoft.Azure.Documents
open Microsoft.Azure.Documents.Client
open Microsoft.Azure.Documents.Linq

let endpointUrl = @"https://fsharp.documents.azure.com:443/"
let authKey = @"UnJnvvTMPk4rkD0R8TNgnh4xnyo3EnZM75QLBrLBgyABE2kHYfGOu4y0st7sf164aBLgRiBKveP9hoaADXRXAA=="

let client = new DocumentClient(new Uri(endpointUrl), authKey) 
let database = new Database()
database.Id <- "FamilyRegistry"
let response = client.CreateDatabaseAsync(database).Result

let documentCollection = new DocumentCollection()
documentCollection.Id <- "FamilyCollection"
let documentCollection' = client.CreateDocumentCollectionAsync(response.Resource.CollectionsLink,documentCollection).Result

type Parent = {firstName:string}
type Pet = {givenName:string}
type Child = {firstName:string; gender:string; grade: int; pets:Pet list}
type Address = {state:string; county:string; city:string}
type family = {id:string; lastName:string; parents: Parent list; children: Child list; address: Address; isRegistered:bool}

let andersenFamily = {id="AndersenFamily"; lastName="Andersen";
                        parents=[{firstName="Thomas"};{firstName="Mary Kay"}];
                        children=[{firstName="Henriette Thaulow";gender="female";
                            grade=5;pets=[{givenName="Fluffy"}]}];
                        address={state = "WA"; county = "King"; city = "Seattle"};
                        isRegistered = true}

client.CreateDocumentAsync(documentCollection'.Resource.DocumentsLink, andersenFamily)

let wakefieldFamily = {id="WakefieldFamily"; lastName="Wakefield";
                        parents=[{firstName="Robin"};{firstName="Ben"}];
                        children=[{firstName="Jesse";gender="female";
                            grade=8;pets=[{givenName="Goofy"};{givenName="Shadow"}]}];
                        address={state = "NY"; county = "Manhattan"; city = "NY"};
                        isRegistered = false}

client.CreateDocumentAsync(documentCollection'.Resource.DocumentsLink, wakefieldFamily)

let queryString = "SELECT * FROM Families f WHERE f.id = \"AndersenFamily\""

//Note - this does not work and I don't know why
let families = client.CreateDocumentQuery(documentCollection'.Resource.DocumentsLink,queryString) 
 
families |> Seq.map(fun f -> f:>family)
         |> Seq.iter(fun f -> printfn "read %A from SQL" f)


//let database = client.CreateDatabaseQuery().Where(fun db -> db.Id = "FamilyRegistry" ).ToArray().FirstOrDefault()
//printfn "%s" database.SelfLink

let database' = client.CreateDatabaseQuery() |> Seq.filter(fun db -> db.Id = "FamilyRegistry")
                                            |> Seq.head
printfn "%s" database.SelfLink 
