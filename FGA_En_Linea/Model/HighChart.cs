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
                graph = new Highcharts(nombre) ;                
                graph.SetCredits(credits);
                graph.SetTitle(new Title()
                {
                    Text = string.Empty
                });
                graph.InitChart(new Chart()
                {
                    BorderRadius = 0,
                    BorderWidth = 0,
                    SpacingBottom = 0,
                    SpacingTop = 10,
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
                graph.SetNavigation(new Navigation { MenuItemStyle = "fontSize: '12px', color: 'black', fontWeight: '',  fontFamily: 'Arial, sans-serif'" });
                graph.SetLegend(new Legend
                {
                    Enabled = true,
                    ItemStyle = "fontSize: '12px', color: 'black', fontWeight: '',  fontFamily: 'Arial, sans-serif'",                   
                });
                graph.SetTooltip(new Tooltip
                {
                    Shared = true,
                    FollowTouchMove = true,
                    Enabled = true,
                    Style = "fontSize: '12px', color: 'black', fontWeight: '',  fontFamily: 'Arial, sans-serif'"
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

        public static XAxis GetXAxis(string[] values) {
            try
            {
                return new XAxis
                {
                    Categories = values,
                    Labels = new XAxisLabels()
                    {
                        Step = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_DateEachN] is null ? 1 : 3,
                        Style = "fontSize: '12px', color: 'black',  fontFamily: 'Arial, sans-serif'"                       
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
                string yLabel = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_YAxis] is null ? "fontSize: '0px'" : "fontSize: '12px', color: 'black',  fontFamily: 'Arial, sans-serif'";
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
                    GridLineWidth = 0,
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
                string yLabel = System.Web.HttpContext.Current.Session[FGA.Utility.Utilitarios.show_YAxis] is null ? "fontSize: '0px'" : "fontSize: '12px', color: 'black',  fontFamily: 'Arial, sans-serif'";
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
                    GridLineWidth = 0,
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
                    return ColorTranslator.FromHtml("#39aac5");
                case 1:
                    return ColorTranslator.FromHtml("#ed7c2f");
                case 2:
                    return ColorTranslator.FromHtml("#d9d9d9");
                case 3:
                    return ColorTranslator.FromHtml("#ddebf7");
                case 4:
                    return ColorTranslator.FromHtml("#2a4781"); //aqui
                case 5:
                    return ColorTranslator.FromHtml("#f1a03d");
                case 6:
                    return ColorTranslator.FromHtml("#4ba79e");
                case 7:
                    return ColorTranslator.FromHtml("#8c8c8c");
                case 8:
                    return ColorTranslator.FromHtml("#a3b0c1");
                case 9:
                    return ColorTranslator.FromHtml("#2e4b86");
                case 10:
                    return ColorTranslator.FromHtml("#8b5a2b");
                default:
                    return ColorTranslator.FromHtml("#11333b");
            }
        }
    }
}