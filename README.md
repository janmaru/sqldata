# SQL Data #

This is a quite simple library. Its aim is to provide two simple methods to query data from different SQL providers as **MSSQL Server**, **SQLite** and **SQLCompact** (hopefully MySQL and PostgreSQL will follow soon after).
First of all, we created three factory methods that we can call from all the other functions:
The first one creates a connection that implements the **IDbConnection interface**.

	module CnFactory = 
	        let create (provider:SQLProvider,connectionString:string) =
	            let cn:IDbConnection =
	                match provider with
	                | SQLite -> new SQLiteConnection(connectionString) :> IDbConnection
	                | SQLCe  -> new SqlCeConnection(connectionString) :> IDbConnection
	                | _-> new SqlConnection(connectionString) :> IDbConnection 
	            cn

The second one creates a command that implements the **IDbCommand interface**.

	    module CmdFactory = 
	        let create (provider:SQLProvider,connectionString:string,cn:IDbConnection,sql:string) =
	            let cmd:IDbCommand = 
	                match provider with
	                | SQLite -> new SQLiteCommand(sql,cn:?>SQLiteConnection):> IDbCommand
	                | SQLCe -> new SqlCeCommand(sql,cn:?>SqlCeConnection):> IDbCommand
	                | _-> new SqlCommand(sql,cn:?>SqlConnection):> IDbCommand 
	            cmd
The third one creates a parameter that implements **the abstract class DbParameter**.

	    //parameter factory
	    module PrmFactory = 
	        let create (provider:SQLProvider,parameter:string*'paramValue) =
	            let prm:DbParameter = 
	                match provider with
	                | SQLite -> new SQLiteParameter(fst parameter, snd parameter):> DbParameter
	                | SQLCe -> new SqlCeParameter(fst parameter, snd parameter):> DbParameter
	                | _-> new SqlParameter(fst parameter, snd parameter):> DbParameter 
	            prm

The **SQL Data Library** itself uses these three factory methods in order to create and later use/open connections and use/execute commands.
For istance the first unsafe (we call it this way 'cause we'll not cacth any failure nor bind data into a success type) method called uQuery:

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

It expects a function bind in order to create a seq of a type 'a.

		type User = { UserId:int ; Name:string; Surname:string}
		
		
		let bindUser (reader: IDataReader) =
		    { 
		        UserId = unbox(reader.["ID"])
		        Name = unbox(reader.["Name"])
		        Surname = unbox(reader.["SurName"])
		    } 
		
		let result = uQuery(SQLServer,
		                CN_STRING,
		                "Select * from tbUsers",
		                CommandType.Text, 
		                Seq.empty,
		                bindUser)
		
		let display user =
		        printfn "%A - %s %s" user.UserId  user.Name  user.Surname

And finally:

		Seq.iter display x

Also, we want to provide some structure for functional composition and in order to do so, we're going to use the Railway Pattern. All the functions for this pattern are provided in the Railway.fs file.

'Cause sequences are lazy loaded we're going to differ the previous method using a list of 'a.


    let result3 = query 
                       (SQLServer,
                        CN_STRING ,                       
                        "Select * from tbUsers",
                        CommandType.Text,
                        List.empty,
                        toUser)

    match result3 with
    | Success x -> Seq.iter display x
    | Failure y ->  printfn "errore: %s" y

where query is defined as:

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