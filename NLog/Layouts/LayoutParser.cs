
namespace NLog.Layouts
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Reflection;
	using System.Text;
	using NLog.Common;
	using NLog.Conditions;
	using NLog.Config;
	using NLog.Internal;
	using NLog.LayoutRenderers;
	using NLog.LayoutRenderers.Wrappers;

	/// <summary>
	/// Parses layout strings.
	/// </summary>
	internal sealed class LayoutParser
	{
		internal static LayoutRenderer[] CompileLayout(LoggingConfiguration cfg, string text)
		{
			string parsed;
			
			return CompileLayout(cfg, new SimpleStringReader(text), false, out parsed);
		}

	
		private static LayoutRenderer[] CompileLayout(LoggingConfiguration cfg, SimpleStringReader sr, bool isNested, out string text)
		{
			var result = new List<LayoutRenderer>();
			var literalBuf = new StringBuilder();

			int ch;

			int p0 = sr.Position;

			while ((ch = sr.Peek()) != -1)
			{
				if (isNested && (ch == '}' || ch == ':'))
				{
					break;
				}

				sr.Read();

				if (ch == '$' && sr.Peek() == '{')
				{
					if (literalBuf.Length > 0)
					{
						result.Add(new LiteralLayoutRenderer(literalBuf.ToString()));
						literalBuf.Length = 0;
					}

					LayoutRenderer newLayoutRenderer = ParseLayoutRenderer(cfg, sr);
					if (CanBeConvertedToLiteral(newLayoutRenderer))
					{
						newLayoutRenderer = ConvertToLiteral(cfg, newLayoutRenderer);
					}

					// layout renderer
					result.Add(newLayoutRenderer);
				}
				else
				{
					literalBuf.Append((char)ch);
				}
			}

			if (literalBuf.Length > 0)
			{
				result.Add(new LiteralLayoutRenderer(literalBuf.ToString()));
				literalBuf.Length = 0;
			}

			int p1 = sr.Position;

			MergeLiterals(result);
			text = sr.Substring(p0, p1);

			return result.ToArray();
		}

		private static string ParseLayoutRendererName(SimpleStringReader sr)
		{
			int ch;

			var nameBuf = new StringBuilder();
			while ((ch = sr.Peek()) != -1)
			{
				if (ch == ':' || ch == '}')
				{
					break;
				}

				nameBuf.Append((char)ch);
				sr.Read();
			}

			return nameBuf.ToString();
		}

		private static string ParseParameterName(SimpleStringReader sr)
		{
			int ch;
			int nestLevel = 0;

			var nameBuf = new StringBuilder();
			while ((ch = sr.Peek()) != -1)
			{
				if ((ch == '=' || ch == '}' || ch == ':') && nestLevel == 0)
				{
					break;
				}

				if (ch == '$')
				{
					sr.Read();
					nameBuf.Append('$');
					if (sr.Peek() == '{')
					{
						nameBuf.Append('{');
						nestLevel++;
						sr.Read();
					}

					continue;
				}

				if (ch == '}')
				{
					nestLevel--;
				}

				if (ch == '\\')
				{
					// skip the backslash
					sr.Read();

					// append next character
					nameBuf.Append((char)sr.Read());
					continue;
				}

				nameBuf.Append((char)ch);
				sr.Read();
			}

			return nameBuf.ToString();
		}

		private static string ParseParameterValue(SimpleStringReader sr)
		{
			int ch;

			var nameBuf = new StringBuilder();
			while ((ch = sr.Peek()) != -1)
			{
				if (ch == ':' || ch == '}')
				{
					break;
				}

				if (ch == '\\')
				{
					// skip the backslash
					sr.Read();

					// append next character
					nameBuf.Append((char)sr.Read());
					continue;
				}

				nameBuf.Append((char)ch);
				sr.Read();
			}

			return nameBuf.ToString();
		}

		private static LayoutRenderer ParseLayoutRenderer(LoggingConfiguration cfg, SimpleStringReader sr)
		{
			int ch = sr.Read();
			Debug.Assert(ch == '{', "'{' expected in layout specification");

			string name = ParseLayoutRendererName(sr);
			LayoutRenderer lr = cfg.ItemFactory.LayoutRenderers.CreateInstance(name);

			var wrappers = new Dictionary<Type, LayoutRenderer>();
			var orderedWrappers = new List<LayoutRenderer>();

			ch = sr.Read();
			while (ch != -1 && ch != '}')
			{
				string parameterName = ParseParameterName(sr).Trim();
				if (sr.Peek() == '=')
				{
					sr.Read(); // skip the '='
					PropertyInfo pi;
					LayoutRenderer parameterTarget = lr;

					if (!PropertyHelper.TryGetPropertyInfo(lr, parameterName, out pi))
					{
						Type wrapperType;

						if (cfg.ItemFactory.AmbientProperties.TryGetDefinition(parameterName, out wrapperType))
						{
							LayoutRenderer wrapperRenderer;

							if (!wrappers.TryGetValue(wrapperType, out wrapperRenderer))
							{
								wrapperRenderer = cfg.ItemFactory.AmbientProperties.CreateInstance(parameterName);
								wrappers[wrapperType] = wrapperRenderer;
								orderedWrappers.Add(wrapperRenderer);
							}

							if (!PropertyHelper.TryGetPropertyInfo(wrapperRenderer, parameterName, out pi))
							{
								pi = null;
							}
							else
							{
								parameterTarget = wrapperRenderer;
							}
						}
					}

					if (pi == null)
					{
						ParseParameterValue(sr);
					}
					else
					{
						if (typeof(Layout).IsAssignableFrom(pi.PropertyType))
						{
							string txt;
							LayoutRenderer[] renderers = CompileLayout(cfg, sr, true, out txt);
							var nestedLayout = new SimpleLayout(txt, renderers);
							pi.SetValue(parameterTarget, nestedLayout, null);
						}
						else if (typeof(ConditionExpression).IsAssignableFrom(pi.PropertyType))
						{
							var conditionExpression = ConditionParser.ParseExpression(sr, cfg);
							pi.SetValue(parameterTarget, conditionExpression, null);
						}
						else
						{
							string value = ParseParameterValue(sr);
							PropertyHelper.SetPropertyFromString(parameterTarget, parameterName, value, cfg);
						}
					}
				}
				else
				{
					// what we've just read is not a parameterName, but a value
					// assign it to a default property (denoted by empty string)
					PropertyInfo pi;

					if (PropertyHelper.TryGetPropertyInfo(lr, string.Empty, out pi))
					{
						if (typeof(SimpleLayout) == pi.PropertyType)
						{
							pi.SetValue(lr, new SimpleLayout(parameterName), null);
						}
						else
						{
							string value = parameterName;
							PropertyHelper.SetPropertyFromString(lr, pi.Name, value, cfg);
						}
					}
					else
					{
						InternalLogger.Warn("{0} has no default property", lr.GetType().FullName);
					}
				}

				ch = sr.Read();
			}

			lr = ApplyWrappers(cfg, lr, orderedWrappers);

			return lr;
		}

		private static LayoutRenderer ApplyWrappers(LoggingConfiguration cfg, LayoutRenderer lr, List<LayoutRenderer> orderedWrappers)
		{
			for (int i = orderedWrappers.Count - 1; i >= 0; --i)
			{
				var newRenderer = (WrapperLayoutRendererBase)orderedWrappers[i];
				InternalLogger.Trace("Wrapping {0} with {1}", lr.GetType().Name, newRenderer.GetType().Name);
				if (CanBeConvertedToLiteral(lr))
				{
					lr = ConvertToLiteral(cfg, lr);
				}

				newRenderer.Inner = new SimpleLayout(string.Empty, new[] { lr });
				lr = newRenderer;
			}

			return lr;
		}

		private static bool CanBeConvertedToLiteral(LayoutRenderer lr)
		{
			foreach(IRenderable renderable in ObjectGraph.AllChilds<IRenderable>(lr))
			{
				if (renderable.GetType() == typeof(SimpleLayout))
					continue;

				if (!renderable.GetType().IsDefined(typeof(AppDomainFixedOutputAttribute), false))
					return false;
			}

			return true;
		}

		private static void MergeLiterals(List<LayoutRenderer> list)
		{
			for (int i = 0; i + 1 < list.Count;)
			{
				var lr1 = list[i] as LiteralLayoutRenderer;
				var lr2 = list[i + 1] as LiteralLayoutRenderer;
				if (lr1 != null && lr2 != null)
				{
					lr1.Text += lr2.Text;
					list.RemoveAt(i + 1);
				}
				else
				{
					i++;
				}
			}
		}

		private static LayoutRenderer ConvertToLiteral(LoggingConfiguration cfg, LayoutRenderer renderer)
		{
			var closeReq = ObjectGraph.DeepInitialize(renderer, cfg, LogManager.ThrowExceptions);
			string text = renderer.Render(LogEventInfo.CreateNullEvent());
			foreach (var item in closeReq)
				item.Close();
			return new LiteralLayoutRenderer(text);
		}
	}
}
