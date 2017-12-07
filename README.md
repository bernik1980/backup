# backup
A .NET application to backup databases and files to another directory or a dropbox-account.

## Supported databases
- Oracle
- IBM DB2
- Microsoft-SQL-Server and Microsoft-SQL-Server Express Edition
- MySQL
- Postgre SQL
- SQLite

## Other supported sources
- directories
- files

## Setup
- Copy the binaries to a folder any folder you like
- Edit the backup.exe.config and add your sources and targets
- Execute the backup.exe from the command like to test your backups
- Configure a daily based task via windows-task-scheduler to get regulary backups

## Procedure
1. Each dataSource creates its backups
   * oracle: RMAN binary
   * mssql: BACKUP function
   * mysql: Dump binary
   * postgres: Dump binary
   * sqlite: Backup api of the ado.net provider
   * file: The file/directory itself
2. Each backup is zipped with best compression
3. Each zip is saved/uploaded with each dataTarget
   * directory: Copy to a folder
   * dropbox: Uploaded to dropbox
4. Old backups are deleted based on a strategry
   * days: Daily backups for unlimited or specified number of days
   * generations: Daily backups for the last week, weekly backups for the last month, unlimited monthly backups
