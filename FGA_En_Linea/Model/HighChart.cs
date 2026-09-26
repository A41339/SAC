using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web;
using System.Web.ModelBinding;

namespace FGA.Model
{
    public static class HighChart
    {
        public static void ConfigChart(ref Highcharts graph, string nombre, ChartTypes? type, int? Heigh = 0, int? SpacingTop = 60)
        {
            try
            {
                Credits credits = new Credits
                {
                    Enabled = false
                };
                graph = new Highcharts(nombre);                
                graph.SetCredits(credits);
                graph.SetTitle(new Title()
                {
                    Text = string.Empty
                });
                graph.InitChart(new Chart()
                {
                    BorderRadius = 0,
                    BorderWidth = 0,
                    SpacingBottom = 8,
                    SpacingTop = SpacingTop ?? 60,
                    SpacingLeft = 10,
                    SpacingRight = 10,
                    Height = (Heigh == 0 ? null : Heigh)
                });
                graph.SetExporting(new Exporting
                {
                    Scale = 1,
                    SourceHeight = 500,
                    SourceWidth = 1000,
                    Filename = "FFC",
                    Buttons = new ExportingButtons
                    {
                        ContextButton = new ExportingButtonsContextButton
                        {
                            VerticalAlign = VerticalAligns.Top,
                            Symbol = "menuball"
                        }
                    },                    
                });
                graph.SetNavigation(new Navigation { MenuItemStyle = "fontSize: '12px', color: '#334155', fontWeight: '500', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'" });
                graph.SetLegend(new Legend
                {
                    Enabled = true,
                    ItemStyle = "fontSize: '12.5px', color: '#1e293b', fontWeight: '600', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                    ItemHoverStyle = "color: '#2F5597'",
                    ItemMarginTop = 6,
                    ItemMarginBottom = 4,
                    ItemDistance = 16,
                    SymbolRadius = 4
                });
                graph.SetTooltip(new Tooltip
                {
                    Shared = true,
                    FollowTouchMove = true,
                    Enabled = true,
                    UseHTML = true,
                    Formatter = "function() { return typeof window.formatGlobalChartTooltip === 'function' ? window.formatGlobalChartTooltip(this) : (this.x + ': ' + this.y); }",
                    BackgroundColor = new BackColorOrGradient(Color.Transparent),
                    BorderWidth = 0,
                    Shadow = false,
                    Style = "fontSize: '12.5px', color: '#1e293b', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'"
                });
               
            }
            catch (Exception)
            {
            }
        }


        public static PlotOptions getLabelStackingNormal()
        {
            try
            {
                return new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        Stacking = Stackings.Normal,
                        DataLabels = new PlotOptionsColumnDataLabels
                        {
                            Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_ColumnDetail] is null ? false : true,
                            Format = "{point.y:,.2f}%",
                            Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                        }
                    }
                };
            }
            catch (Exception) {return null; }
        }

        public static PlotOptions getLabelStackingAmmount()
        {
            try
            {
                return new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        Stacking = Stackings.Normal,
                        DataLabels = new PlotOptionsColumnDataLabels
                        {
                            Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_ColumnDetail] is null ? false : true,
                            Format = "{point.y:,.2f}",
                            Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                        }
                    }
                };
            }
            catch (Exception) { return null; }
        }

        public static PlotOptions getLabelStackingQuantity()
        {
            try
            {
                return new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        Stacking = Stackings.Normal,
                        DataLabels = new PlotOptionsColumnDataLabels
                        {
                            Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_ColumnDetail] is null ? false : true,
                            Format = "{point.y:,.0f}",
                            Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                        }
                    }
                };
            }
            catch (Exception) { return null; }
        }


        public static PlotOptions getLabelStackingPercent()
        {
            try
            {
                return new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        Stacking = Stackings.Percent,
                        DataLabels = new PlotOptionsColumnDataLabels
                        {
                            Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_ColumnDetail] is null ? false : true,
                            Format = "{point.y:,.2f}%",
                            Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                             
                        }                         
                    }
                };
            }
            catch (Exception) {
                return null;
            }
        }

        public static PlotOptions getLabelStackingPercentBar()
        {
            try
            {
                return new PlotOptions
                {
                    Bar = new PlotOptionsBar
                    {                         
                        Stacking = Stackings.Percent,
                        DataLabels = new PlotOptionsBarDataLabels
                        {
                            Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_ColumnDetail] is null ? false : true,
                            Format = "{point.y:,.2f}%",
                            Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                        },
                        MinPointLength = 15 
                    }
                };
            }
            catch (Exception) {
                return null;
            }
        }

        public static PlotOptions getLabelStackingPercent(decimal pointWidth)
        {
            try
            {
                return new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        Stacking = Stackings.Percent,
                        DataLabels = new PlotOptionsColumnDataLabels
                        {
                            Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_ColumnDetail] is null ? false : true,
                            Format = "{point.y:,.2f}%",
                            Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                            Overflow = "allow"
                        },
                        PointWidth = (Number)pointWidth
                    }
                };
            }
            catch (Exception) {
                return null;
            }
        }

        public static PlotOptions getLabelPercent()
        {
            try
            {
                return new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        DataLabels = new PlotOptionsColumnDataLabels
                        {
                            Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_ColumnDetail] is null ? false : true,
                            Format = "{point.y:,.2f}%",
                            Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                            Inside = false,
                            Overflow = "allow"
                        },
                        MinPointLength = 15
                    }
                };
            }
            catch (Exception) { return null; }
        }


        public static PlotOptions getLabelAmmount(decimal pointWidth)
        {
            try
            {
                return new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        DataLabels = new PlotOptionsColumnDataLabels
                        {
                            Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_ColumnDetail] is null ? false : true,
                            Format = "{point.y:,.0f}",
                            Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                            Inside = false,
                            Y = -10
                        },
                        PointWidth = (Number)pointWidth,
                        MinPointLength = 15
                    }
                };
            }
            catch (Exception) { return null; }
        }


        public static PlotOptions getLabelAmmount()
        {
            try
            {
                return new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        DataLabels = new PlotOptionsColumnDataLabels
                        {
                            Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_ColumnDetail] is null ? false : true,
                            Format = "{point.y:,.0f}",
                            Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                            Inside = false,
                            Y = -10,

                        },
                        MinPointLength = 15
                    }
                };
            }
            catch (Exception) { return null; }
        }

        public static PlotOptions getBarLabelAmmount()
        {
            try
            {
                return new PlotOptions
                {
                    Bar = new PlotOptionsBar
                    {
                        DataLabels = new PlotOptionsBarDataLabels
                        {
                            Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_ColumnDetail] is null ? false : true,
                            Format = "{point.y:,.0f}",
                            Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                            Inside = false                            
                        },
                        MinPointLength = 15
                    }
                };
            }
            catch (Exception) { return null; }
        }

        public static PlotOptions getLabelDecimals()
        {
            try
            {
                return new PlotOptions
                {
                    Column = new PlotOptionsColumn
                    {
                        DataLabels = new PlotOptionsColumnDataLabels
                        {
                            Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_ColumnDetail] is null ? false : true,
                            Format = "{point.y:,.2f}",
                            Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                            Inside = false,
                            Y = -10
                        },
                        MinPointLength = 15
                    }
                };
            }
            catch (Exception) { return null; }
        }

        public static PlotOptionsLine getLineDash()
        {
            try
            {
                PlotOptionsLine LineDash = new PlotOptionsLine
                {
                    DashStyle = DashStyles.LongDash,
                    LineWidth = 2,
                    Marker = new PlotOptionsLineMarker
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true
                    },
                    DataLabels = new PlotOptionsLineDataLabels
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true,
                        Format = "{point.y:,.2f}",
                        Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                        Inside = false,
                        Y = -10
                    }
                };

                return LineDash;
            }
            catch (Exception) { return null;  }
        }

        public static PlotOptionsLine getLineDashPercent()
        {
            try
            {
                PlotOptionsLine LineDash = new PlotOptionsLine
                {
                    DashStyle = DashStyles.LongDash,
                    LineWidth = 2,
                    Marker = new PlotOptionsLineMarker
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true
                    },
                    DataLabels = new PlotOptionsLineDataLabels
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true,
                        Format = "{point.y:,.2f}%",
                        Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                        Inside = false,
                        Overflow = "allow",
                        Y = -10
                    }
                };

                return LineDash;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static PlotOptionsLine getLine()
        {
            try
            {
                PlotOptionsLine Line = new PlotOptionsLine
                {
                    DashStyle = DashStyles.Solid,
                    LineWidth = 2,
                    Marker = new PlotOptionsLineMarker
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true
                    },
                    DataLabels = new PlotOptionsLineDataLabels
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true,
                        Format = "{point.y:,.2f}",
                        Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                        Inside = false,
                        Overflow = "allow",
                        Y = -10
                    }
                };

                return Line;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static PlotOptionsLine getLinePercent()
        {
            try
            {
                PlotOptionsLine Line = new PlotOptionsLine
                {
                    DashStyle = DashStyles.Solid,
                    LineWidth = 2,
                    Marker = new PlotOptionsLineMarker
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true
                    },
                    DataLabels = new PlotOptionsLineDataLabels
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true,
                        Format = "{point.y:,.2f}%",
                        Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                        Inside = false,
                        Overflow = "allow",
                        Y = -10
                    }
                };

                return Line;
            }
            catch (Exception) { return null; }
        }

        public static PlotOptionsLine getLineWidth(int width)
        {
            try
            {
                PlotOptionsLine LineWidth = new PlotOptionsLine
                {
                    DashStyle = DashStyles.LongDash,
                    LineWidth = width,
                    Marker = new PlotOptionsLineMarker
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true
                    },
                    DataLabels = new PlotOptionsLineDataLabels
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true,
                        Format = "{point.y:,.2f}",
                        Style = "fontSize: '12px', fontWeight: '700', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                        Inside = false,
                        Overflow = "allow",
                        Y = -10
                    }                    
                };

                return LineWidth;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static PlotOptionsSpline getSpline(int width = 3)
        {
            try
            {
                return new PlotOptionsSpline
                {
                    DashStyle = DashStyles.Solid,
                    LineWidth = (Number)width,
                    Marker = new PlotOptionsSplineMarker
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true,
                        Radius = (Number)4
                    },
                    DataLabels = new PlotOptionsSplineDataLabels
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true,
                        Format = "{point.y:,.2f}",
                        Style = "fontSize: '12px', fontWeight: '700', color: '#1e293b', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                        Inside = false,
                        Overflow = "allow",
                        Y = -10
                    }
                };
            }
            catch (Exception) { return null; }
        }

        public static PlotOptionsSpline getSplinePercent(int width = 3)
        {
            try
            {
                return new PlotOptionsSpline
                {
                    DashStyle = DashStyles.Solid,
                    LineWidth = (Number)width,
                    Marker = new PlotOptionsSplineMarker
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true,
                        Radius = (Number)4
                    },
                    DataLabels = new PlotOptionsSplineDataLabels
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true,
                        Format = "{point.y:,.2f}%",
                        Style = "fontSize: '12px', fontWeight: '700', color: '#1e293b', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                        Inside = false,
                        Overflow = "allow",
                        Y = -10
                    }
                };
            }
            catch (Exception) { return null; }
        }

        public static PlotOptionsSpline getSplineDashPercent(int width = 2)
        {
            try
            {
                return new PlotOptionsSpline
                {
                    DashStyle = DashStyles.ShortDash,
                    LineWidth = (Number)width,
                    Marker = new PlotOptionsSplineMarker
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true,
                        Radius = (Number)3.5
                    },
                    DataLabels = new PlotOptionsSplineDataLabels
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true,
                        Format = "{point.y:,.2f}%",
                        Style = "fontSize: '12px', fontWeight: '700', color: '#1e293b', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                        Inside = false,
                        Overflow = "allow",
                        Y = -10
                    }
                };
            }
            catch (Exception) { return null; }
        }

        public static PlotOptionsAreaspline getAreasplinePercent(double fillOpacity = 0.18, int lineWidth = 2)
        {
            try
            {
                return new PlotOptionsAreaspline
                {
                    FillOpacity = (Number)fillOpacity,
                    LineWidth = (Number)lineWidth,
                    Marker = new PlotOptionsAreasplineMarker
                    {
                        Enabled = false
                    },
                    DataLabels = new PlotOptionsAreasplineDataLabels
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_LineDetail] is null ? false : true,
                        Format = "{point.y:,.2f}%",
                        Style = "fontSize: '12px', fontWeight: '700', color: '#1e293b', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                        Inside = false,
                        Overflow = "allow",
                        Y = -10
                    }
                };
            }
            catch (Exception) { return null; }
        }

        public static PlotOptionsColumn getColumnStyled(double borderRadius = 4, bool percent = true)
        {
            try
            {
                return new PlotOptionsColumn
                {
                    BorderRadius = (Number)borderRadius,
                    BorderWidth = (Number)0,
                    DataLabels = new PlotOptionsColumnDataLabels
                    {
                        Enabled = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_ColumnDetail] is null ? false : true,
                        Format = percent ? "{point.y:,.2f}%" : "{point.y:,.0f}",
                        Style = "fontSize: '12px', fontWeight: '700', color: '#1e293b', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
                        Inside = false
                    },
                    MinPointLength = 10
                };
            }
            catch (Exception) { return null; }
        }

        public static PlotOptions getPlotOptionsColumn(double borderRadius = 4, bool percent = true)
        {
            try
            {
                return new PlotOptions
                {
                    Column = getColumnStyled(borderRadius, percent)
                };
            }
            catch (Exception) { return null; }
        }

        public static string[] FormatearCategoriasAMeses(string[] values)
        {
            if (values == null || values.Length == 0) return values;
            string[] result = new string[values.Length];
            var culture = new System.Globalization.CultureInfo("es-ES");

            for (int i = 0; i < values.Length; i++)
            {
                result[i] = FormatearPeriodoAMes(values[i], culture);
            }
            return result;
        }

        public static string FormatearPeriodoAMes(string val, System.Globalization.CultureInfo culture = null)
        {
            if (string.IsNullOrWhiteSpace(val)) return val;
            culture = culture ?? new System.Globalization.CultureInfo("es-ES");
            string trimmed = val.Trim();

            // 1. Formato M-yyyy o MM-yyyy o M.yyyy o M/yyyy (ej: "1-2025", "8.2025", "08/2026")
            var matchM = System.Text.RegularExpressions.Regex.Match(trimmed, @"^(\d{1,2})[-/. ](\d{4})$");
            if (matchM.Success)
            {
                if (int.TryParse(matchM.Groups[1].Value, out int m) && int.TryParse(matchM.Groups[2].Value, out int y))
                {
                    if (m >= 1 && m <= 12 && y >= 1990 && y <= 2100)
                    {
                        return new DateTime(y, m, 1).ToString("MMM-yy", culture);
                    }
                }
            }

            // 2. Formato yyyy-M o yyyy-MM (ej: "2025-01", "2025-1")
            var matchY = System.Text.RegularExpressions.Regex.Match(trimmed, @"^(\d{4})[-/. ](\d{1,2})$");
            if (matchY.Success)
            {
                if (int.TryParse(matchY.Groups[1].Value, out int y) && int.TryParse(matchY.Groups[2].Value, out int m))
                {
                    if (m >= 1 && m <= 12 && y >= 1990 && y <= 2100)
                    {
                        return new DateTime(y, m, 1).ToString("MMM-yy", culture);
                    }
                }
            }

            // 3. Formato fecha completa yyyy-MM-dd (ej: "2025-01-31" o "2025-01-31T00:00:00")
            var matchISO = System.Text.RegularExpressions.Regex.Match(trimmed, @"^(\d{4})[-/. ](\d{1,2})[-/. ](\d{1,2})");
            if (matchISO.Success)
            {
                if (int.TryParse(matchISO.Groups[1].Value, out int y) && int.TryParse(matchISO.Groups[2].Value, out int m))
                {
                    if (m >= 1 && m <= 12 && y >= 1990 && y <= 2100)
                    {
                        return new DateTime(y, m, 1).ToString("MMM-yy", culture);
                    }
                }
            }

            // 4. Formato fecha completa dd/MM/yyyy o dd-MM-yyyy (ej: "31/01/2025")
            var matchDMY = System.Text.RegularExpressions.Regex.Match(trimmed, @"^(\d{1,2})[-/. ](\d{1,2})[-/. ](\d{4})$");
            if (matchDMY.Success)
            {
                if (int.TryParse(matchDMY.Groups[2].Value, out int m) && int.TryParse(matchDMY.Groups[3].Value, out int y))
                {
                    if (m >= 1 && m <= 12 && y >= 1990 && y <= 2100)
                    {
                        return new DateTime(y, m, 1).ToString("MMM-yy", culture);
                    }
                }
            }

            // 5. Nombres de meses en texto (ej: "Enero 2025", "Diciembre 24", "ene 25", "ene.-25")
            var matchTxt = System.Text.RegularExpressions.Regex.Match(trimmed, @"^([a-zA-ZáéíóúÁÉÍÓÚ.]+)[-/\s]+(\d{2,4})$");
            if (matchTxt.Success)
            {
                string txt = matchTxt.Groups[1].Value.ToLower().Replace(".", "");
                if (int.TryParse(matchTxt.Groups[2].Value, out int y))
                {
                    if (y < 100) y += 2000;
                    if (y >= 1990 && y <= 2100)
                    {
                        int mesNum = GetMesNumero(txt);
                        if (mesNum >= 1 && mesNum <= 12)
                        {
                            return new DateTime(y, mesNum, 1).ToString("MMM-yy", culture);
                        }
                    }
                }
            }

            return val;
        }

        private static int GetMesNumero(string nombreMes)
        {
            switch (nombreMes.ToLower().Trim())
            {
                case "ene":
                case "enero":
                    return 1;
                case "feb":
                case "febrero":
                    return 2;
                case "mar":
                case "marzo":
                    return 3;
                case "abr":
                case "abril":
                    return 4;
                case "may":
                case "mayo":
                    return 5;
                case "jun":
                case "junio":
                    return 6;
                case "jul":
                case "julio":
                    return 7;
                case "ago":
                case "agosto":
                    return 8;
                case "set":
                case "sep":
                case "setiembre":
                case "septiembre":
                    return 9;
                case "oct":
                case "octubre":
                    return 10;
                case "nov":
                case "noviembre":
                    return 11;
                case "dic":
                case "diciembre":
                    return 12;
                default:
                    return 0;
            }
        }

        public static XAxis GetXAxis(string[] values) {
            try
            {
                return new XAxis
                {
                    Categories = FormatearCategoriasAMeses(values),
                    GridLineWidth = 1,
                    GridLineColor = ColorTranslator.FromHtml("#f1f5f9"),
                    GridLineDashStyle = DashStyles.Dash,
                    LineColor = ColorTranslator.FromHtml("#cbd5e1"),
                    TickColor = ColorTranslator.FromHtml("#cbd5e1"),
                    Labels = new XAxisLabels()
                    {
                        Step = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_DateEachN] is null ? 1 : 3,
                        Style = "fontSize: '12.5px', color: '#1e293b', fontWeight: '600', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'"                       
                    },
                    ShowEmpty = true,                     
                    Title = new XAxisTitle()
                    {
                        Text = " "
                    }
                };
            }
            catch (Exception) { return null; }
        }

        public static YAxis GetYAxis(decimal? pMin, decimal? pMax, string format, AxisTypes? type = AxisTypes.Linear, bool reversed = false)
        {
            try
            {
                string yLabel = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_YAxis] is null ? "fontSize: '0px'" : "fontSize: '12px', color: '#334155', fontWeight: '600', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'";
                return new YAxis()
                {
                    Title = new YAxisTitle()
                    {
                        Text = "",
                    },
                    Labels = new YAxisLabels()
                    {
                        Formatter = format,
                        Style = yLabel
                    },
                    ShowFirstLabel = true,
                    ShowLastLabel = true,
                    ShowEmpty = true,
                    AllowDecimals = true, 
                    MaxPadding = (Number)0.22,
                    //Min = (Number?)pMin,
                    //Max = (Number?)pMax,
                    GridLineWidth = 1,
                    GridLineColor = ColorTranslator.FromHtml("#f1f5f9"),
                    GridLineDashStyle = DashStyles.Dash,
                    LineColor = ColorTranslator.FromHtml("#cbd5e1"),
                    Type = (type is null ? AxisTypes.Logarithmic : type),
                    Reversed =  reversed                   
                };
            }
            catch (Exception) { return null;  }
        }

        public static YAxis GetYAxisMax(decimal? pMin, decimal? pMax, string format, AxisTypes? type = AxisTypes.Linear, bool reversed = false)
        {
            try
            {
                string yLabel = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_YAxis] is null ? "fontSize: '0px'" : "fontSize: '12px', color: '#334155', fontWeight: '600', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'";
                return new YAxis()
                {
                    Title = new YAxisTitle()
                    {
                        Text = "",
                    },
                    Labels = new YAxisLabels()
                    {
                        Formatter = format,
                        Style = yLabel
                    },
                    ShowFirstLabel = true,
                    ShowLastLabel = true,
                    ShowEmpty = true,
                    AllowDecimals = true,
                    Min = (Number?)pMin,
                    Max = (Number?)pMax,
                    GridLineWidth = 1,
                    GridLineColor = ColorTranslator.FromHtml("#f1f5f9"),
                    GridLineDashStyle = DashStyles.Dash,
                    LineColor = ColorTranslator.FromHtml("#cbd5e1"),
                    Type = (type is null ? AxisTypes.Logarithmic : type),
                    Reversed = reversed
                };
            }
            catch (Exception) { return null; }
        }


        public static System.Drawing.Color GetColor(int i)
        {
            switch (i)
            {
                case 0:
                    return ColorTranslator.FromHtml("#2F5597"); // Azul Institucional (ancla)
                case 1:
                    return ColorTranslator.FromHtml("#6B9FD4"); // Azul Cielo Pastel
                case 2:
                    return ColorTranslator.FromHtml("#94A3B8"); // Gris Slate Profesional
                case 3:
                    return ColorTranslator.FromHtml("#F4A261"); // Naranja Durazno Suave
                case 4:
                    return ColorTranslator.FromHtml("#52B788"); // Verde Salvia Suave
                case 5:
                    return ColorTranslator.FromHtml("#64748B"); // Gris Acero Oscuro
                case 6:
                    return ColorTranslator.FromHtml("#A78BFA"); // Lavanda / Violeta Pastel
                case 7:
                    return ColorTranslator.FromHtml("#FCD34D"); // Ámbar Dorado Suave
                case 8:
                    return ColorTranslator.FromHtml("#5DADE2"); // Azul Claro / Cielo
                case 9:
                    return ColorTranslator.FromHtml("#F08080"); // Coral Rosado Suave
                case 10:
                    return ColorTranslator.FromHtml("#81B3A8"); // Teal Grisáceo Suave
                case 11:
                    return ColorTranslator.FromHtml("#E11D48"); // Rojo Alerta (mora >180 días)
                case 12:
                    return ColorTranslator.FromHtml("#7B5EA7"); // Púrpura Institucional Suave
                default:
                    return ColorTranslator.FromHtml("#143750"); // Azul Marino Institucional
            }
        }

        public static System.Drawing.Color GetMoraColor(int tramo)
        {
            switch (tramo)
            {
                case 0: return GetColor(3);  // 1 - 30 días: Verde esmeralda (#10b981)
                case 1: return GetColor(6);  // 31 - 60 días: Cyan (#06b6d4)
                case 2: return GetColor(5);  // 61 - 90 días: Ámbar (#f59e0b)
                case 3: return GetColor(1);  // 91 - 180 días: Naranja cálido (#ea580c)
                case 4: return GetColor(10); // Más de 180 días: Rojo alerta (#e11d48)
                case 5: return GetColor(11); // Cobro Judicial: Borgoña / Rojo oscuro (#991b1b)
                default: return GetColor(7); // Alerta genérica (#ef4444)
            }
        }
    }
}