# SQL-InfoSchema-App
ASP.NET Core MVC application that will help you formulate SQL select queries and store them in a database table for later runtime use by your application(s).
Unique interface is built by reading the SQL InformationSchema to determine tables and relationships. Sample code, generated by the app, shows how to implement a stored query in a Web API controller. 

** **THIS IS A VERY EARLY LOOK (APRIL-2019) -- STAY TUNED FOR UPDATES**

[Tweet](http://twitter.com/home?status=SQL%20Query%20Builder/Runner%20with%20Dapper%20https://github.com/netbizsystems/SQL-InfoSchema-App)

### Pick a Table

After selecting a base table for your query, related tables are automatically selected based on declaritive references found in your schema. Those FK references will be included in your query using a JOIN. Don't worry, you can exclude any of them on the next page.

![InfoView](infoview.png?raw=true)

### Tweak and Save the Generated Query

Sample data is queried from your database to make it easy to see exactly what the data looks like in the table(s) that you have chosen. Exclude a related table if it is not of interest and/or exclude columns that are also not of interest. Click the **Run Query** button to see which rows will be returned from your query as defined.

Click **Save SQL Query** to save your query to the SQL query store (table) and make it available for use in any application that wants to use that queried result.

![QueryView](queryview.png?raw=true)

### See The Code

The cloned solution has a class library with several helpful classes to work with your query. This example shows how the **QueryRunnerController** class (from .Net Controller) can be used to surface your query in a client application. **MUCH** more coming here... stay tuned!!

![CodeView](codeview.png?raw=true)

### Manage and Test Your Queries (coming soon)



![InfoView](libraryview.png?raw=true)


## Prerequisites

* Visual Studio 2017
* .NET Core 2.0 (https://www.microsoft.com/net/core)

## Setup Requirements to Run Local

* Open the SqlInformationSolution.sln solution in Visual Studio.
* Create the required database objects using the "Create AES.InfoQuery.sql" found in the solution items folder.
* Update the database connection requirements (shown below) in appsettings.json.
* Build and Run (F5)

**appsettings.json** - requires two connection strings, one for the app meta-data and the other for the database that you are writing queries against. The SQL/DDL script for the meta-data tables can be found in the **Solution Items** folder.
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=.;Initial Catalog=AdventureWorksLT2014;Integrated Security=True;MultipleActiveResultSets=True;App=SQLInfoApp",
    "InfoStoreConnection": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SQLInformationStore;Integrated Security=True;"
  }
}
```

