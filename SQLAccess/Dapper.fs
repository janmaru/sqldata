namespace SQLAccess
    open System.Data // IDbConnection - IDbCommand
    open System.Data.Common //  DbParameter
    open System.Data.SQLite
    open System.Data.SqlClient
    open System.Data.SqlServerCe
    open Railway
    open Dapper

    //simple type, can't define an optional parameter
    type inputDb2 = {connection:IDbConnection; sql:string; param:obj}

    //class with optional parameter
    type inputDb(connection:IDbConnection, sql:string, ?param:obj) = 
            member this.Connection = connection
            member this.Sql = sql
            member this.Param = param

    module DapperFSharp =
        let uQuery<'Result> (connection:IDbConnection,sql:string,param:obj) : 'Result seq =
            match box param with
            | null -> connection.Query<'Result>(sql)
            | _ ->  connection.Query<'Result>(sql, param)

        let query<'Result> (input:inputDb) : 'Result seq =
            match input.Param with
            | Some p -> input.Connection.Query<'Result>(input.Sql, p)
            | None ->  input.Connection.Query<'Result>(input.Sql)
 