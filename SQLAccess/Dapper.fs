namespace SQLAccess
    module DapperFSharp =
        open System.Data.SqlClient
        open System.Dynamic
        open System.Collections.Generic
        open System.Data
        open Dapper

        let dapperQuery<'Result> (connection:IDbConnection)  (sql:string) =
            connection.Query<'Result>(sql)
    
        let dapperParametrizedQuery<'Result> (connection:IDbConnection) (sql:string) (param:obj) : 'Result seq =
            connection.Query<'Result>(sql, param)
//    
//        let dapperMapParametrizedQuery<'Result> (sql:string) (param : Map<string,_>) (connection:IDbConnection) : 'Result seq =
//            let expando = ExpandoObject()
//            let expandoDictionary = expando :> IDictionary<string,obj>
//            for paramValue in param do
//                expandoDictionary.Add(paramValue.Key, paramValue.Value :> obj)
//    
//            connection |> dapperParametrizedQuery sql expando