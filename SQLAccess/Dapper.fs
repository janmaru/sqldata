///*******
//https://github.com/StackExchange/dapper-dot-net
//Performance
////
//A key feature of Dapper is performance. The following metrics show how long it takes to execute 500 SELECT statements against a DB and map the data returned to objects.
////
//The performance tests are broken in to 3 lists:
////
//POCO serialization for frameworks that support pulling static typed objects from the DB. Using raw SQL.
//Dynamic serialization for frameworks that support returning dynamic lists of objects.
//Typical framework usage. Often typical framework usage differs from the optimal usage performance wise. Often it will not involve writing SQL.

//Performance of SELECT mapping over 500 iterations - POCO serialization

//          Method                      Duration
//Hand coded (using a SqlDataReader)	    47ms 
//Dapper ExecuteMapperQuery             	49ms
//ServiceStack.OrmLite (QueryById)      	50ms
//PetaPoco                              	52ms
//BLToolkit	                                80ms
//SubSonic CodingHorror                    107ms
//NHibernate SQL	                       104ms
//Linq 2 SQL ExecuteQuery	               181ms
//Entity framework ExecuteStoreQuery	   631ms

////Performance benchmarks are available here: https://github.com/StackExchange/dapper-dot-net/blob/master/Dapper.Tests/PerformanceTests.cs
///*******

namespace SQLData
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

    module MappingData =

        let uQuery<'a> (provider:SQLProvider,connectionString:string,sql:string, param:obj) : 'a seq =
            use cn = CnFactory.create(provider, connectionString) 
            match box param with
                | null -> cn.Query<'a>(sql)
                | _ ->  cn.Query<'a>(sql, param)
  
        let uQuery2<'a> (input:inputDb) : 'a seq =
            match input.Param with
            | Some p -> input.Connection.Query<'a>(input.Sql, p)
            | None ->  input.Connection.Query<'a>(input.Sql)
 
 //ExecuteScalar with Dapper
 //The reason that we never added one is simply: .Single() does the same thing.  
 //conn.Query<string>(sql).Single();     
     
        let query<'a> (provider:SQLProvider,connectionString:string,sql:string, param:obj) =
            use cn = CnFactory.create(provider, connectionString) 
            try
                let result = 
                    match box param with
                    | null -> cn.Query<'a>(sql)
                    | _ ->  cn.Query<'a>(sql, param)
                Success result
            with
            | ex -> Failure ex.Message

        let nonQuery (provider:SQLProvider,connectionString:string,sql:string, param:obj) =
            use cn = CnFactory.create(provider, connectionString) 
            try
                let result = 
                    match box param with
                    | null -> cn.Execute(sql)
                    | _ ->  cn.Execute(sql, param)
                Success result
            with
            | ex -> Failure ex.Message

 