﻿/*
 * This code is provided under the Code Project Open Licence (CPOL)
 * See http://www.codeproject.com/info/cpol10.aspx for details
 */

using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace System.Drawing
{
    internal sealed class ThemedColors
    {
        public enum ColorScheme
        {
            NormalColor = 0,
            HomeStead   = 1,
            Metallic    = 2,
            NoTheme     = 3
        }

        private static ColorScheme GetCurrentThemeIndex()
        {
            ColorScheme theme = ColorScheme.NoTheme;

            if (VisualStyleInformation.IsSupportedByOS && VisualStyleInformation.IsEnabledByUser && Application.RenderWithVisualStyles)
                switch (VisualStyleInformation.ColorScheme)
                {
                    case NormalColor:
                        theme = ColorScheme.NormalColor;
                        break;
                    case HomeStead:
                        theme = ColorScheme.HomeStead;
                        break;
                    case Metallic:
                        theme = ColorScheme.Metallic;
                        break;
                    default:
                        theme = ColorScheme.NoTheme;
                        break;
                }

            return theme;
        }

        #region "    Variables and Constants "

        private const string NormalColor = "NormalColor";
        private const string HomeStead   = "HomeStead";
        private const string Metallic    = "Metallic";
        private const string NoTheme     = "NoTheme";

        private static readonly Color[] _toolBorder;

        #endregion

        #region "    Properties "

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static ColorScheme CurrentThemeIndex => GetCurrentThemeIndex();

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static Color ToolBorder => _toolBorder[(int)CurrentThemeIndex];

        #endregion

        #region "    Constructors "

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ThemedColors()
        {
            _toolBorder = new[] { Color.FromArgb(127, 157, 185), Color.FromArgb(164, 185, 127), Color.FromArgb(165, 172, 178), Color.FromArgb(132, 130, 132) };
        }

        private ThemedColors()
        {
        }

        #endregion
    }
}