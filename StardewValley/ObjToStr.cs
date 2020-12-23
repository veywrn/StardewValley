using Sickhead.Engine.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

public static class ObjToStr
{
	private struct ToStringDescription
	{
		public Type Type;

		public List<ToStringMember> Members;
	}

	private struct ToStringMember
	{
		public MemberInfo Member;

		private string _name;

		private string _format;

		public string Name
		{
			get
			{
				if (!string.IsNullOrEmpty(_name))
				{
					return _name;
				}
				return Member.Name;
			}
			set
			{
				_name = value;
			}
		}

		public string Format
		{
			get
			{
				if (!string.IsNullOrEmpty(_format))
				{
					return _format;
				}
				return "{0}";
			}
			set
			{
				_format = value;
			}
		}
	}

	public class Style
	{
		public bool ShowRootObjectType;

		public string ObjectDelimiter;

		public string MemberDelimiter;

		public string MemberNameValueDelimiter;

		public bool TrailingNewline;

		public static Style TypeAndMembersSingleLine = new Style
		{
			ShowRootObjectType = true,
			ObjectDelimiter = ":",
			MemberDelimiter = ",",
			MemberNameValueDelimiter = "="
		};

		public static Style MembersOnlyMultiline = new Style
		{
			ShowRootObjectType = false,
			ObjectDelimiter = "",
			MemberDelimiter = "\n",
			MemberNameValueDelimiter = "="
		};

		public Style()
		{
			ShowRootObjectType = true;
			ObjectDelimiter = ":";
			MemberDelimiter = ",";
			MemberNameValueDelimiter = "=";
		}
	}

	private static readonly StringBuilder _stringBuilder = new StringBuilder();

	private static readonly Dictionary<Type, ToStringDescription> _cache = new Dictionary<Type, ToStringDescription>();

	public static string Format(object obj, Style style)
	{
		Type type = obj.GetType();
		_cache.Clear();
		if (!_cache.TryGetValue(obj.GetType(), out ToStringDescription desc))
		{
			ToStringDescription toStringDescription = default(ToStringDescription);
			toStringDescription.Type = type;
			toStringDescription.Members = new List<ToStringMember>();
			desc = toStringDescription;
			_cache.Add(type, desc);
			BindingFlags attrs = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			FieldInfo[] fields = type.GetFields(attrs);
			foreach (FieldInfo k in fields)
			{
				ToStringMember toStringMember = default(ToStringMember);
				toStringMember.Member = k;
				toStringMember.Name = k.Name;
				ToStringMember item = toStringMember;
				Type dataType2 = k.GetDataType();
				if (dataType2 == typeof(string))
				{
					item.Format = "\"{0}\"";
				}
				_ = k.GetDataType().IsArray;
				if (dataType2.HasElementType)
				{
					item.Format = "{1}[{2}] {0}";
				}
				desc.Members.Add(item);
			}
			desc.Members.Sort(CompareToStringMembers);
		}
		lock (_stringBuilder)
		{
			_stringBuilder.Clear();
			if (style.ShowRootObjectType)
			{
				_stringBuilder.Append(desc.Type.Name);
				_stringBuilder.Append(style.ObjectDelimiter);
			}
			for (int j = 0; j < desc.Members.Count; j++)
			{
				ToStringMember i = desc.Members[j];
				Type dataType = i.Member.GetDataType();
				object val = i.Member.GetValue(obj);
				_stringBuilder.Append(dataType.Name);
				_stringBuilder.Append(" ");
				_stringBuilder.Append(i.Name);
				_stringBuilder.Append(style.MemberNameValueDelimiter);
				if (val == null)
				{
					_stringBuilder.Append("null");
				}
				else
				{
					Type vtype = val.GetType();
					if (vtype.HasElementType)
					{
						Type etype = vtype.GetElementType();
						string ecount = "?";
						_stringBuilder.AppendFormat(i.Format, val, etype, ecount);
					}
					else
					{
						_stringBuilder.AppendFormat(i.Format, val);
					}
				}
				if (j != desc.Members.Count - 1)
				{
					_stringBuilder.Append(style.MemberDelimiter);
				}
			}
			return _stringBuilder.ToString();
		}
	}

	private static int CompareToStringMembers(ToStringMember a, ToStringMember b)
	{
		return a.Name.CompareTo(b.Name);
	}
}
