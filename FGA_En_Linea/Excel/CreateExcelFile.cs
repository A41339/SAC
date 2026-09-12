using System;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace FGA
{
    public class CreateExcelFile
    {

        public static bool CreateExcelDocument(DataSet ds, string excelFilename, bool includeAutoFilter)
        {
            try
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(excelFilename, SpreadsheetDocumentType.Workbook))
                {
                    WriteExcelFile(ds, document, includeAutoFilter);
                }
                Trace.WriteLine("Successfully created: " + excelFilename);
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed, exception thrown: " + ex.Message);
                return false;
            }
        }

        private static void WriteExcelFile(DataSet ds, SpreadsheetDocument spreadsheet, bool includeAutoFilter)
        {
            // Reset rows cache
            _rowListCache = new List<Row>();

            // Add a WorkbookPart to the document.
            WorkbookPart workbookpart = spreadsheet.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet();

            // add styles to sheet
            WorkbookStylesPart wbsp = workbookpart.AddNewPart<WorkbookStylesPart>();
            wbsp.Stylesheet = CreateStylesheet();
            wbsp.Stylesheet.Save();

            DataTable dt = ds.Tables[0];
            string worksheetName = dt.TableName;

            // Create columns calculating size of biggest text for the database column
            int numberOfColumns = dt.Columns.Count;
            Columns columns = new Columns();
            for (int colInx = 0; colInx < numberOfColumns; colInx++)
            {
                DataColumn col = dt.Columns[colInx];
                string maxText = col.ColumnName;
                foreach (DataRow dr in dt.Rows)
                {
                    string value = string.Empty;
                    if (col.DataType.FullName == "System.DateTime")
                    {
                        DateTime dtValue;
                        if (DateTime.TryParse(dr[col].ToString(), out dtValue))
                            value = dtValue.ToShortDateString();
                    }
                    else
                    {
                        value = dr[col].ToString();
                    }

                    if (value.Length > maxText.Length)
                    {
                        maxText = value;
                    }
                }
                double width = maxText.Length;
                columns.Append(CreateColumnData((uint)colInx + 1, (uint)colInx + 1, width+2));
            }
            worksheetPart.Worksheet.Append(columns);

            // Create SheetData and assign to worksheetpart
            SheetData sd = new SheetData();
            worksheetPart.Worksheet.Append(sd);

            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheet.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet()
            {
                Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = worksheetName
            };
            sheets.Append(sheet);

            // Append this worksheet's data to our Workbook, using OpenXmlWriter, to prevent memory problems
            WriteDataTableToExcelWorksheet(dt, ref worksheetPart, includeAutoFilter);

            // Save it and close it
            spreadsheet.WorkbookPart.Workbook.Save();
            spreadsheet.Close();
        }

        private static void WriteDataTableToExcelWorksheet(DataTable dt, ref WorksheetPart worksheetPart, bool includeAutoFilter)
        {
            string cellValue = "";

            int numberOfColumns = dt.Columns.Count;
            bool[] IsNumericColumn = new bool[numberOfColumns];
            bool[] IsDateColumn = new bool[numberOfColumns];

            string[] excelColumnNames = new string[numberOfColumns];
            for (int n = 0; n < numberOfColumns; n++)
                excelColumnNames[n] = GetExcelColumnName(n);

            uint rowIndex = 1;

            for (int colInx = 0; colInx < numberOfColumns; colInx++)
            {
                DataColumn col = dt.Columns[colInx];
                var cell = CreateSpreadsheetCellIfNotExist(worksheetPart.Worksheet, excelColumnNames[colInx] + rowIndex.ToString());
                cell.CellValue = new CellValue(col.ColumnName);
                cell.DataType = CellValues.String;
                cell.StyleIndex = (UInt32Value)1U;

                IsNumericColumn[colInx] = (col.DataType.FullName == "System.Decimal") || (col.DataType.FullName == "System.Int32") || (col.DataType.FullName == "System.Int64") || (col.DataType.FullName == "System.Double") || (col.DataType.FullName == "System.Single");
                IsDateColumn[colInx] = (col.DataType.FullName == "System.DateTime");
            }

            if (includeAutoFilter)
            {
                AutoFilter autoFilter1 = new AutoFilter()
                { Reference = "A1:" + excelColumnNames[numberOfColumns - 1] + "1" };
                worksheetPart.Worksheet.Append(autoFilter1);
            }

            double cellNumericValue = 0;
            foreach (DataRow dr in dt.Rows)
            {
                ++rowIndex;
                Cell cell;

                for (int colInx = 0; colInx < numberOfColumns; colInx++)
                {
                    cellValue = dr.ItemArray[colInx].ToString();
                    cellValue = ReplaceHexadecimalSymbols(cellValue);

                    // Create cell with data
                    if (IsNumericColumn[colInx])
                    {
                        //  For numeric cells, make sure our input data IS a number, then write it out to the Excel file.
                        //  If this numeric value is NULL, then don't write anything to the Excel file.
                        cellNumericValue = 0;
                        if (double.TryParse(cellValue, out cellNumericValue))
                        {
                            cellValue = cellNumericValue.ToString();
                        }
                        else
                        {
                            cellValue = null;
                        }

                        cell = CreateSpreadsheetCellIfNotExist(worksheetPart.Worksheet, excelColumnNames[colInx] + rowIndex.ToString());
                        cell.CellValue = new CellValue(cellValue);
                        cell.DataType = CellValues.Number;
                        cell.StyleIndex = (rowIndex % 2 == 0) ? (UInt32Value)0U : (UInt32Value)3U;
                    }
                    else if (IsDateColumn[colInx])
                    {
                        //  This is a date value.
                        DateTime dtValue;
                        string strValue = "";
                        if (DateTime.TryParse(cellValue, out dtValue))
                            strValue = dtValue.ToOADate().ToString(CultureInfo.InvariantCulture);

                        cell = CreateSpreadsheetCellIfNotExist(worksheetPart.Worksheet, excelColumnNames[colInx] + rowIndex.ToString());
                        cell.CellValue = new CellValue(strValue);
                        cell.DataType = new EnumValue<CellValues>(CellValues.Number);  //Date is only available in Office 2010
                        cell.StyleIndex = (rowIndex % 2 == 0) ? (UInt32Value)2U : (UInt32Value)4U;
                    }
                    else
                    {
                        //  For text cells, just write the input data straight out to the Excel file.
                        cell = CreateSpreadsheetCellIfNotExist(worksheetPart.Worksheet, excelColumnNames[colInx] + rowIndex.ToString());
                        cell.CellValue = new CellValue(cellValue);
                        cell.DataType = CellValues.String;
                        cell.StyleIndex = (rowIndex % 2 == 0) ? (UInt32Value)0U : (UInt32Value)3U;
                    }
                }
            }

            worksheetPart.Worksheet.Save();
        }

        private static string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }

        //  Convert a zero-based column index into an Excel column reference  (A, B, C.. Y, Y, AA, AB, AC... AY, AZ, B1, B2..)
        public static string GetExcelColumnName(int columnIndex)
        {
            char firstChar;
            char secondChar;
            char thirdChar;

            if (columnIndex < 26)
                return ((char)('A' + columnIndex)).ToString();

            if (columnIndex < 702)
            {
                firstChar = (char)('A' + (columnIndex / 26) - 1);
                secondChar = (char)('A' + (columnIndex % 26));
                return string.Format("{0}{1}", firstChar, secondChar);
            }

            int firstInt = columnIndex / 26 / 26;
            int secondInt = (columnIndex - firstInt * 26 * 26) / 26;

            if (secondInt == 0)
            {
                secondInt = 26;
                firstInt = firstInt - 1;
            }

            int thirdInt = (columnIndex - firstInt * 26 * 26 - secondInt * 26);
            firstChar = (char)('A' + firstInt - 1);
            secondChar = (char)('A' + secondInt - 1);
            thirdChar = (char)('A' + thirdInt);

            return string.Format("{0}{1}{2}", firstChar, secondChar, thirdChar);
        }

        private static Column CreateColumnData(UInt32 StartColumnIndex, UInt32 EndColumnIndex, double ColumnWidth)
        {
            Column column;
            column = new Column
            {
                Min = StartColumnIndex,
                Max = EndColumnIndex,
                Width = ColumnWidth,
                CustomWidth = true
            };
            return column;
        }

        // Given a cell name, parses the specified cell to get the column name.
        private static string GetColumnName(string cellName)
        {
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellName);
            return match.Value;
        }

        private static uint GetRowIndex(string cellName)
        {
            Regex regex = new Regex(@"\d+");
            Match match = regex.Match(cellName);

            return uint.Parse(match.Value);
        }

        private static IList<Row> _rowListCache = new List<Row>();

        // Given a Worksheet and a cell name, verifies that the specified cell exists.
        // If it does not exist, creates a new cell. 
        private static Cell CreateSpreadsheetCellIfNotExist(Worksheet worksheet, string cellName)
        {
            string columnName = GetColumnName(cellName);
            uint rowIndex = GetRowIndex(cellName);

            Row rowWorkSheet = rowIndex < _rowListCache.Count ? _rowListCache[(int)rowIndex] : null;
            Cell cell;

            // If the Worksheet does not contain the specified row, create the specified row.
            // Create the specified cell in that row, and insert the row into the Worksheet.
            if (rowWorkSheet == null)
            {
                Row row = new Row() { RowIndex = new UInt32Value(rowIndex) };
                cell = new Cell() { CellReference = new StringValue(cellName) };
                row.Append(cell);
                worksheet.Descendants<SheetData>().First().Append(row);
                _rowListCache.Add(row);
            }
            else
            {
                Row row = rowWorkSheet;
                cell = new Cell() { CellReference = new StringValue(cellName) };
                row.Append(cell);
            }

            return cell;
        }

        private static Stylesheet CreateStylesheet()
        {
            // Stylesheet declarion and namespace
            Stylesheet stylesheet = new Stylesheet() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x14ac" } };
            stylesheet.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            stylesheet.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");

            // List of fonts
            Fonts fontsList = new Fonts() { Count = (UInt32Value)2U, KnownFonts = true };

            // FontId=0 - Regular Excel font
            Font font0 = new Font();
            FontSize fontSize0 = new FontSize() { Val = 11D };
            Color color0 = new Color() { Theme = (UInt32Value)1U };
            FontName fontName0 = new FontName() { Val = "Calibri" };
            FontFamilyNumbering fontFamilyNumbering0 = new FontFamilyNumbering() { Val = 2 };
            FontScheme fontScheme0 = new FontScheme() { Val = FontSchemeValues.Minor };
            font0.Append(fontSize0);
            font0.Append(color0);
            font0.Append(fontName0);
            font0.Append(fontFamilyNumbering0);
            font0.Append(fontScheme0);

            // FontId=1 - Bold font for header
            Font font1 = new Font();
            Bold bold = new Bold();
            font1.Append(bold);            
            fontsList.Append(font0);
            fontsList.Append(font1);

            // List of fills
            Fills fillList = new Fills() { Count = (UInt32Value)3U };

            // FillId = 0, Normal background
            Fill fill0 = new Fill();
            PatternFill patternFill0 = new PatternFill() { PatternType = PatternValues.None };
            fill0.Append(patternFill0);
            
            // FillId = 00, Normal background
            Fill fill00 = new Fill();
            PatternFill patternFill00 = new PatternFill() { PatternType = PatternValues.None };
            fill00.Append(patternFill00);

            // FillId = 1, Light Blue for alternating cells
            Fill fill1 = new Fill();
            PatternFill patternFill1 = new PatternFill() { PatternType = PatternValues.Solid };
            ForegroundColor foregroundColor1 = new ForegroundColor() { Rgb = "dedede" };
            patternFill1.Append(foregroundColor1);
            fill1.Append(patternFill1);

            fillList.Append(fill0);
            fillList.Append(fill00);
            fillList.Append(fill1);

            // Borders style
            Borders bordersList = new Borders() { Count = (UInt32Value)1U };
            Border border1 = new Border();
            LeftBorder leftBorder1 = new LeftBorder();
            RightBorder rightBorder1 = new RightBorder();
            TopBorder topBorder1 = new TopBorder();
            BottomBorder bottomBorder1 = new BottomBorder();
            DiagonalBorder diagonalBorder1 = new DiagonalBorder();
            border1.Append(leftBorder1);
            border1.Append(rightBorder1);
            border1.Append(topBorder1);
            border1.Append(bottomBorder1);
            border1.Append(diagonalBorder1);
            bordersList.Append(border1);

            // List of cell styles formats
            CellStyleFormats cellStyleFormatsList = new CellStyleFormats() { Count = (UInt32Value)1U };
            CellFormat cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U };
            cellStyleFormatsList.Append(cellFormat);

            // Cells formats 
            CellFormats cellFormatsList = new CellFormats() { Count = (UInt32Value)5U };
            
            // StyleIndex = 0 - Regular font
            CellFormat cellFormat0 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U };

            // StyleIndex = 1 - Bold font
            CellFormat cellFormat1 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)1U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U };

            // StyleIndex = 2 - Date (Short Date)
            CellFormat cellFormat2 = new CellFormat() { NumberFormatId = (UInt32Value)14U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U };

            // StyleIndex = 3 - Light blue background (Text and Numbers)
            CellFormat cellFormat3 = new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)2U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyFill = true };

            // StyleIndex = 4 - Light blue background (Date)
            CellFormat cellFormat4 = new CellFormat() { NumberFormatId = (UInt32Value)14U, FontId = (UInt32Value)0U, FillId = (UInt32Value)2U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyFill = true };

            cellFormatsList.Append(cellFormat0);
            cellFormatsList.Append(cellFormat1);
            cellFormatsList.Append(cellFormat2);
            cellFormatsList.Append(cellFormat3);
            cellFormatsList.Append(cellFormat4);

            // Cells styles
            CellStyles cellStyleList = new CellStyles() { Count = (UInt32Value)1U };
            CellStyle cellStyle0 = new CellStyle() { Name = "Normal", FormatId = (UInt32Value)0U, BuiltinId = (UInt32Value)0U };
            cellStyleList.Append(cellStyle0);

            DifferentialFormats differentialFormats0 = new DifferentialFormats() { Count = (UInt32Value)0U };
            TableStyles tableStyles0 = new TableStyles() { Count = (UInt32Value)0U, DefaultTableStyle = "TableStyleMedium2", DefaultPivotStyle = "PivotStyleMedium9" };
            StylesheetExtensionList stylesheetExtensionList = new StylesheetExtensionList();
            StylesheetExtension stylesheetExtension1 = new StylesheetExtension() { Uri = "{EB79DEF2-80B8-43e5-95BD-54CBDDF9020C}" };
            stylesheetExtensionList.Append(stylesheetExtension1);

            stylesheet.Append(fontsList);
            stylesheet.Append(fillList);
            stylesheet.Append(bordersList);
            stylesheet.Append(cellStyleFormatsList);
            stylesheet.Append(cellFormatsList);
            stylesheet.Append(cellStyleList);
            stylesheet.Append(differentialFormats0);
            stylesheet.Append(tableStyles0);
            stylesheet.Append(stylesheetExtensionList);

            return stylesheet;
        }
    }
}
