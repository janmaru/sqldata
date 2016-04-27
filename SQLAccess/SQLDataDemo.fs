namespace SQLAccess
    module SQLDataFSharp =
        open System.Data
        open System.Data.SQLite
        open System.Data.SqlClient
        open System.Data.SqlServerCe

        //method to check connection
        let check (connectionString: string)  =
            use cn = new SQLiteConnection(connectionString)
            cn.Open()
            true

        //method for querying SQLite via sql text
        let queryTextSQLite (connectionString: string) (sql: string) toType  = 
          seq { use cn = new SQLiteConnection(connectionString)
                let cmd = new SQLiteCommand(sql, cn) 
                cmd.CommandType<-CommandType.Text
                        
                cn.Open()
                use reader = cmd.ExecuteReader()
                while reader.Read() do
                    yield reader |> toType
             }

        //sample with tuple of parameters
        let queryTextSQLite2 (connectionString:string,sql:string,toType)  = 
          seq { use cn = new SQLiteConnection(connectionString)
                let cmd = new SQLiteCommand(sql, cn) 
                cmd.CommandType<-CommandType.Text
                        
                cn.Open()
                use reader = cmd.ExecuteReader()
                while reader.Read() do
                    yield reader |> toType
             }

        let queryProcedureSQLite (connectionString: string) (procedure: string) toType  = 
          seq { use cn = new SQLiteConnection(connectionString)
                let cmd = new SQLiteCommand(procedure, cn) 
                cmd.CommandType<-CommandType.StoredProcedure
                        
                cn.Open()
                use reader = cmd.ExecuteReader()
                while reader.Read() do
                    yield reader |> toType
             }

        let querySQLite (connectionString: string) (sql: string) toType (ct:CommandType) = 
          seq { use cn = new SQLiteConnection(connectionString)
                let cmd = new SQLiteCommand(sql, cn) 
                cmd.CommandType<-ct
                        
                cn.Open()
                use reader = cmd.ExecuteReader()
                while reader.Read() do
                    yield reader |> toType
             }

        let queryParametrizedSQLite (connectionString: string) 
                                      (sql: string)
                                      (ct:CommandType)
                                      (parameters:seq<string*'paramValue>)
                                       toType 
                                       = 
          seq { use cn = new SQLiteConnection(connectionString)
                let cmd = new SQLiteCommand(sql, cn) 
                cmd.CommandType<-ct

                parameters 
                    |> Seq.iter (fun x-> cmd.Parameters.Add(new SQLiteParameter((fst x:string), (snd x))) |> ignore)

                cn.Open()
                use reader = cmd.ExecuteReader()
                while reader.Read() do
                    yield reader |> toType
             }

        let nonQueryParametrizedSQLite (connectionString: string) 
                                              (sql: string)
                                              (ct:CommandType)
                                              (parameters:seq<string*'paramValue>)
                                               toType 
                                               = 
                        use cn = new SQLiteConnection(connectionString)
                        let cmd = new SQLiteCommand(sql, cn) 
                        cmd.CommandType<-ct
                        parameters 
                            |> Seq.iter (fun x-> cmd.Parameters.Add(new SQLiteParameter((fst x:string), (snd x))) |> ignore)

                        cn.Open()
                        cmd.ExecuteNonQuery()
 
        type provider = SQLServer | SQLite| SQLCe  

        // The IDbConnection interface enables an inheriting class to implement a Connection class, 
        // which represents a unique session with a data source (for example, a network connection to a server). 
        let queryNOTWORKING (connectionString: string)
                  (target:provider)
                  (sql: string)
                  (ct:CommandType)
                  (parameters:seq<string*'paramValue>)
                  (toType:IDataReader->'Result)  = 

                use cn = 
                    match target with
                    | SQLite -> new SQLiteConnection(connectionString) :> IDbConnection
                    | SQLCe  -> new SqlCeConnection(connectionString) :> IDbConnection
                    | _-> new SqlConnection(connectionString) :> IDbConnection 

                printfn "%s --cn" cn.ConnectionString
                //Individual sequence elements are computed only as required, so a sequence can provide better performance 
                //than a list in situations in which not all the elements are used. 
                seq { 
                        use cmd =   
                            match target with
                            | SQLite -> new SQLiteCommand(sql,cn:?>SQLiteConnection):> IDbCommand
                            | SQLCe -> new SqlCeCommand(sql,cn:?>SqlCeConnection):> IDbCommand
                            | _-> new SqlCommand(sql,cn:?>SqlConnection):> IDbCommand 
                        cmd.CommandType<-ct 
                        
                        printfn "%s --cmd " cmd.Connection.ConnectionString
                        cn.Open()
                        use reader = cmd.ExecuteReader() 
                        while reader.Read() do
                            yield reader |> toType
                    }
        let query2 (connectionString: string)
                  (target:provider)
                  (sql: string)
                  (ct:CommandType)
                  (parameters:seq<string*'paramValue>)
                  (toType:IDataReader->'Result)  = 
                seq { 
                        use cn = 
                            match target with
                            | SQLite -> new SQLiteConnection(connectionString) :> IDbConnection
                            | SQLCe  -> new SqlCeConnection(connectionString) :> IDbConnection
                            | _-> new SqlConnection(connectionString) :> IDbConnection 
                        use cmd =   
                            match target with
                            | SQLite -> new SQLiteCommand(sql,cn:?>SQLiteConnection):> IDbCommand
                            | SQLCe -> new SqlCeCommand(sql,cn:?>SqlCeConnection):> IDbCommand
                            | _-> new SqlCommand(sql,cn:?>SqlConnection):> IDbCommand 
                        cmd.CommandType<-ct 
                        cn.Open()
                        use reader = cmd.ExecuteReader()
                        while reader.Read() do
                            yield reader |> toType
                    }

        let nonQuery2 (connectionString: string)
                  (target:provider)
                  (sql: string)
                  (ct:CommandType )
                  (parameters:seq<string*'paramValue>) = 

                use cn =   
                    match target with
                    | SQLite -> new SQLiteConnection(connectionString) :> IDbConnection
                    | SQLCe  -> new SqlCeConnection(connectionString) :> IDbConnection
                    | _-> new SqlConnection(connectionString) :> IDbConnection 
 
                use cmd =   
                    match target with
                    | SQLite -> new SQLiteCommand(sql,cn:?>SQLiteConnection):> IDbCommand
                    | SQLCe -> new SqlCeCommand(sql,cn:?>SqlCeConnection):> IDbCommand
                    | _-> new SqlCommand(sql,cn:?>SqlConnection):> IDbCommand 
                cmd.CommandType<-ct 
                          
                cn.Open()
                cmd.ExecuteNonQuery()
                 