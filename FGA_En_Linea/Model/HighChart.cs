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
        public static void ConfigChart(ref Highcharts graph, string nombre, ChartTypes? type, int? Heigh = 0)
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
                    SpacingTop = 12,
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
                    ItemStyle = "fontSize: '12px', color: '#334155', fontWeight: '600', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
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
                    Style = "fontSize: '12px', color: '#1e293b', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'"
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
                            Style = "fontSize: '12px', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
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
                            Style = "fontSize: '12px', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
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
                            Style = "fontSize: '12px', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
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
                            Style = "fontSize: '12px', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
                             
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
                            Style = "fontSize: '12px', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
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
                            Style = "fontSize: '12px', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
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
                            Style = "fontSize: '12x', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
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
                            Style = "fontSize: '12px', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
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
                            Style = "fontSize: '12px', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
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
                            Style = "fontSize: '12px', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
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
                            Style = "fontSize: '12px', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
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
                        Style = "fontSize: '12px', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
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
                        Style = "fontSize: '12px', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
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
                        Style = "fontSize: '12px', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
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
                        Style = "fontSize: '12px', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
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
                        Style = "fontSize: '12px', textShadow: 'none', textOutline: 'none',  fontFamily: 'Arial, sans-serif'",
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
                        Style = "fontSize: '11px', fontWeight: '600', color: '#334155', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
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
                        Style = "fontSize: '11px', fontWeight: '600', color: '#334155', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
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
                        Style = "fontSize: '11px', fontWeight: '600', color: '#334155', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
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
                        Style = "fontSize: '11px', fontWeight: '600', color: '#2F5597', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
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
                        Style = "fontSize: '11px', fontWeight: '600', color: '#334155', textShadow: 'none', textOutline: 'none', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'",
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

        public static XAxis GetXAxis(string[] values) {
            try
            {
                return new XAxis
                {
                    Categories = values,
                    GridLineWidth = 1,
                    GridLineColor = ColorTranslator.FromHtml("#f1f5f9"),
                    GridLineDashStyle = DashStyles.Dash,
                    LineColor = ColorTranslator.FromHtml("#cbd5e1"),
                    TickColor = ColorTranslator.FromHtml("#cbd5e1"),
                    Labels = new XAxisLabels()
                    {
                        Step = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_DateEachN] is null ? 1 : 3,
                        Style = "fontSize: '11px', color: '#64748b', fontWeight: '500', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'"                       
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
                string yLabel = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_YAxis] is null ? "fontSize: '0px'" : "fontSize: '11px', color: '#64748b', fontWeight: '500', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'";
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
                string yLabel = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_YAxis] is null ? "fontSize: '0px'" : "fontSize: '11px', color: '#64748b', fontWeight: '500', fontFamily: '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif'";
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
                    return ColorTranslator.FromHtml("#0284c7"); // Azul Zafiro Primario
                case 1:
                    return ColorTranslator.FromHtml("#ea580c"); // Ámbar / Naranja Cálido
                case 2:
                    return ColorTranslator.FromHtml("#2F5597"); // Azul Institucional FFC
                case 3:
                    return ColorTranslator.FromHtml("#10b981"); // Esmeralda / Éxito
                case 4:
                    return ColorTranslator.FromHtml("#6366f1"); // Índigo / Violeta
                case 5:
                    return ColorTranslator.FromHtml("#f59e0b"); // Ámbar Dorado
                case 6:
                    return ColorTranslator.FromHtml("#06b6d4"); // Cyan Moderno
                case 7:
                    return ColorTranslator.FromHtml("#ef4444"); // Rojo Coral / Alerta Mora
                case 8:
                    return ColorTranslator.FromHtml("#8b5cf6"); // Púrpura Elegante
                case 9:
                    return ColorTranslator.FromHtml("#64748b"); // Slate Neutro
                case 10:
                    return ColorTranslator.FromHtml("#e11d48"); // Rojo Alerta Crítica / Mora >180 días
                case 11:
                    return ColorTranslator.FromHtml("#991b1b"); // Borgoña / Cobro Judicial
                case 12:
                    return ColorTranslator.FromHtml("#0f766e"); // Teal Profundo
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