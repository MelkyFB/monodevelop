﻿//
// EditorTheme.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using MonoDevelop.Components;
using Xwt.Drawing;
using System.Collections.Generic;
using MonoDevelop.Core;
using System.Linq;
using Cairo;

namespace MonoDevelop.Ide.Editor.Highlighting
{
	public sealed class ThemeSetting 
	{
		public readonly string Name = ""; // not defined in vs.net

		IReadOnlyList<string> scopes;
		public IReadOnlyList<string> Scopes { get { return scopes; } }

		IReadOnlyDictionary<string, string> settings;

		internal IReadOnlyDictionary<string, string> Settings {
			get {
				return settings;
			}
		}

		internal ThemeSetting (string name, IReadOnlyList<string> scopes, IReadOnlyDictionary<string, string> settings)
		{
			Name = name;
			this.scopes = scopes ?? new List<string> ();
			this.settings = settings ?? new Dictionary<string, string> ();
		}

		public bool TryGetSetting (string key, out string value)
		{
			return settings.TryGetValue (key, out value);
		}

		public bool TryGetColor (string key, out HslColor color)
		{
			string value;
			if (!settings.TryGetValue (key, out value)) {
				color = new HslColor (0, 0, 0);
				return false;
			}
			try {
				color = HslColor.Parse (value);
			} catch (Exception e) {
				LoggingService.LogError ("Error while parsing color " + key, e);
				color = new HslColor (0, 0, 0);
				return false;
			}
			return true;
		}

		public override string ToString ()
		{
			return string.Format ("[ThemeSetting: Name={0}]", Name);
		}
	}

	public sealed class EditorTheme
	{
		public readonly static string DefaultThemeName = "Light";
		public readonly static string DefaultDarkThemeName = "Dark";

		public string Name {
			get;
			private set;
		}

		public string Uuid {
			get;
			private set;
		}

		internal string FileName { get; set; }

		List<ThemeSetting> settings;
		internal object CollapsedText;

		public IReadOnlyList<ThemeSetting> Settings {
			get {
				return settings;
			}
		}

		internal EditorTheme (string name) : this (name, new List<ThemeSetting> ())
		{
		}

		internal EditorTheme (string name, List<ThemeSetting> settings) : this (name, settings, Guid.NewGuid ().ToString ())
		{
		}

		internal EditorTheme (string name, List<ThemeSetting> settings, string uuuid)
		{
			if (name == null)
				throw new ArgumentNullException (nameof (name));
			if (settings == null)
				throw new ArgumentNullException (nameof (settings));
			if (uuuid == null)
				throw new ArgumentNullException (nameof (uuuid));
			Name = name;
			this.settings = settings;
			this.Uuid = uuuid;
		}

		HslColor GetColor (string key, string scope)
		{
			HslColor result = default (HslColor);
			foreach (var setting in settings) {
				if (setting.Scopes.Count == 0 || setting.Scopes.Any (s => IsCompatibleScope (s.Trim (), scope))) {
					HslColor tryC;
					if (setting.TryGetColor (key, out tryC))
						result = tryC;
				}
			}
			return result;
		}

		public bool TryGetColor (string scope, string key, out HslColor result)
		{
			bool found = false;
			var foundColor = default (HslColor);
			foreach (var setting in settings) {
				if (setting.Scopes.Count == 0 || setting.Scopes.Any (s => IsCompatibleScope (s, scope))) {
					if (setting.TryGetColor (key, out foundColor))
						found = true;
				}
			}
			if (found) {
				result = foundColor;
				return true;
			}
			result = default (HslColor);
			return false;
		}

		public bool TryGetColor (string key, out HslColor color)
		{
			foreach (var setting in settings) {
				if (setting.TryGetColor (key, out color))
					return true;
			}
			color = default (HslColor);
			return false;
		}

		static bool IsCompatibleScope (string key, string scope)
		{
			var idx = key.IndexOf (' ');
			if (idx >= 0)
				key = key.Substring (0, idx);
			return scope.Contains (key);
		}

		internal ChunkStyle GetChunkStyle (string scope)
		{
			return new ChunkStyle () {
				Name = scope,
				Foreground = GetColor (EditorThemeColors.Foreground, scope),
				Background = GetColor (EditorThemeColors.Background, scope)
			};
		}

		internal Cairo.Color GetForeground (ChunkStyle chunkStyle)
		{
			if (chunkStyle.TransparentForeground)
				return GetColor (EditorThemeColors.Foreground, "");
			return chunkStyle.Foreground;
		}

		internal EditorTheme Clone ()
		{
			return (EditorTheme)this.MemberwiseClone ();
		}
	}
}