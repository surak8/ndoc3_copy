// AssemblySlashDoc.cs - represents an assembly and /doc pair
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

namespace NDoc3.Core {
	/// <summary>Represents the path to an assembly and its associated documentation comment XML file.</summary>
	/// <remarks>Documentation comment XML files are known colloquially as <i>"SlashDoc"</i> files as they produced
	/// by the Microsoft C# compiler when the /doc command-line option is specified. The format of these files is detailed in
	/// the ECMA C# Specification (Appendix E). 
	/// See <see href="http://www.ecma-international.org/publications/standards/Ecma-334.htm">here</see> for further details.
	/// </remarks>
	[Serializable]
	public class AssemblySlashDoc : ICloneable {
		/// <summary>Path to assembly</summary>
		private FilePath assembly;
		/// <summary>Path to slashdoc</summary>
		private FilePath slashDoc;

		/// <overloads>Initializes a new instance of the <see cref="AssemblySlashDoc"/> class.</overloads>
		/// <summary>Initializes a blank instance of the <see cref="AssemblySlashDoc"/> class.</summary>
		public AssemblySlashDoc() {
			assembly = new FilePath();
			slashDoc = new FilePath();
		}

		/// <summary>Initializes a new instance of the <see cref="AssemblySlashDoc"/> class
		/// with the specified Assembly and SlashDoc paths.</summary>
		/// <param name="assemblyFilename">An assembly filename.</param>
		/// <param name="slashDocFilename">A documentation comment XML filename.</param>
		public AssemblySlashDoc(string assemblyFilename, string slashDocFilename) {
			assembly = new FilePath(assemblyFilename);

			slashDoc = slashDocFilename.Length > 0 ? new FilePath(slashDocFilename) : new FilePath();
		}

		/// <summary>
		/// Gets or sets the path to an assembly file.
		/// </summary>
		/// <value>A <see cref="FilePath"/> representing the path to an assembly.</value>
		[PropertyGridUI.FilenameEditor.FileDialogFilter
			 ("Select Assembly",
			 "Library and Executable files (*.dll, *.exe)|*.dll;*.exe|Library files (*.dll)|*.dll|Executable files (*.exe)|*.exe|All files (*.*)|*.*")]
		public FilePath Assembly {
			get { return assembly; }
			set { assembly = value; }
		}

		/// <summary>
		/// Gets or sets the path to a documentation comment XML file.
		/// </summary>
		/// <value>A <see cref="FilePath"/> representing the path to a documentation comment XML file.</value>
		[PropertyGridUI.FilenameEditor.FileDialogFilter
			 ("Select Assembly",
			 "/doc Output files (*.xml)|*.xml|All files (*.*)|*.*")]
		public FilePath SlashDoc {
			get { return slashDoc; }
			set { slashDoc = value; }
		}

		/// <summary>
		/// <see cref="System.ICloneable"/>
		/// </summary>
		/// <returns>Cloned object</returns>
		public object Clone() {
			AssemblySlashDoc ret = new AssemblySlashDoc();
			ret.assembly = new FilePath(assembly);
			ret.slashDoc = new FilePath(slashDoc);

			return ret;
		}
	}
}
