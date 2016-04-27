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

        let query2<'Result> (connection:IDbConnection, sql:string) =
            connection.Query<'Result>(sql)
    
        let query3<'Result> (connection:IDbConnection,sql:string,param:obj) : 'Result seq =
            connection.Query<'Result>(sql, param)

//----------
        //simple type, can't define an optional parameter
        type inputDb2 = {connection:IDbConnection; sql:string; param:obj}

        //class with optional parameter
        type inputDb(connection:IDbConnection, sql:string, ?param:obj) = 
                member this.Connection = connection
                member this.Sql = sql
                member this.Param = param

        let query<'Result> (input:inputDb) : 'Result seq =
            match input.Param with
            | Some p -> input.Connection.Query<'Result>(input.Sql, p)
            | None ->  input.Connection.Query<'Result>(input.Sql)
//---------