using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using System.Collections.Generic;
using System.Linq;

namespace PiqyuLogo.Controllers
{
    public class PdfController : Controller
    {
        [HttpGet]
        public ActionResult ModifyPdf()
        {
            return View();
        }

        [HttpPost]

       
        public ActionResult ModifyPdf(IEnumerable<HttpPostedFileBase> folderPath, HttpPostedFileBase imageFile)
        {
            if (folderPath == null || !folderPath.Any())
                return Content("No folder or PDFs selected.");

            if (imageFile == null || imageFile.ContentLength == 0)
                return Content("No image file selected.");

            try
            {
                foreach (var file in folderPath)
                {
                    if (file.ContentType != "application/pdf")
                        continue; // Skip non-PDF files.

                    string uploadDir = Server.MapPath("~/App_Data/uploads");
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    string pdfFilePath = Path.Combine(uploadDir, Path.GetFileName(file.FileName));
                    file.SaveAs(pdfFilePath);

                    string newFilePath = Path.Combine(uploadDir, Path.GetFileNameWithoutExtension(file.FileName) + "_modified.pdf");
                    AddImageToAllPages(pdfFilePath, newFilePath, imageFile);
                }

                return Content("PDFs modified successfully. Check the uploads folder for updated files.");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }



        private void AddImageToAllPages(string inputPdfPath, string outputPdfPath, HttpPostedFileBase imageFile)
        {
            string uploadDir = Server.MapPath("~/App_Data/uploads");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            string imagePath = Path.Combine(uploadDir, Path.GetFileName(imageFile.FileName));
            imageFile.SaveAs(imagePath);

            using (PdfReader pdfReader = new PdfReader(inputPdfPath))
            using (PdfWriter pdfWriter = new PdfWriter(outputPdfPath))
            using (PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter))
            {
                Document document = new Document(pdfDocument);

                ImageData imageData = ImageDataFactory.Create(imagePath);
                Image image = new Image(imageData);

                float widthInPoints = 75f;
                float heightInPoints = 75f;

                image.ScaleToFit(widthInPoints, heightInPoints);

                int numberOfPages = pdfDocument.GetNumberOfPages();
                for (int i = 1; i <= numberOfPages; i++)
                {
                    float pageWidth = pdfDocument.GetPage(i).GetPageSize().GetWidth();
                    float pageHeight = pdfDocument.GetPage(i).GetPageSize().GetHeight();

                    float x = (pageWidth - widthInPoints) / 2; // Center horizontally
                    float y = pageHeight - heightInPoints - 20; // Top of the page with margin

                    image.SetFixedPosition(i, x, y);
                    document.Add(image);
                }

                System.IO.File.Delete(imagePath);
            }
        }
    }
}
