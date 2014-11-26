using System.Xml.Serialization;

namespace InnaTourWebService.Models
{
    /// <summary>
    /// Сгенерированный отчет
    /// </summary>
    public class ReportResponse
    {
         [XmlAttribute("Content")]
        public byte[] content;

         [XmlAttribute("FileType")]
        public FileType type;

        public ReportResponse() { }     
    }
}