using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract.Core.Resource;
using Tesseract.LMDB;

namespace Planewalker.Content {

	/// <summary>
	/// <para>
	/// A database key is a variable-length string based key for identifying entries in a database. Keys are
	/// intended to be treated like paths within a file system, although within the database their is no explicit
	/// hierarchical relationship.
	/// </para>
	/// <para>Note that a database may only support keys of up to a certain length.</para>
	/// </summary>
	public struct DatabaseKey {

		/// <summary>
		/// The path separator character.
		/// </summary>
		public const char Separator = '/';

		/// <summary>
		/// The text representation of the key.
		/// </summary>
		public string PathText { get; } = string.Empty;

		private byte[]? mem = null;

		/// <summary>
		/// The binary form of the key.
		/// </summary>
		public ReadOnlyMemory<byte> PathBinary {
			get {
				mem ??= Encoding.UTF8.GetBytes(PathText);
				return mem;
			}
		}

		public DatabaseKey() { }

		public DatabaseKey(string str) {
			PathText = str;
		}

		public static DatabaseKey operator /(DatabaseKey left, string right) => new(left.PathText + Separator + right);

		public static implicit operator DatabaseKey(string str) => new(str);

		public static implicit operator string(DatabaseKey key) => key.PathText;

	}

	public class DatabaseTable {

		public Database Database { get; }

		public string Name { get; }

		public uint DBI { get; }

		internal DatabaseTable(Database db, string name) {
			Database = db;
			Name = name;
			var txn = Database.Environment.Begin();
			DBI = txn.Open(Name, MDBDBFlags.Create);
			txn.Commit();
		}

	}

	public class Database : IDisposable {

		public string FileName { get; }

		public MDBEnv Environment { get; }

		private readonly Dictionary<string, DatabaseTable> tables = new();

		public Database(string fileName) {
			FileName = fileName;
			Environment = new() { MaxDBs = 16 };
			Environment.Open(fileName, MDBEnvFlags.NoSubDir);
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			Environment.Dispose();
		}

		public DatabaseTable this[string name] {
			get {
				if (tables.TryGetValue(name, out DatabaseTable? table)) return table;
				table = new DatabaseTable(this, name);
				tables[name] = table;
				return table;
			}
		}

	}

	/// <summary>
	/// Stream implementation which wraps an entry in a <see cref="DatabaseTable"/>.
	/// </summary>
	public class DatabaseStream : Stream {

		/// <summary>
		/// The table this stream's entry is stored in.
		/// </summary>
		public DatabaseTable Table { get; }

		/// <summary>
		/// The key in the database identifying this entry.
		/// </summary>
		public DatabaseKey Key { get; }

		// The transaction used to interact with the database
		private MDBTxn transaction;

		private MemoryStream? entryMemory = null;
		private bool dirty = false;

		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite { get; }

		public override long Length => entryMemory?.Length ?? 0;

		public DatabaseStream(DatabaseTable table, DatabaseKey key, bool isReadOnly = true) {
			Table = table;
			Key = key;
			CanWrite = !isReadOnly;
			transaction = table.Database.Environment.Begin(null, isReadOnly ? MDBEnvFlags.ReadOnly : default);
		}

		public override long Position {
			get => entryMemory?.Position ?? 0;
			set {
				if (entryMemory!= null) entryMemory.Position = value;
			}
		}

		private MemoryStream CheckMemory() {
			// If we don't have a copy of the entry's memory, try to read it from the database or create new memory and mark as dirty
			if (entryMemory == null) {
				if (transaction.TryGet(Table.DBI, Key.PathBinary.Span, out Span<byte> data)) entryMemory = new MemoryStream(data.ToArray());
				else {
					entryMemory = new MemoryStream();
					dirty = true;
				}
			}
			return entryMemory;
		}

		private void WritebackMemory() {
			// Skip this process if this is a readonly entry
			if (!CanWrite) return;
			// If dirty, store the entry's memory back to the database
			if (dirty) {
				if (entryMemory != null) {
					Span<byte> data = entryMemory.GetBuffer();
					transaction.Put(Table.DBI, Key.PathBinary.Span, ref data);
				}
				dirty = false;
			}
		}

		public override void Flush() {
			// Commit what we have and start a new transaction
			WritebackMemory();
			transaction.Commit();
			transaction = Table.Database.Environment.Begin(null, CanWrite ? default : MDBEnvFlags.ReadOnly);
			// Reset our memory and seek to the same position if not 0
			long pos = Position;
			entryMemory = null;
			if (pos != 0) CheckMemory().Position = pos;
		}

		public override int Read(byte[] buffer, int offset, int count) => CheckMemory().Read(buffer, offset, count);

		public override long Seek(long offset, SeekOrigin origin) => CheckMemory().Seek(offset, origin);

		public override void SetLength(long value) {
			if (!CanWrite) throw new NotSupportedException();
			CheckMemory().SetLength(value);
			dirty = true;
		}

		public override void Write(byte[] buffer, int offset, int count) {
			if (!CanWrite) throw new NotSupportedException();
			CheckMemory().Write(buffer, offset, count);
			dirty = true;
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				WritebackMemory();
				transaction.Commit();
				entryMemory?.Dispose();
			}
		}

	}

	public class DatabaseResourceDomain : ResourceDomain {

		public DatabaseTable Table { get; }

		public override bool Writable { get; }

		public DatabaseResourceDomain(string name, DatabaseTable table, bool isReadOnly = true) : base(name) {
			Table = table;
			Writable = !isReadOnly;
		}

		public override IEnumerable<ResourceLocation> EnumerateDirectory(ResourceLocation dir) {
			List<ResourceLocation> resources = new();
			using var txn = Table.Database.Environment.Begin(parent: null, MDBEnvFlags.ReadOnly);
			var cursor = txn.OpenCursor(Table.DBI);
			Span<byte> key1 = Encoding.UTF8.GetBytes(dir.Path);
			Span<byte> key2 = default;
			MDBCursorOp op = MDBCursorOp.First;
			while(cursor.Get(ref key2, out _, op)) {
				op = MDBCursorOp.Next;
				// Skip if not a longer path
				if (key2.Length <= key1.Length) continue;
				// Only add if the prefix matches
				if (key1.SequenceEqual(key2[..key1.Length])) {
					resources.Add(new ResourceLocation(this, Encoding.UTF8.GetString(key2)));
				}
			}
			txn.Abort();
			return resources;
		}

		public override bool Exists(ResourceLocation file) {
			using var txn = Table.Database.Environment.Begin(parent: null, MDBEnvFlags.ReadOnly);
			bool exist = txn.TryGet(Table.DBI, Encoding.UTF8.GetBytes(file.Path), out _);
			txn.Abort();
			return exist;
		}

		public override ResourceMetadata GetMetadata(ResourceLocation file) {
			using var txn = Table.Database.Environment.Begin(parent: null, MDBEnvFlags.ReadOnly);
			if (!txn.TryGet(Table.DBI, Encoding.UTF8.GetBytes(file.Path), out Span<byte> data)) return default;

			MIME.TryGuessFromExtension(FileResourceDomain.GetExtensionFromFileName(file.Name), out string? mime);

			return new ResourceMetadata() {
				Local = true,
				Size = data.Length,
				MIMEType = mime
			};
		}

		public override Stream OpenStream(ResourceLocation file) => new DatabaseStream(Table, file.Path, isReadOnly: false);

	}
}
