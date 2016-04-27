namespace SQLAccess
    module SQLData =
        open System.Data
        open System.Data.SQLite
        open System.Data.SqlClient
        open System.Data.SqlServerCe
        open Railway

        type SQLProvider = SQLServer | SQLite| SQLCe  
        
        let createCn (provider:SQLProvider,connectionString:string) =
         try
            let cn:IDbConnection =
              match provider with
                | SQLite -> new SQLiteConnection(connectionString) :> IDbConnection
                | SQLCe  -> new SqlCeConnection(connectionString) :> IDbConnection
                | _-> new SqlConnection(connectionString) :> IDbConnection 
            Success cn
         with
            | ex -> Failure ex.Message

        let createCmd (provider:SQLProvider,connectionString:string,cn:IDbConnection,sql:string) =
          try
            let cmd:IDbCommand = 
                match provider with
                | SQLite -> new SQLiteCommand(sql,cn:?>SQLiteConnection):> IDbCommand
                | SQLCe -> new SqlCeCommand(sql,cn:?>SqlCeConnection):> IDbCommand
                | _-> new SqlCommand(sql,cn:?>SqlConnection):> IDbCommand 
            Success cmd
          with
            | ex -> Failure ex.Message
              
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

        let private justCreateCn (provider:SQLProvider,connectionString:string) =
            let cn:IDbConnection =
                match provider with
                | SQLite -> new SQLiteConnection(connectionString) :> IDbConnection
                | SQLCe  -> new SqlCeConnection(connectionString) :> IDbConnection
                | _-> new SqlConnection(connectionString) :> IDbConnection 
            cn
 
        let private justCreateCmd (provider:SQLProvider,connectionString:string,cn:IDbConnection,sql:string) =
            let cmd:IDbCommand = 
                match provider with
                | SQLite -> new SQLiteCommand(sql,cn:?>SQLiteConnection):> IDbCommand
                | SQLCe -> new SqlCeCommand(sql,cn:?>SqlCeConnection):> IDbCommand
                | _-> new SqlCommand(sql,cn:?>SqlConnection):> IDbCommand 
            cmd
 
                     
       //method to check connection
        let checkConnection (provider:SQLProvider,connectionString:string)  =
            createCn (provider,connectionString) 
            >>= simpleCheckConnection

       //query some records
        let unsafeQuery (
                        connectionString: string,
                        provider:SQLProvider,
                        sql: string,
                        ct:CommandType,
                        parameters:seq<string*'paramValue>,
                        toType:IDataReader->'Result)  = 
                 seq { 
                        use cn = justCreateCn(provider,connectionString)
                        use cmd = justCreateCmd(provider,connectionString,cn, sql)  
                        cmd.CommandType<-ct 
                        cn.Open()
                        use reader = cmd.ExecuteReader()
                        while reader.Read() do
                            yield reader |> toType
                   }
    
        let query (connectionString: string,
                   provider:SQLProvider,
                   sql: string,
                   ct:CommandType,
                   parameters:seq<string*'paramValue>,
                   toType:IDataReader->'Result)  = 
           try
              let someValues = seq { 
                        use cn = justCreateCn(provider,connectionString)
                        use cmd = justCreateCmd(provider,connectionString,cn, sql)  
                        cmd.CommandType<-ct 
                        cn.Open()
                        use reader = cmd.ExecuteReader()
                        while reader.Read() do
                            yield reader |> toType
                   }
              Success someValues
           with
             | ex -> Failure ex.Message

        //query some records
        let queryList (connectionString: string)
                (provider:SQLProvider)
                (sql: string)
                (ct:CommandType)
                (parameters:seq<string*'paramValue>)
                (toType:IDataReader->'Result)  = 
           try
              let someValues = [
                        use cn = justCreateCn(provider,connectionString)
                        use cmd = justCreateCmd(provider,connectionString,cn, sql)  
                        cmd.CommandType<-ct 
                        cn.Open()
                        use reader = cmd.ExecuteReader()
                        while reader.Read() do
                            yield reader |> toType
                       ]
              Success someValues
           with
             | ex -> Failure ex.Message
                    
        //execute some command
        let nonQuery (connectionString: string)
                  (provider:SQLProvider)
                  (sql: string)
                  (ct:CommandType )
                  (parameters:seq<string*'paramValue>) = 
            try
                use cn = justCreateCn(provider,connectionString)
                use cmd = justCreateCmd(provider,connectionString,cn, sql)  
                cmd.CommandType<-ct 
                cn.Open()
                let recordsAffected = cmd.ExecuteNonQuery()
                Success recordsAffected
            with
                | ex -> Failure ex.Message