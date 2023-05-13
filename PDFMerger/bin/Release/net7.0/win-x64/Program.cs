using iTextSharp.text;
using iTextSharp.text.pdf;
using Serilog;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using Serilog.Events;
using System.Net;
using PDFMerger.Models;
using iTextSharp.text.pdf.parser;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            #region Create Logger
            // Get .exe file path
            string exeFilePath = Directory.GetCurrentDirectory();

            // Loger to write on Console 
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File($"{exeFilePath}\\Logs.txt")
                .CreateLogger();
            #endregion

            #region ReadAppsettings
            // Getting Data from AppSettings.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true);
            IConfiguration config = builder.Build();
            // Get appsettings.json data to foldersInfo class.
            var foldersInfo = config.GetSection("FoldersInfo").Get<FoldersInfo>();
            #endregion

            if(foldersInfo != null && !string.IsNullOrEmpty(foldersInfo.Folder1) && !string.IsNullOrEmpty(foldersInfo.Folder2) && !string.IsNullOrEmpty(foldersInfo.OutputFolder))
            {
                Log.Information("Getting Files from folder's path");
                // Get all PDF file from Folder1.
                List<string> filesInFolder1 = new List<string>();
                foreach (string file in Directory.EnumerateFiles(foldersInfo.Folder1, "*.pdf"))
                {
                    if (!string.IsNullOrEmpty(file))
                    {
                        filesInFolder1.Add(file);
                    }
                }

                // Get all PDF file from Folder2.
                List<string> filesInFolder2 = new List<string>();
                foreach (string file in Directory.EnumerateFiles(foldersInfo.Folder2, "*.pdf"))
                {
                    if (!string.IsNullOrEmpty(file))
                    {
                        filesInFolder2.Add(file);
                    }
                }

                Log.Information("Start Merging Files....");
                // Merging PDFS
                foreach (var item in filesInFolder1)
                {
                    Log.Information($"Merging {System.IO.Path.GetFileName(item)}");
                    var asd = System.IO.Path.GetFileNameWithoutExtension(item);
                    var file2 = filesInFolder2.FirstOrDefault(x => x.Contains(System.IO.Path.GetFileNameWithoutExtension(item)));
                    //var file2 = filesInFolder2.FirstOrDefault(x => x.Contains(item.Split("_")[1]));
                    if (!string.IsNullOrEmpty(file2))
                    {
                        string[] pdfFiles = { item, file2 };
                        var outputFileName = $"{foldersInfo.OutputFolder}/{System.IO.Path.GetFileName(item)}";
                        MergePDFs(pdfFiles, outputFileName);
                        Log.Information($"Merged Successfully File name : {System.IO.Path.GetFileName(item)}");
                    }
                    else
                    {
                        Log.Error($"File not fount in Folder2, FileName : {System.IO.Path.GetFileName(item)}, Folder2Value : {foldersInfo.Folder2}");
                    }
                }
            }
            else
            {
                Log.Error("Check appsettings.json Something is missing");
            }
        }
        catch(Exception ex)
        {
            Log.Error($"Error message : {ex.Message}");
        }
    }

    static void MergePDFs(string[] fileNames, string outputFileName)
    {
        try
        {
            using (FileStream stream = new FileStream(outputFileName, FileMode.Create))
            {
                Document document = new Document();
                PdfCopy pdf = new PdfCopy(document, stream);

                document.Open();

                foreach (string fileName in fileNames)
                {
                    PdfReader reader = new PdfReader(fileName);
                    pdf.AddDocument(reader);
                    reader.Close();
                }

                pdf.Close();
                document.Close();
            }
        }
        catch(Exception ex)
        {
            Log.Error(ex.Message);
        }
    }
}