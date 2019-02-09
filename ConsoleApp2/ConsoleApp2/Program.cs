using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    using System.IO;
    using System.Text.RegularExpressions;

    public class CsvConfig
    {
        public char Delimiter { get; private set; }
        public string NewLineMark { get; private set; }
        public char QuotationMark { get; private set; }

        public CsvConfig(char delimiter, string newLineMark, char quotationMark)
        {
            Delimiter = delimiter;
            NewLineMark = newLineMark;
            QuotationMark = quotationMark;
        }

        // useful configs

        public static CsvConfig Default
        {
            get { return new CsvConfig(',', "\r\n", '\"'); }
        }

        // etc.
    }
    public class CsvReader
    {
        private CsvConfig m_config;

        public CsvReader(CsvConfig config = null)
        {
            if (config == null)
                m_config = CsvConfig.Default;
            else
                m_config = config;
        }

        public IEnumerable<string[]> Read(string csvFileContents)
        {
            using (StringReader reader = new StringReader(csvFileContents))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;
                    yield return ParseLine(line);
                }
            }
        }

        private string[] ParseLine(string line)
        {
            Stack<string> result = new Stack<string>();

            int i = 0;
            while (true)
            {
                string cell = ParseNextCell(line, ref i);
                if (cell == null)
                    break;
                result.Push(cell);
            }

            // remove last elements if they're empty
            while (string.IsNullOrEmpty(result.Peek()))
            {
                result.Pop();
            }

            var resultAsArray = result.ToArray();
            Array.Reverse(resultAsArray);
            return resultAsArray;
        }

        // returns iterator after delimiter or after end of string
        private string ParseNextCell(string line, ref int i)
        {
            if (i >= line.Length)
                return null;

            if (line[i] != m_config.QuotationMark)
                return ParseNotEscapedCell(line, ref i);
            else
                return ParseEscapedCell(line, ref i);
        }

        // returns iterator after delimiter or after end of string
        private string ParseNotEscapedCell(string line, ref int i)
        {
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                if (i >= line.Length) // return iterator after end of string
                    break;
                if (line[i] == m_config.Delimiter)
                {
                    i++; // return iterator after delimiter
                    break;
                }
                sb.Append(line[i]);
                i++;
            }
            return sb.ToString();
        }

        // returns iterator after delimiter or after end of string
        private string ParseEscapedCell(string line, ref int i)
        {
            i++; // omit first character (quotation mark)
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                if (i >= line.Length)
                    break;
                if (line[i] == m_config.QuotationMark)
                {
                    i++; // we're more interested in the next character
                    if (i >= line.Length)
                    {
                        // quotation mark was closing cell;
                        // return iterator after end of string
                        break;
                    }
                    if (line[i] == m_config.Delimiter)
                    {
                        // quotation mark was closing cell;
                        // return iterator after delimiter
                        i++;
                        break;
                    }
                    if (line[i] == m_config.QuotationMark)
                    {
                        // it was doubled (escaped) quotation mark;
                        // do nothing -- we've already skipped first quotation mark
                    }

                }
                sb.Append(line[i]);
                i++;
            }

            return sb.ToString();
        }
    }
    class Program
    {
       

        static void Main(string[] args)
        {
            DataTable dtContent = new DataTable();
            dtContent.TableName = "TextFileContent";
            //Regex rexRunOnLine = new Regex(@"^[^""]*(?:""[^""]*""[^""]*)*""[^""]*$");
            Int64 rows = -1;
            bool isColumn = true;
            string separatorChar = ",";
            using (StreamReader file = new StreamReader(@"C:\test.txt"))
            {
                if (file != null)
                {

                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        Regex regx = new Regex(separatorChar + "(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                        string[] fields = regx.Split(line); //result;// result.Split(new char[] { ',' });
                        if (fields.Length > 1)
                        {
                            if (rows == -1)
                                rows = fields.Length;
                            DataRow dr = dtContent.NewRow();
                            for (int rowsCount = 0; rowsCount < rows; rowsCount++)
                            {
                                if (isColumn)
                                {
                                    DataColumn dcolColumn = new DataColumn(fields[rowsCount], typeof(string));
                                    dtContent.Columns.Add(dcolColumn);
                                }
                                else
                                {
                                    dr[rowsCount] = fields[rowsCount].Trim('"');
                                }
                            }
                            if (!isColumn)
                            {
                                dtContent.Rows.Add(dr);
                                dtContent.AcceptChanges();
                            }
                            isColumn = false;
                        }
                    }
                }
            }

            //var reader = new CsvReader(new CsvConfig(',', "\r\n",' ') { });
            ////string csv = File.ReadAllText(@"C:\test.txt");
            //string csv = File.ReadAllText(@"C:\PO_VENDORS.txt");
            ////bool isColumn = true;PO_VENDORS
            //foreach (var fields in reader.Read(csv))
            //{
            //    // so something with the whole row
            //    //foreach (var cell in row)
            //    //{
            //    //    Console.WriteLine("RowsIndex" + row + " Value " + cell);
            //    //    // do something with the cell
            //    //}
            //    if (rows == -1)
            //        rows = fields.Length;
            //    DataRow dr = dtContent.NewRow();
            //    for (int rowsCount = 0; rowsCount < rows; rowsCount++)
            //    {
            //        if (isColumn)
            //        {
            //            DataColumn dcolColumn = new DataColumn(fields[rowsCount], typeof(string));
            //            dtContent.Columns.Add(dcolColumn);
            //        }
            //        else
            //        {
            //            dr[rowsCount] = fields[rowsCount];
            //        }
            //    }
            //    if (!isColumn)
            //    {
            //        dtContent.Rows.Add(dr);
            //        dtContent.AcceptChanges();
            //    }
            //    isColumn = false;
            //}

            //List<string> myValues = new List<string>();

            //string line;

            //StreamReader file = new StreamReader(@"C:\PO_VENDORS.txt").ReadToEnd();

            //if ((line = file.ReadLine()) != null)
            //{
            //    string[] fields = line.Split('|');
            //    if(fields != null && fields.Any())
            //    {
            //        DataTable dtContent = new DataTable();
            //        dtContent.TableName = "TextFileContent";
            //        //foreach (var col in fields)
            //        //{
            //        //    DataColumn dcolColumn = new DataColumn(col, typeof(string));
            //        //    dtContent.Columns.Add(dcolColumn);
            //        //}
            //        Int64 rows = fields.Length;

            //        for (int rowsCount=0; rowsCount<rows; rowsCount++)
            //        {
            //            //if (rowsCount == 0)
            //            //{
            //                DataColumn dcolColumn = new DataColumn(fields[0], typeof(string));
            //                dtContent.Columns.Add(dcolColumn);
            //            //}
            //            //else
            //            //{
            //            //    DataRow dr = dtContent.NewRow();
            //            //    dtContent.Rows.Add();
            //            //}

            //        }
            //    }
            //using (SqlConnection con = new SqlConnection(@"server=localhost;database=TestDataBase;uid=biswapm;connection timeout=30;Trusted_Connection=Yes;Integrated Security=SSPI"))
            //{

            //    con.Open();
            //    SqlCommand cmd = new SqlCommand("INSERT INTO Products(Product_Name, Product_Price, RstName) VALUES (@Product_Name, @Product_Price, @RstName)", con);
            //    cmd.Parameters.AddWithValue("@Product_Name", fields[0].ToString());
            //    cmd.Parameters.AddWithValue("@Product_Price", fields[1].ToString());
            //    cmd.Parameters.AddWithValue("@RstName", fields[2].ToString());
            //    cmd.ExecuteNonQuery();
            //}
        }
    }
}

