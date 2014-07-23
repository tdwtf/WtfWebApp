param(
	[string]$connectionString
)

try {
	if(!$connectionString) {
		Write-Host "Getting connection string from file: ..\TheDailyWtf\Web.config ..."  -foregroundcolor "yellow"
		[xml]$config = Get-Content ..\TheDailyWtf\Web.config
		$node = $config.SelectSingleNode("/configuration/appSettings/add[@key='InedoLib.DbConnectionString']")
		$connectionString = $node.value
	}
	
	$connectionDetails = New-Object System.Data.SqlClient.SqlConnectionStringBuilder -ArgumentList $connectionString
	
	$testConnection = New-Object System.Data.SqlClient.SqlConnection
	$testConnection.ConnectionString = $connectionString
	Write-Host "Testing connection to database..." -foregroundcolor "magenta"
	try {
		$testConnection.Open()
	}
	catch {
		Write-Error "Could not connect to database server. Make sure the $($connectionDetails.InitialCatalog) database exists and that you have db_owner privileges to modify it. Message: $($_.Exception.Message)"
		exit 1
	}
	finally {
		$testConnection.Close()
	}

	Write-Host "Running BuildMaster changescripter tool to update DDL-DML..." -foregroundcolor "green"
	&.\bmdbupdate.exe UPDATE /conn="$connectionString" /init="yes"

	Write-Host "Updating database functions, stored procedures, and views..." -foregroundcolor "green"

	foreach ($file in (Get-ChildItem .\OBJECTS\*.sql -recurse)) {
		Write-Host "Executing $file.Name script..." -foregroundcolor "gray"
		sqlcmd -S $connectionDetails.DataSource -E -i $file -b -d $connectionDetails.InitialCatalog
	}

	Write-Host "Database update complete."
}
catch {
	Write-Error $_.Exception.Message
}