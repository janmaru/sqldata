﻿namespace Mahamudra.System.Data.Sql  
open System.Data // IDbConnection - IDbCommand
open System.Data.Common //  DbParameter
open System.Data.SQLite
open System.Data.SqlClient
open System.Data.SqlServerCe
open Mahamudra.System.Railway

//#region factory methods
// connection factory
//Methods with curried arguments cannot be overloaded. 
type Factory =  
    static member createCn(provider:SQLProvider) (connectionString:string) =
        let cn:IDbConnection =
            match provider with
            | SQLite -> new SQLiteConnection(connectionString) :> IDbConnection
            | SQLCe  -> new SqlCeConnection(connectionString) :> IDbConnection
            | _-> new SqlConnection(connectionString) :> IDbConnection 
        cn
    static member createCmd (provider:SQLProvider) (cn:IDbConnection) (sql:string) =
        let cmd:IDbCommand = 
            match provider with
            | SQLite -> new SQLiteCommand(sql,cn:?>SQLiteConnection):> IDbCommand
            | SQLCe -> new SqlCeCommand(sql,cn:?>SqlCeConnection):> IDbCommand
            | _-> new SqlCommand(sql,cn:?>SqlConnection):> IDbCommand 
        cmd

    static member createPmt (provider:SQLProvider) (parameter:string*'paramValue) =
        let prm:DbParameter = 
            match provider with
            | SQLite -> new SQLiteParameter(fst parameter, snd parameter):> DbParameter
            | SQLCe -> new SqlCeParameter(fst parameter, snd parameter):> DbParameter
            | _-> new SqlParameter(fst parameter, snd parameter):> DbParameter 
        prm

type Client =  
    static member  uQuery  (provider:SQLProvider)
                            (connectionString: string) 
                            (commandType:CommandType) 
                            (parameters:seq<string*'paramValue>) 
                            (bind:IDataReader->'Result) 
                            (sql: string)  = 
                seq { 
                    use cn = Factory.createCn provider connectionString 
                    use cmd = Factory.createCmd provider cn sql  
                    cmd.CommandType<-commandType 
                    parameters 
                            |> Seq.iter (fun p-> cmd.Parameters.Add(Factory.createPmt provider p) |> ignore)

                    cn.Open()
                    use reader = cmd.ExecuteReader()
                    while reader.Read() do
                        yield reader |> bind
                }

    static member query (provider:SQLProvider)
                        (connectionString: string) 
                        (commandType:CommandType) 
                        (parameters:seq<string*'paramValue>) 
                        (bind:IDataReader->'Result) 
                        (sql: string)  = 
                try
                    let someValues = [
                        use cn = Factory.createCn provider connectionString 
                        use cmd = Factory.createCmd provider cn sql  
                        cmd.CommandType<-commandType 
                        parameters 
                                |> Seq.iter (fun p-> cmd.Parameters.Add(Factory.createPmt provider p) |> ignore)

                        cn.Open()
                        use reader = cmd.ExecuteReader()
                        while reader.Read() do
                            yield reader |> bind
                        ]
                    Success someValues
                with
                    | ex -> Failure ex.Message
 
     //execute all CRUD mixed commands; useful for getting the autoincremented id
     static member nonQuery (provider:SQLProvider)
                            (connectionString: string) 
                            (commandType:CommandType) 
                            (parameters:seq<string*'paramValue>) 
                            (sql: string)  = 
                    try
                        use cn = Factory.createCn provider connectionString 
                        use cmd = Factory.createCmd provider cn sql  
                        cmd.CommandType<-commandType 
                        parameters 
                                |> Seq.iter (fun p-> cmd.Parameters.Add(Factory.createPmt provider p) |> ignore)

                        cn.Open()
                        let recordsAffected = cmd.ExecuteNonQuery()
                        Success recordsAffected
                    with
                        | ex -> Failure ex.Message
                             
    //execute all CRUD mixed commands; useful for getting the autoincremented id
     static member mixedQuery (provider:SQLProvider)
                              (connectionString: string) 
                              (commandType:CommandType) 
                              (parameters:seq<string*'paramValue>) 
                              (sql: string)  = 
                    try
                        use cn = Factory.createCn provider connectionString 
                        use cmd = Factory.createCmd provider cn sql  
                        cmd.CommandType<-commandType 
                        parameters 
                                |> Seq.iter (fun p-> cmd.Parameters.Add(Factory.createPmt provider p) |> ignore)

                        cn.Open()
                        let value = cmd.ExecuteScalar()
                        Success value
                    with
                        | ex -> Failure ex.Message

//implementation with tuple arguments
module CnFactory = 
    let create (provider:SQLProvider,connectionString:string) =
        let cn:IDbConnection =
            match provider with
            | SQLite -> new SQLiteConnection(connectionString) :> IDbConnection
            | SQLCe  -> new SqlCeConnection(connectionString) :> IDbConnection
            | _-> new SqlConnection(connectionString) :> IDbConnection 
        cn

//command factory
module CmdFactory = 
    let create (provider:SQLProvider,connectionString:string,cn:IDbConnection,sql:string) =
        let cmd:IDbCommand = 
            match provider with
            | SQLite -> new SQLiteCommand(sql,cn:?>SQLiteConnection):> IDbCommand
            | SQLCe -> new SqlCeCommand(sql,cn:?>SqlCeConnection):> IDbCommand
            | _-> new SqlCommand(sql,cn:?>SqlConnection):> IDbCommand 
        cmd

//parameter factory
module PrmFactory = 
    let create (provider:SQLProvider,parameter:string*'paramValue) =
        let prm:DbParameter = 
            match provider with
            | SQLite -> new SQLiteParameter(fst parameter, snd parameter):> DbParameter
            | SQLCe -> new SqlCeParameter(fst parameter, snd parameter):> DbParameter
            | _-> new SqlParameter(fst parameter, snd parameter):> DbParameter 
        prm
//#endregion

module SQLData =

    let createCn (provider:SQLProvider,connectionString:string) =
        try
        let cn = CnFactory.create(provider, connectionString) 
        Success cn
        with
        | ex -> Failure ex.Message

    let createCmd (provider:SQLProvider,connectionString:string,cn:IDbConnection,sql:string) =
        try
        let cmd = CmdFactory.create(provider, connectionString, cn, sql)
        Success cmd
        with
        | ex -> Failure ex.Message
              
    //#region private methods
    //method to check connection, it is not usable outside of this file
    let private simpleCheckConnection (cn:IDbConnection)  =
        try
            try
                cn.Open()
                Success true
            with
                | ex -> Failure ex.Message
        finally
            cn.Close()
    //#endregion

    //method to check connection using a railway switch function
    let checkConnection (provider:SQLProvider,connectionString:string)  =
        createCn (provider,connectionString) 
        >>= simpleCheckConnection

///*******
//A sequence is a logical series of elements all of one type. 
//Sequences are particularly useful when you have a large, ordered collection of data but do not necessarily expect to use all the elements. 
//Individual sequence elements are computed only as required, so a sequence can provide better performance than a list in situations in which not all the elements are used.
///*******

    //query some records, input tuple, no Result output but a simple sequence
    let uQuery (provider:SQLProvider,
                connectionString: string,
                sql: string,
                commandType:CommandType,
                parameters:seq<string*'paramValue>,
                bind:IDataReader->'Result)  = 
                seq { 
                    use cn = CnFactory.create(provider,connectionString)
                    use cmd = CmdFactory.create(provider,connectionString,cn, sql)  
                    cmd.CommandType<-commandType 
                    parameters 
                            |> Seq.iter (fun p-> cmd.Parameters.Add(PrmFactory.create(provider, p)) |> ignore)

                    cn.Open()
                    use reader = cmd.ExecuteReader()
                    while reader.Read() do
                        yield reader |> bind
                }
    
    //query some records in a safe mode, only output as a list is makeable
    let query (provider:SQLProvider,
                connectionString: string,
                sql: string,
                commandType:CommandType,
                parameters:seq<string*'paramValue>,
                bind:IDataReader->'Result)  = 
        try
            let someValues = [
                    use cn =  CnFactory.create(provider,connectionString)
                    use cmd = CmdFactory.create(provider,connectionString,cn, sql)  
                    cmd.CommandType<-commandType 
                    parameters 
                            |> Seq.iter (fun p-> cmd.Parameters.Add(PrmFactory.create(provider, p)) |> ignore)
                      
                    cn.Open()
                    use reader = cmd.ExecuteReader()
                    while reader.Read() do
                        yield reader |> bind
                    ]
            Success someValues
        with
            | ex -> Failure ex.Message
                    
    //execute some command
    let nonQuery (provider:SQLProvider,
                    connectionString: string,
                    sql: string,
                    commandType:CommandType,
                    parameters:seq<string*'paramValue>)  = 
        try
            use cn =  CnFactory.create(provider,connectionString)
            use cmd = CmdFactory.create(provider,connectionString,cn, sql)
            cmd.CommandType<-commandType   
            parameters 
                |> Seq.iter (fun p-> cmd.Parameters.Add(PrmFactory.create(provider, p)) |> ignore)

            cn.Open()
            let recordsAffected = cmd.ExecuteNonQuery()
            Success recordsAffected
        with
            | ex -> Failure ex.Message

    //execute all CRUD mixed commands; useful for getting the autoincremented id
    let mixedQuery (provider:SQLProvider,
                    connectionString: string,
                    sql: string,
                    commandType:CommandType,
                    parameters:seq<string*'paramValue>)  = 
        try
            use cn =  CnFactory.create(provider,connectionString)
            use cmd = CmdFactory.create(provider,connectionString,cn, sql)
            cmd.CommandType<-commandType   
            parameters 
                |> Seq.iter (fun p-> cmd.Parameters.Add(PrmFactory.create(provider, p)) |> ignore)

            cn.Open()
            let value = cmd.ExecuteScalar()
            Success value
        with
            | ex -> Failure ex.Message