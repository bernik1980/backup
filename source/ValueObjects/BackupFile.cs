using System;

namespace ValueObjects
{
	/// <summary>
	/// A file created from a source provider.
	/// </summary>
	public class BackupFile
	{
		/// <summary>
		/// A unique identifier for the file. Will be initialized with Guid.NewGuid().
		/// The identifier will be used to save the file on disk when creating the backup, to be sure, no other file is overwritten.
		/// </summary>
		public string Identifier { get; private set; }
		/// <summary>
		/// The actual timestamp when this backup was created.
		/// </summary>
		public DateTime? CreatedOn { get; set; }
		/// <summary>
		/// The name of this backup. This will be used when saving the backup to a target.
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// The full path of the backup.
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// Creates a new backup file.
		/// The identifier will be initialized with Guid.NewGuid().
		/// </summary>
		/// <param name="directory">The directory where this file shell be saved.</param>
		/// <param name="name">The name of this file when saved to target later.</param>
		public BackupFile(string directory, string name)
		{
			this.Identifier = Guid.NewGuid().ToString();
			this.Name = name;

			this.Path = System.IO.Path.Combine(directory, this.Identifier);
		}

		/// <summary>
		/// Creates a new file from the given path.
		/// The identifier will be the name of the file without extension.
		/// </summary>
		/// <param name="path"></param>
		public BackupFile(string path)
		{
			this.Path = path;
			this.Identifier = System.IO.Path.GetFileNameWithoutExtension(this.Path);
			this.Name = System.IO.Path.GetFileName(this.Path);
		}
	}
}