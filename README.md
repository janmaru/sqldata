# SQL Data #

This is a quite simple library. Its aim is to provide two simple methods to query data from different SQL providers as MSSQL Server, SQLite e SQLCompact (happily will follow MySQL and PostgreSQL).
The first method is called unsafeQuery:

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

It expects a function toType in order to create a seq of a type 'a.

		type User = { UserId:int ; Name:string; Surname:string}


		let toUser (reader: IDataReader) =
		    { 
		        UserId = unbox(reader.["ID"])
		        Name = unbox(reader.["Name"])
		        Surname = unbox(reader.["SurName"])
		    } 

		let result = unsafeQuery 
		                        (CN_STRING, 
		                        SQLServer,
		                        "Select * from tbUsers",
		                        CommandType.Text, 
		                        Seq.empty, 
		                        toUser)


	    let display user =
	            printfn "%A - %s %s" user.UserId  user.Name  user.Surname

And finally:

		Seq.iter display x

Also, we want to provide some structure for functional composition and in order to do so, we're going to use the Railway Pattern. All the functions for this pattern are provided in Railway.fs

'Cause sequences are lazy loaded we're going to differ the previous method using a list of 'a.


		 let result3 = queryList 
		                        CN_STRING 
		                        SQLServer 
		                        "Select * from tbUsers" 
		                        CommandType.Text 
		                        List.empty 
		                        toUser
		
		    match result3 with
		    | Success x -> Seq.iter display x
		    | Failure y ->  printfn "errore: %s" y

where queryList is defined as:


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