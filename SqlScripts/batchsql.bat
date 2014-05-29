@ECHO OFF

SET BLD_DDLVER=%1
SET BLD_SQLSERV=%2
SET BLD_DBNAME=%3
SET BLD_NOPAUSE=%4

SET BLD_SQLBASEDIR=.\
IF "%BLD_DBNAME%"=="" SET BLD_DBNAME=TheDailyWtf2
IF "%BLD_SQLSERV%"=="" SET BLD_SQLSERV=localhost

ECHO ****************************************
ECHO Batch Start (%BLD_SQLBASEDIR%*.sql)
ECHO ****************************************
ECHO.
ECHO ** Database: %BLD_DBNAME% 
ECHO.



IF "%BLD_DDLVER%"=="ALL" GOTO DDLDMLALL
IF "%BLD_DDLVER%"=="" GOTO DDLDMLNONE


:DDLDMLSOME
ECHO ** DDL-DML Version #%BLD_DDLVER% **
FOR /R %BLD_SQLBASEDIR%DDL-DML\%BLD_DDLVER% %%f IN (*.sql) DO (
  ECHO Running %%f
  OSQL -S "%BLD_SQLSERV%" -E -i "%%f" -n -b -d %BLD_DBNAME%
  IF ERRORLEVEL 1 GOTO END
)
GOTO DDLDMLEND

:DDLDMLALL
ECHO ** EXECUTING ALL DDL-DML **
..\BuildMasterSolution\Tools\BuildMaster.Data.ChangeScripter\bin\Debug\bmdbcser.exe REBUILD
FOR /R %BLD_SQLBASEDIR%DDL-DML %%f IN (*.sql) DO (
  ECHO Running %%f
  OSQL -S "%BLD_SQLSERV%" -E -i "%%f" -n -b -d %BLD_DBNAME%
  IF ERRORLEVEL 1 GOTO END
)
GOTO DDLDMLEND

:DDLDMLNONE IF "%BLD_DDLVER%"=="" ECHO ** No DDL-DML To Execute **
..\BuildMasterSolution\Tools\BuildMaster.Data.ChangeScripter\bin\Debug\bmdbcser.exe SYNC

:DDLDMLEND



ECHO.
ECHO ** OBJECTS **
FOR /R %BLD_SQLBASEDIR%OBJECTS %%f IN (*.sql) DO (
  ECHO Running %%f
  OSQL -S "%BLD_SQLSERV%" -E -i "%%f" -n -b -d %BLD_DBNAME%
  IF ERRORLEVEL 1 GOTO END
)


GOTO END

:ERROR
ECHO BATCH ABORTED DUE TO ERROR

:END
ECHO.
ECHO ****************************************
ECHO Batch Execution Complete (%BLD_SQLBASEDIR%*.sql)
ECHO ****************************************

IF "%BLD_NOPAUSE%" == "NOPAUSE" GOTO EXIT
PAUSE

:EXIT
SET BLD_SQLBASEDIR=
SET BLD_SQLSERV=
SET BLD_DDLVER=
SET BLD_NOPAUSE=
