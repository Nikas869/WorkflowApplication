using dBASE.NET;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace JdSuite.Common.FileProcessing
{
    public static class WorkflowFileFactory
    {
        public static WorkflowFile LoadFromXmlFile(string filePath)
        {
            CheckExtension(filePath, "xml");

            using (var fileStream = ReadFile(filePath))
            {
                return new WorkflowFile(filePath, XDocument.Load(fileStream, LoadOptions.SetLineInfo));
            }
        }

        public static WorkflowFile LoadFromCsvFile(string filePath, CsvLoadingOptions options)
        {
            CheckExtension(filePath, "csv");

            var content = new XDocument();
            content.Add(new XElement(options.XmlRootName));

            using (var fileStream = ReadFile(filePath))
            {
                using (var parser = new TextFieldParser(fileStream, options.FileEncoding))
                {
                    parser.Delimiters = new[] { options.Delimiter };

                    var headers = new List<string>();

                    // Parse headers
                    if (options.FirstLineHeaders)
                    {
                        headers.AddRange(ParserBase.GetCleanHeaders(parser.ReadFields()));
                    }

                    var firstLine = parser.ReadFields();

                    // Generate predefined headers
                    if (headers.Count == 0)
                    {
                        headers.AddRange(Enumerable.Range(0, firstLine.Length)
                            .Select(i => $"F{i}"));
                    }
                    // first line doesn't contain headers, so create Item from it
                    else
                    {
                        var node = new XElement("Item");
                        content.Root.Add(node);

                        Enumerable.Range(0, firstLine.Length)
                            .ToList()
                            .ForEach(i =>
                            {
                                node.Add(new XElement(headers[i], firstLine[i]));
                            });
                    }

                    // parse rest lines
                    while (!parser.EndOfData)
                    {
                        var fields = parser.ReadFields();

                        var node = new XElement("Item");
                        content.Root.Add(node);

                        Enumerable.Range(0, fields.Length)
                            .ToList()
                            .ForEach(i =>
                            {
                                node.Add(new XElement(headers[i], firstLine[i]));
                            });
                    }

                    return new WorkflowFile(filePath, content);
                }
            }
        }

        public static WorkflowFile LoadFromDbfFile(string filePath, DbfLoadingOptions options)
        {
            CheckExtension(filePath, "dbf");

            var content = new XDocument();
            content.Add(new XElement(options.XmlRootName));

            using (var fileStream = ReadFile(filePath))
            {
                var parser = new Dbf(options.FileEncoding);
                parser.Read(fileStream);

                // Parse headers
                var headers = ParserBase.GetCleanHeaders(parser.Fields.Select(field => field.Name).ToArray());

                for (int row = 0; row < parser.Records.Count; row++)
                {
                    var node = new XElement("Item");
                    content.Add(node);

                    for (int column = 0; column < parser.Records[row].Data.Count; column++)
                    {
                        if (parser.Records[row].Data[column] == null)
                        {
                            node.Add(new XElement(headers[column], "null"));

                            continue;
                        }

                        node.Add(new XElement(headers[column], parser.Records[row].Data[column].ToString()));
                    }
                }
            }

            return new WorkflowFile(filePath, content);
        }

        private static void CheckExtension(string filePath, string extension)
        {
            if (!string.Equals(Path.GetExtension(filePath), extension, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"File should be an .{extension} document!", nameof(filePath));
            }
        }

        private static Stream ReadFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new ApplicationException($"File {filePath} doesn't exist!");
            }

            return File.OpenRead(filePath);
        }
    }
}
