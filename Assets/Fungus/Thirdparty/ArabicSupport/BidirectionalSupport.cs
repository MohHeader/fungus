using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;

public class BidirectionalSupport
{
	enum DetectionOption
	{
		Depends = 0,
		AsLTR = 1,
		AsRTL = 2,
	}

	static DetectionOption m_DetectNonLatin = DetectionOption.AsRTL;
	static DetectionOption m_DetectWhiteSpace = DetectionOption.Depends;
	static DetectionOption m_DetectNumber = DetectionOption.AsLTR;
	static DetectionOption m_DetectPunctuation = DetectionOption.AsLTR;

	static StringBuilder m_build = new StringBuilder();

	public static string Fix(string original)
	{
		if (string.IsNullOrEmpty(original))
			return original;

		original.Replace("\r\n", "\n");
		m_build.Length = 0;

		int l = 0, e = 0;
		bool onRTL = IsRTLChar(original[0]);
		while (l < original.Length)
		{
			var c = original[l];
			bool ignored = IsIgnoredChar(c);
			if (IsRTLChar(c) != onRTL && !ignored)
			{
				var substring = original.Substring(e, l - e);
				m_build.Append(onRTL ? Reverse(substring) : substring);

				onRTL = IsRTLChar(c);
				e = l;
			} else if (ignored && onRTL && l < original.Length - 1 && IsRTLChar(c = original[l + 1]) != onRTL) {
				var substring = original.Substring(e, l - e);
				m_build.Append(onRTL ? Reverse(substring) : substring);

				onRTL = IsRTLChar(c);
				e = l;
			}
			l++;
		}

		var subs = original.Substring(e);
		m_build.Append(onRTL ? Reverse(subs) : subs);

		return m_build.ToString().Replace("\r\n", "\n");
	}

	static readonly HashSet<char> InternalChars = new HashSet<char>(new char[] {'{', '}', '\\', '_', '^', '[',']'});


	protected static bool IsRTLChar (char c) {
		return (m_DetectNonLatin == DetectionOption.AsRTL && c > '\xFF') ||
			(m_DetectWhiteSpace == DetectionOption.AsRTL && char.IsWhiteSpace(c)) ||
			(m_DetectPunctuation == DetectionOption.AsRTL && char.IsPunctuation(c) && !InternalChars.Contains(c)) ||
			(m_DetectNumber == DetectionOption.AsRTL && char.IsDigit(c));
	}

	protected static bool IsIgnoredChar (char c) {
		return (m_DetectNonLatin == DetectionOption.Depends && c > '\xFF') ||
			(m_DetectWhiteSpace == DetectionOption.Depends && char.IsWhiteSpace(c)) ||
			(m_DetectPunctuation == DetectionOption.Depends && char.IsPunctuation(c) && !InternalChars.Contains(c)) ||
			(m_DetectNumber == DetectionOption.Depends && char.IsDigit(c));
	}

	protected static string Reverse(string original) {
		// Arabic Support by Abdullah Konash
		// https://github.com/Konash/arabic-support-unity/
		return ArabicSupport.ArabicFixer.Fix(original, true, true);
	}
}