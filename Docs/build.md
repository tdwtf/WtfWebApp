### Build Instructions:

 1) Ensure a SQL Server database exists (the default name is *TheDailyWtf2*)
 2) Run the **UpdateSql.ps1** PowerShell script to update the database schema
 3) Ensure an IIS website exists with its home directory set to the full path at: **..\TheDailyWtf**
 4) The IIS application pool should be ASP.NET 4.0 Integrated mode
 5) Edit the **Discourse.Username** and **Discourse.ApiKey** values in the *web.config* file at the solution root to your own forum username and password
 6) Rebuild the **TheDailyWtf.sln ** solution