using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Megatec.MasterWeb.ReportLogic;
using Megatec.MasterTour.BusinessRules;
using System.Collections;
using System.IO;
using InnaTourWebService.Models;

using PerpetuumSoft.Reporting.Components;
using PerpetuumSoft.Reporting.Export.Excel;
using PerpetuumSoft.Reporting.Export.Html;
using PerpetuumSoft.Reporting.Export.Pdf;
using PerpetuumSoft.Reporting.Export.Rtf;
using PerpetuumSoft.Reporting.Export.Graph;
using PerpetuumSoft.Reporting.DOM;
using System.ComponentModel;

using System.Collections.Specialized;

namespace InnaTourWebService.DataBase
{
    public class ReportsGenerator
    {
        private IContainer components;
        private ReportManager reportManager;
        private InlineReportSlot reportSlot;


        public bool ContainsTrial(Document doc)
        {
            FindTextOptions options = new FindTextOptions();
            for (int i = 0; i < doc.Pages.Count; i++)
            {
                for (int j = 0; j < doc.Pages[i].Controls.Count; j++)
                {
                    if (doc.Pages[i].Controls[j].FindText("(TRIAL)", options))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public byte[] GenerateReport(Dogovor dogovor, Guid reportGuid, Hashtable data, FileType fileType)
        {
            this.components = new Container();
            this.reportManager = new ReportManager(this.components);
            this.reportSlot = new InlineReportSlot(this.components);
            this.reportManager.Reports.Add(this.reportSlot);


            data["DogovorCode"] = dogovor.Code;

            Dogovors dogs = new Dogovors(new DataCache());
            dogs.RowFilter = "dg_key = " + dogovor.Key ;
            dogs.Fill();

            ReportDataManager manager = new ReportDataManager();

            NameValueCollection col = new NameValueCollection();
            foreach(object key in data.Keys)
                col.Add(key.ToString(), data[key].ToString());

           // manager.FillData(reportGuid.ToString(), null, col);

            DataCache dataContainer = new DataCache();

            Rep_profiles _profiles = new Rep_profiles(dataContainer)
            {
                RowFilter = string.Format("RP_GUID = '{0}'", reportGuid.ToString())
            };
            _profiles.Fill();

            ReportTemplates templates = new ReportTemplates(dataContainer)
            {
                RowFilter = string.Format("RT_PRKEY = {0}", _profiles[0].Key)
            };
            templates.Fill();

            this.reportManager = manager.ReportManager;
            this.reportManager.DataSources["DogovorCode"] = dogovor.Code;
            this.reportManager.DataSources["Dogovors"] = dogs;
            this.reportManager.DataSources["Manager"] = "Creator";
            this.reportManager.DataSources["FormatDate"] = "dd.MM.yy";
            manager.ReportSlot.DocumentStream = templates[0].Template;

            this.reportSlot = manager.ReportSlot;

            MemoryStream stream = new MemoryStream();
            
            PerpetuumSoft.Reporting.DOM.Document doc = manager.ReportSlot.RenderDocument();

            while (ContainsTrial(doc))
                doc = this.reportSlot.RenderDocument();

               if (fileType == FileType.html)
               {
                 string fileContent;
                 HtmlExportFilter htmlFilter = new HtmlExportFilter();
                 string virtualPath = AppDomain.CurrentDomain.BaseDirectory + @"/temp/";

                 if (!Directory.Exists(virtualPath))
                    Directory.CreateDirectory(virtualPath);

                 Directory.SetCurrentDirectory(virtualPath);

                 htmlFilter.ImagesFormat = ImagesFormat.Jpeg;

                 string fileName = "report_" + new Random().Next(1, 1000000).ToString() + ".html";

                 htmlFilter.Export(doc, virtualPath + fileName, false);
                 
                 using (StreamReader reader = new StreamReader(fileName))
                   fileContent = reader.ReadToEnd();
                
                 File.Delete(fileName);

                 return GetBytes(fileContent);
               }

               switch(fileType)
               {
                   case FileType.pdf:
                        new PdfExportFilter().Export(doc, stream);
                       break;
                   case FileType.rtf:
                        new RtfExportFilter().Export(doc, stream);
                       break;
                    case FileType.xls:
                        new ExcelExportFilter().Export(doc, stream);
                       break;
               }
               return stream.ToArray();
        }

        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}