# backup
A .NET application to backup databases (mssql, postgresql, mysql) and files to a directory or a dropbox-account.

Used für easily backup databases and files of a server to an external hard-drive or a customer dropbox.

## Setup
- Copy the binaries to a folder any folder you like
- Edit the backup.exe.config and add your sources and targets
- Execute the backup.exe from the command like to test your backups
- Configure a daily based task via windows-task-scheduler to get regulary backups

## Procedure
1. Each dataSource creates its backups
   * mssql: BACKUP function
   * mysql: Dump binary
   * postgres: Dump binary
   * file: The file/directory itself
2. Each backup is zipped with best compression
3. Each zip is saved/uploaded with each dataTarget
   * directory: Copy to a folder
   * dropbox: Uploaded to dropbox
4. Old backups are deleted based on a strategry
   * days: Daily backups for unlimited or specified number of days
   * generations: Daily backups for the last week, weekly backups for the last month, unlimited monthly backups
