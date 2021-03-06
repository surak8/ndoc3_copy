// Copyright (C) 2004  Kevin Downs
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.ComponentModel;
using System.IO;

using NDoc3.Core.PropertyGridUI;

namespace NDoc3.Core {
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	[DefaultProperty("Path")]
	[TypeConverter(typeof(TypeConverter))]
	public class PathItemBase {
		#region Static Members
		private static string _basePath;
		/// <summary>
		/// The base path for converting <see cref="PathItemBase"/> path to relative form.
		/// </summary>
		/// <remarks>
		/// If the path has not been explicitly set, it defaults to the working directory.
		/// </remarks>
		public static string BasePath {
			get {
				if (!string.IsNullOrEmpty(_basePath))
					return _basePath;
				return Directory.GetCurrentDirectory();
			}
			set {
				_basePath = value;
			}
		}

		/// <summary>
		/// Explicit conversion of <see cref="PathItemBase"/> to <see cref="String"/>.
		/// </summary>
		/// <param name="path">The <see cref="PathItemBase"/> to convert.</param>
		/// <returns>A string containg the fully-qualified path contained in the passed <see cref="PathItemBase"/>.</returns>
		public static implicit operator String(PathItemBase path) {
			return path._Path;
		}

		#endregion

		#region Private Fields
		private string _Path = "";
		private bool _FixedPath;
		#endregion

		#region Constructors
		/// <overloads>
		/// Initializes a new instance of the <see cref="PathItemBase"/> class.
		/// </overloads>
		/// <summary>
		/// Initializes a new instance of the <see cref="PathItemBase"/> class.
		/// </summary>
		protected PathItemBase() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="PathItemBase"/> class from a given path string.
		/// </summary>
		/// <param name="path">A relative or absolute path.</param>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> is a <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="path"/> is an empty string.</exception>
		/// <remarks>
		/// If a <paramref name="path"/> is rooted, <see cref="FixedPath"/> is set to <see langword="true"/>, otherwise
		/// is is set to <see langword="false"/>
		/// </remarks>
		protected PathItemBase(string path) {
			// Check if the path is specified
			if (path == null)
				throw new ArgumentNullException("path");

			if (path.Length > 0) {
				// Check if the path specified are relative or aboslute
				if (!System.IO.Path.IsPathRooted(path)) {
					path = PathUtilities.RelativeToAbsolutePath(BasePath, path);
					FixedPath = false;
				} else {
					FixedPath = true;
				}
				// Normalize the path according to the current operating system
				_Path = NormalizePath(path);
			} else {
				_Path = "";
				_FixedPath = false;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PathItemBase"/> class from an existing <see cref="PathItemBase"/> instance.
		/// </summary>
		/// <param name="pathItemBase">An existing <see cref="PathItemBase"/> instance.</param>
		/// <exception cref="ArgumentNullException"><paramref name="pathItemBase"/> is a <see langword="null"/>.</exception>
		protected PathItemBase(PathItemBase pathItemBase) {
			if (pathItemBase == null)
				throw new ArgumentNullException("pathItemBase");

			_Path = pathItemBase._Path;
			_FixedPath = pathItemBase._FixedPath;
		}

		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the fully qualified path.
		/// </summary>
		/// <value>The fully qualified path</value>
		/// <exception cref="ArgumentNullException">set <paramref name="value"/> is a <see langword="null"/>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">set <paramref name="value"/> is an empty string.</exception>
		/// <remarks>
		/// If the set path is not rooted, <see cref="FixedPath"/> is set to <see langword="false"/>, otherwise
		/// it left at its current setting.
		/// </remarks>
		[MergableProperty(false)]
		[PropertyOrder(10)]
		public virtual string Path {
			get { return _Path; }
			set {
				if (value == null)
					throw new ArgumentNullException("value");

				if (value.Length == 0)
					throw new ArgumentOutOfRangeException("value", "path must not be empty.");

				if (!System.IO.Path.IsPathRooted(value)) {
					value = PathUtilities.RelativeToAbsolutePath(BasePath, value);
					FixedPath = false;
				}
				_Path = NormalizePath(value);
			}
		}

		/// <summary>
		/// Gets or sets an indication whether the path should be saved as fixed or relative to the project file.
		/// </summary>
		/// <value>
		/// if <see langword="true"/>, NDoc3 will save this as a Fixed path; 
		/// otherwise, it will be saved as a path relative to the NDoc3 project file.
		/// </value>
		[Description("If true, NDoc3 will save this as a fixed path; otherwise, it will be saved as a path relative to the NDoc3 project file.")]
		[DefaultValue(false)]
		[PropertyOrder(20)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public bool FixedPath {
			get { return _FixedPath; }
			set { _FixedPath = value; }
		}

		/// <summary>
		/// Tests, if the path represented by this instance is valid.
		/// </summary>
		public virtual bool Exists { get { throw new NotImplementedException(); } }

		#endregion

		/// <summary>
		/// Normalize the path representation according to the current platform we're running on.
		/// </summary>
		/// <remarks>
		/// The implementation of this method must ensure, that 2 paths pointing to the same location
		/// are considered equal. On Windows, this e.g. means case insensitive comparison of paths.
		/// </remarks>
		protected virtual string NormalizePath(string path) {
			return path;
			//			return PathUtilities.NormalizePath(path);
		}

		internal void SetPathInternal(string path) {
			_Path = NormalizePath(path);
		}

		/// <inheritDoc/>
		public override string ToString() {
			string displayPath = PersistablePath(BasePath);
			return displayPath;
		}

		#region Equality
		/// <inheritDoc/>
		public override bool Equals(object obj) {
			return Equals(obj as PathItemBase);
		}

		/// <inheritDoc/>
		public virtual bool Equals(PathItemBase other) {
			if (ReferenceEquals(other, null)) return false;
			if (GetType() != other.GetType()) return false;

			return ToString() == other.ToString();
		}

		/// <inheritDoc/>
		public override int GetHashCode() {
			return ToString().GetHashCode();
		}

		/// <summary>Equality operator.</summary>
		public static bool operator ==(PathItemBase x, PathItemBase y) {
			if (ReferenceEquals(x, null)) return false;
			return x.Equals(y);
		}
		/// <summary>Inequality operator.</summary>
		public static bool operator !=(PathItemBase x, PathItemBase y) {
			return !(x == y);
		}

		#endregion

		#region Helpers
		internal string PersistablePath(string basePath) {
			string displayPath = _Path;
			displayPath = FixedPath ? FixPath(basePath, displayPath) : RelativePath(displayPath);
			return displayPath;
		}

		private static string FixPath(string BasePath, string path) {
			if (System.IO.Path.IsPathRooted(path)) {
				return path;
			}
			return PathUtilities.RelativeToAbsolutePath(BasePath, path);
		}

		private static string RelativePath(string path) {
			if (System.IO.Path.IsPathRooted(path)) {
				return PathUtilities.AbsoluteToRelativePath(BasePath, path);
			}
			return path;
		}

		#endregion

		// This is a special type converter which will be associated with the PathItemBase class.
		// It converts an PathItemBase object to a string representation for use in a property grid.
		/// <summary>
		/// 
		/// </summary>
		internal class TypeConverter : PropertySorter {
			/// <inheritDoc/>
			public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destType) {
				if (destType == typeof(string) && value is PathItemBase) {
					return value.ToString();
				}
				return base.ConvertTo(context, culture, value, destType);
			}

			/// <inheritDoc/>
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
				if (sourceType == typeof(string)) {
					return true;
				}
				return base.CanConvertFrom(context, sourceType);
			}

			/// <inheritDoc/>
			public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
				if (value is string) {
					return new PathItemBase((string)value);
				}
				return base.ConvertFrom(context, culture, value);
			}
		}
	}
}
