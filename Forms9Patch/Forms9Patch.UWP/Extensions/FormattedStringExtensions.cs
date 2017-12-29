﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Controls;


namespace Forms9Patch.UWP
{
    static class FormattedStringExtensions
    {
        static bool _textDecorationsPresent;
        static bool _textDecorationsPresentSet;
        public static bool TextDecorationsPresent
        {
            get
            {
                if (!_textDecorationsPresentSet)
                {
                    _textDecorationsPresent = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Text.TextDecorations");
                    _textDecorationsPresentSet = true;
                }
                return _textDecorationsPresent;
            }
        }

        internal static void SetAndFormatText(this TextBlock winTextBlock, Forms9Patch.Label f9pLabel, double altFontSize=-1)
        {
            var label = f9pLabel;
            var textBlock = winTextBlock;

            if (f9pLabel == null || textBlock==null)
                return;

            textBlock.Text = "";
            textBlock.Inlines.Clear();
            textBlock.FontSize = (altFontSize > 0 ? altFontSize : label.DecipheredFontSize());
            textBlock.LineHeight = FontExtensions.LineHeightForFontSize(textBlock.FontSize);
            textBlock.LineStackingStrategy = Windows.UI.Xaml.LineStackingStrategy.BaselineToBaseline;
            textBlock.FontFamily = FontService.GetWinFontFamily(label.FontFamily);

            if (label.Text != null)
            {
                textBlock.Text = label.Text;
                return;
            }
            if (label.FormattedText!=null)
            {
                var formattedText = label.FormattedText;
                for (var i = 0; i < formattedText.Spans.Count; i++)
                    textBlock.Inlines.Add(formattedText.Spans[i].ToRun());
                return;
            }

            var text = label.F9PFormattedString?.Text;
            if (label.F9PFormattedString is HTMLMarkupString htmlMarkupString)
                text = htmlMarkupString.UnmarkedText;

            if (string.IsNullOrWhiteSpace(text) || text == label.F9PFormattedString?.Text)
            {
                // there isn't any markup!
                textBlock.Text = text;
                return;
            }

            #region Layout font-spans (MetaFonts)
            var metaFonts = new List<MetaFont>();
            var baseMetaFont = new MetaFont(
                label.FontFamily,
                textBlock.FontSize, //(altFontSize > 0 ? altFontSize : label.DecipheredFontSize()), //(label.FontSize < 0 ? (double)Windows.UI.Xaml.Application.Current.Resources["ControlContentThemeFontSize"] : label.FontSize)), //label.FontSize,
                (label.FontAttributes & Xamarin.Forms.FontAttributes.Bold) > 0,//textBlock.FontWeight.Weight >= Windows.UI.Text.FontWeights.Bold.Weight,
                (label.FontAttributes & Xamarin.Forms.FontAttributes.Italic) > 0,// (textBlock.FontStyle & Windows.UI.Text.FontStyle.Italic) > 0,
                textColor: label.TextColor//,
                //backgroundColor: label.BackgroundColor
                );

            var MathMetaFont = new MetaFont(baseMetaFont)
            {
                Family = "STIXGeneral" //FontService.ReconcileFontFamily("Forms9Patch.Resources.Fonts.STIXGeneral.otf", PCL.Utils.ReflectionExtensions.GetAssembly(typeof(Forms9Patch.Label)))
            };

            for (int i = 0; i < text.Length; i++)
            {
                if (i + 1 < text.Length && text[i] == '\ud835' && text[i + 1] >= '\udc00' && text[i + 1] <= '\udeff')
                {
                    metaFonts.Add(new MetaFont(MathMetaFont));
                    metaFonts.Add(new MetaFont(MathMetaFont));  // there are two because we're using a double byte unicode character
                    i++;
                }
                else
                    metaFonts.Add(new MetaFont(baseMetaFont));
            }
            #endregion


            #region Apply non-font Spans
            foreach (var span in label.F9PFormattedString._spans)
            {
                int spanStart = span.Start;
                int spanEnd = span.End;

                //spanEnd++;
                if (spanEnd >= text.Length)
                    spanEnd = text.Length - 1;

                for (int i = spanStart; i <= spanEnd; i++)
                {
                    switch (span.Key)
                    {
                        case FontFamilySpan.SpanKey: // TextElement.FontFamily
                            var fontFamily = ((FontFamilySpan)span).FontFamilyName;
                            metaFonts[i].Family = fontFamily;
                            break;
                        case FontSizeSpan.SpanKey:  // TextElement.FontSize
                            float size = ((FontSizeSpan)span).Size;
                            metaFonts[i].Size = (size < 0 ? metaFonts[i].Size * (-size) : size);
                            break;
                        case BoldSpan.SpanKey: // Bold span // TextElement.FontWeight (Thin, ExtraLight, Light, SemiLight, Normal, Medium, SemiBold, Bold, ExtraBold, Black, ExtraBlack)
                            metaFonts[i].Bold = true;
                            break;
                        case ItalicsSpan.SpanKey: // Italic span // TextElement.FontStyle (Normal, Italic, Oblique)
                            metaFonts[i].Italic = true;
                            break;
                        case FontColorSpan.SpanKey: // TextElement.Foreground
                            metaFonts[i].TextColor = ((FontColorSpan)span).Color;
                            break;
                        case UnderlineSpan.SpanKey: // Underline span  // TextElement.TextDecorations = None, Strikethought, Underline
                            metaFonts[i].Underline = true;
                        break;
                        case StrikethroughSpan.SpanKey: // TextElement.TextDecorations = None, Strikethought, Underline
                            metaFonts[i].Strikethrough = true;
                            break;
                        case SuperscriptSpan.SpanKey: // Run with Typographic.Variants=FontVariants.Superscript while using Cambria
                            metaFonts[i].Baseline = FontBaseline.Superscript;
                            break;
                        case SubscriptSpan.SpanKey: // Run with Typographic.Varients=FontVariants.Subscript while using Cambria
                            metaFonts[i].Baseline = FontBaseline.Subscript;
                            break;
                        case NumeratorSpan.SpanKey: // no UWP solution - need to use SuperScript
                            metaFonts[i].Baseline = FontBaseline.Numerator;
                            break;
                        case DenominatorSpan.SpanKey: // no UWP solution - need to use Subscript
                            metaFonts[i].Baseline = FontBaseline.Denominator;
                            break;
                        case ActionSpan.SpanKey:  // Hyperlink span ??
                            metaFonts[i].Action = new MetaFontAction((ActionSpan)span);
                            break;
                        case BackgroundColorSpan.SpanKey: // if Win10 fall creator's update, there is a solution: create TextHighlighter, set its BackgroundColor and add the Range (Start/End) to it's Ranges, and add to TextBlock.Highlighters
                            metaFonts[i].BackgroundColor = ((BackgroundColorSpan)span).Color;
                            break;
                    }
                }
            }
            #endregion


            #region Convert MetaFonts to InLines
            var inlineColection = new List<Inline>();

            // run through MetaFonts to see if we need to set new Font attributes
            var lastMetaFont = baseMetaFont;
            int startIndex = 0;
            for (int i = 0; i < metaFonts.Count; i++)
            {
                var metaFont = metaFonts[i];
                if (lastMetaFont != metaFont)
                {
                    // we are at the start of a new span
                    if (i > 0) // && lastMetaFont != baseMetaFont)
                        AddInline(textBlock, label, lastMetaFont, text, startIndex, i - startIndex);
                    lastMetaFont = metaFont;
                    startIndex = i;
                }
            }
            AddInline(textBlock, label, lastMetaFont, text, startIndex, text.Length - startIndex);
            #endregion

        }


        static void AddInline(TextBlock textBlock, Forms9Patch.Label label, MetaFont metaFont, string text, int startIndex, int length)
        {
            var run = new Run();

            run.Text = text.Substring(startIndex, length);
            run.FontSize = metaFont.Size;
            run.FontWeight = metaFont.Bold ? Windows.UI.Text.FontWeights.Bold : Windows.UI.Text.FontWeights.Normal;
            run.FontStyle = metaFont.Italic ? Windows.UI.Text.FontStyle.Italic : Windows.UI.Text.FontStyle.Normal;

            
            if (TextDecorationsPresent)
            {
                run.TextDecorations = Windows.UI.Text.TextDecorations.None;
                if (metaFont.Strikethrough)
                    run.TextDecorations |= Windows.UI.Text.TextDecorations.Strikethrough;
            }

            switch (metaFont.Baseline)
            {
                case FontBaseline.Numerator:
                    run.FontFamily = new Windows.UI.Xaml.Media.FontFamily("Cambria");
                    Typography.SetVariants(run, Windows.UI.Xaml.FontVariants.Superscript);
                    break;
                case FontBaseline.Superscript:
                    run.FontFamily = new Windows.UI.Xaml.Media.FontFamily("Cambria");
                    Typography.SetVariants(run, Windows.UI.Xaml.FontVariants.Superscript);
                    break;
                case FontBaseline.Denominator:
                    run.FontFamily = new Windows.UI.Xaml.Media.FontFamily("Cambria");
                    Typography.SetVariants(run, Windows.UI.Xaml.FontVariants.Subscript);
                    break;
                case FontBaseline.Subscript:
                    run.FontFamily = new Windows.UI.Xaml.Media.FontFamily("Cambria");
                    Typography.SetVariants(run, Windows.UI.Xaml.FontVariants.Subscript);
                    break;
                default:
                    if (metaFont.Family!=null)
                        run.FontFamily = new Windows.UI.Xaml.Media.FontFamily(FontService.ReconcileFontFamily(metaFont.Family));
                    break;
            }

            if (!metaFont.BackgroundColor.IsDefaultOrTransparent())
            {
                try
                {
                    textBlock.ApplyBackgroundColor(metaFont.BackgroundColor, startIndex, length);
                }
                catch (Exception e)
                {
                    throw new Exception("It appears that this Xamarin.Forms.UWP app was built with a Windows TargetVersion < 10.0.16299.0 (Windows 10 Fall Creators Update).  10.0.16299.0 is needed to support Forms9Patch.Label.HtmlText background color attributes.", e);
                }
            }

            if (metaFont.IsActionEmpty())
            {
                run.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(metaFont.TextColor.ToWindowsColor());
                if (metaFont.Underline && TextDecorationsPresent)
                    run.TextDecorations |= Windows.UI.Text.TextDecorations.Underline;
                textBlock.Inlines.Add(run);
            }
            else
            {
                run.Foreground = new Windows.UI.Xaml.Media.SolidColorBrush(Xamarin.Forms.Color.Blue.ToWindowsColor());
                if (TextDecorationsPresent)
                    run.TextDecorations |= Windows.UI.Text.TextDecorations.Underline;
                var hyperlink = new Hyperlink();
                hyperlink.Inlines.Add(run);
                hyperlink.Click += (Hyperlink sender, HyperlinkClickEventArgs args) => label.Tap(metaFont.Action.Id, metaFont.Action.Href);
                textBlock.Inlines.Add(hyperlink);
            }
        }

    }
}
