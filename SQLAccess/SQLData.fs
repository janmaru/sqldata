namespace SQLAccess
    open System.Data // IDbConnection - IDbCommand
    open System.Data.Common //  DbParameter
    open System.Data.SQLite
    open System.Data.SqlClient
    open System.Data.SqlServerCe
    open Railway
        
    //we define a type provider of some kind
    type SQLProvider = SQLServer | SQLite| SQLCe 

    //#region factory methods
    // connection factory
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
                      parameters:seq<string*'paramValue>,
                      bind:IDataReader->'Result)  = 
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