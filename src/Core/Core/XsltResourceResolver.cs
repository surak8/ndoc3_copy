// XsltResourceResolver
// Copyright (C) 2004 Kevin Downs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.IO;

namespace NDoc3.Core {
	/// <summary>	
	/// Resolves URLs stored as embedded resources in an assembly.
	/// </summary> 
	/// <remarks>for debugging purposes, it is possible to direct the resolver to look for the resources in a 
	/// disk directory rather than extracting them from the assembly. 
	/// This is especially useful  as it allows the stylesheets to be changed 
	/// and re-run without recompiling the assembly.</remarks>
	public class XsltResourceResolver : XmlUrlResolver {
		private string _ExtensibiltyStylesheet;
		private readonly string[] _ResourceDirs;
		//		private Assembly _Assembly;
		//		private bool _UseEmbeddedResources;
		private readonly Type _embeddedResourceBase;

		/// <summary>
		/// Creates a new instance of the <see cref="XsltResourceResolver"/> class.
		/// </summary>
		/// <param name="resourceDirs">Either, the namespace of the embedded resources, or a file URI to a disk directory where the recources may be found.</param>
		/// <param name="embeddedResourceBase">The type's assembly+namespace indicate the location embedded resources shall be probed from.</param>
		public XsltResourceResolver(Type embeddedResourceBase, params string[] resourceDirs) {
			if (resourceDirs == null)
				resourceDirs = new string[0];

			Trace.WriteLine(string.Format("XSLT probing directories {0}", resourceDirs.Length));
			foreach (string rd in resourceDirs) {
				Trace.WriteLine(string.Format("{0}", rd));
			}

			_ExtensibiltyStylesheet = String.Empty;
			_ResourceDirs = resourceDirs;
			_embeddedResourceBase = embeddedResourceBase;

			//			if (resourceBase.StartsWith("file://"))
			//			{
			//				_ResourceBase=resourceBase.Substring(7);
			//				_UseEmbeddedResources=false;
			//			}
			//			else
			//			{
			//				_ResourceBase = resourceBase;
			//				_Assembly=Assembly.GetCallingAssembly();
			//				_UseEmbeddedResources=true;
			//			}
		}

		/// <summary>
		/// User-defined Extensibility Stylesheet
		/// </summary>
		/// <value>fully-qualified filename of exstensibility stylesheet</value>
		public string ExtensibilityStylesheet {
			get {
				if (_ExtensibiltyStylesheet.Length == 0) { return String.Empty; }
				if (Path.IsPathRooted(_ExtensibiltyStylesheet)) {
					return _ExtensibiltyStylesheet;
				}
				return Path.GetFullPath(_ExtensibiltyStylesheet);
			}
			set { _ExtensibiltyStylesheet = value; }
		}

		/// <summary>
		/// Resolves the absolute URI from the base and relative URIs.
		/// </summary>
		/// <param name="baseUri">The base URI used to resolve the relative URI.</param>
		/// <param name="relativeUri">The URI to resolve. The URI can be absolute or relative. If absolute, this value effectively replaces the <paramref name="baseUri"/> value. If relative, it combines with the <paramref name="baseUri"/> to make an absolute URI.</param>
		/// <returns>A <see cref="Uri"/> representing the absolute URI or <see langword="null"/> if the relative URI can not be resolved.</returns>
		/// <remarks><paramref name="baseUri"/> is always <see langword="null"/> when this method is called from <see cref="System.Xml.Xsl.XslTransform.Load(System.Xml.XmlReader)">XslTransform.Load</see></remarks>
		public override Uri ResolveUri(Uri baseUri, string relativeUri) {
			Uri temp;
			if (relativeUri.StartsWith("res:")) {
				temp = new Uri(relativeUri);
			} else if (relativeUri.StartsWith("user:")) {
				temp = ExtensibilityStylesheet.Length == 0 ? new Uri("res:blank.xslt") : base.ResolveUri(baseUri, ExtensibilityStylesheet);
			} else temp = relativeUri.StartsWith("file:") ? base.ResolveUri(baseUri, relativeUri) : new Uri("res:" + relativeUri);

			return temp;
		}

		private readonly List<string> reportedLocations = new List<string>();

		/// <summary>
		/// Maps a URI to an object containing the actual resource.
		/// </summary>
		/// <param name="absoluteUri">The URI returned from <see cref="ResolveUri"/>.</param>
		/// <param name="role">unused.</param>
		/// <param name="ofObjectToReturn">The type of object to return. The current implementation only returns <b>System.IO.Stream</b> or <b>System.Xml.XmlReader</b> objects.</param>
		/// <returns></returns>
		public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn) {
			Stream xsltStream = null;
			if (absoluteUri.Scheme == "res") {

				if (_ResourceDirs != null) {
					foreach (string _ResourceBase in _ResourceDirs) {
						Uri fileUri = new Uri(_ResourceBase + Path.DirectorySeparatorChar + absoluteUri.AbsolutePath);
						//						Debug.WriteLine(string.Format("Probing {0}", fileUri.AbsoluteUri));
						try {
							xsltStream = base.GetEntity(fileUri, role, Type.GetType("System.IO.Stream")) as Stream;
							if (xsltStream != null) {
								if (!reportedLocations.Contains(fileUri.AbsoluteUri)) {
									reportedLocations.Add(fileUri.AbsoluteUri);
									Trace.WriteLine(string.Format("Using {0}", fileUri.AbsoluteUri));
								}
								break;
							}
						} catch { }
					}
				}

				if (xsltStream == null) {
					string resourceName = _embeddedResourceBase.Namespace + "." + absoluteUri.AbsolutePath;
					string resourceDisplayName = string.Format("assembly:[{0}]{1}", _embeddedResourceBase.Assembly.GetName().Name, resourceName);
					if (!reportedLocations.Contains(resourceDisplayName)) {
						reportedLocations.Add(resourceDisplayName);
						Trace.Write(string.Format("No external found - using embedded {0}", resourceDisplayName));
					}
					xsltStream = _embeddedResourceBase.Assembly.GetManifestResourceStream(resourceName);
				}

				//                if (_UseEmbeddedResources)
				//				{
				//					string resourceName = _ResourceBase + "." + absoluteUri.AbsolutePath;
				//					xsltStream=_Assembly.GetManifestResourceStream(resourceName);
				//				}
				//				else
				//				{
				//					Uri fileUri = new Uri(_ResourceBase + Path.DirectorySeparatorChar + absoluteUri.AbsolutePath);
				//					xsltStream= base.GetEntity(fileUri, role, Type.GetType("System.IO.Stream")) as Stream;
				//				}
			} else {
				Debug.WriteLine(string.Format("Probing {0}", absoluteUri.AbsoluteUri));
				xsltStream = base.GetEntity(absoluteUri, role, Type.GetType("System.IO.Stream")) as Stream;
				if (xsltStream != null) {
					if (!reportedLocations.Contains(absoluteUri.AbsoluteUri)) {
						reportedLocations.Add(absoluteUri.AbsoluteUri);
						Trace.Write(string.Format("Using {0}", absoluteUri.AbsoluteUri));
					}
				}
			}

			if (xsltStream != null
				&& ofObjectToReturn == typeof(XmlReader)) {
				return new XmlTextReader(xsltStream);
			}

			return xsltStream;
		}
	}
}
