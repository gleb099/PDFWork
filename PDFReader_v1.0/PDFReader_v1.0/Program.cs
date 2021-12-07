using System;
using System.IO;
using System.Text;
using BitMiracle.Docotic.Pdf;
using iTextSharp.text.pdf;
using Tesseract;

namespace PDFReader
{
    class Program
    {
        static void Main(string[] args)
        {
            var documentText = new StringBuilder();
            using (var pdf = new BitMiracle.Docotic.Pdf.PdfDocument("C:/Users/shevchenko/Desktop/Рамочка/test2.pdf"))
            {
                using (var engine = new TesseractEngine(@"tessdata", "rus", EngineMode.LstmOnly))
                {
                    for (int i = 0; i < pdf.PageCount; ++i)
                    {
                        if (documentText.Length > 0)
                            documentText.Append("\r\n\r\n");

                        BitMiracle.Docotic.Pdf.PdfPage page = pdf.Pages[i];
                        string searchableText = page.GetText();
                        
                        PdfPoint temp = new PdfPoint(300, 300);
                        float width = GetPageWidth("C:/Users/shevchenko/Desktop/Рамочка/test2.pdf");
                        float height = GetPageHeight("C:/Users/shevchenko/Desktop/Рамочка/test2.pdf");
                        PdfSize size = new PdfSize(width, height);
                        PdfTextBox tb1 = page.AddTextBox(temp, size);
                        tb1.Text = "asdfasdfasdf";
                        //PdfColor tc = new PdfColor();
                        tb1.FontColor = new PdfRgbColor(255, 255, 255);
                        if (!string.IsNullOrEmpty(searchableText.Trim()))
                        {
                            documentText.Append(searchableText);
                            //continue;
                        }

                        // TODO: This page is not searchable. Perform OCR here

                        PdfDrawOptions options = PdfDrawOptions.Create();
                        options.BackgroundColor = new PdfRgbColor(255, 255, 255);
                        options.HorizontalResolution = 300;
                        options.VerticalResolution = 300;

                        string pageImage = $"page_{i}.png";
                        page.Save(pageImage, options);

                        using (Pix img = Pix.LoadFromFile(pageImage))
                        {
                            using (Page recognizedPage = engine.Process(img))
                            {
                                Console.WriteLine($"Mean confidence for page #{i}: {recognizedPage.GetMeanConfidence()}");

                                string recognizedText = recognizedPage.GetText();
                                documentText.Append(recognizedText);
                            }
                        }

                        //File.Delete(pageImage);
                    }
                }
            }

            using (var writer = new System.IO.StreamWriter("C:/Users/shevchenko/Desktop/Рамочка/Res/result.txt"))
            {
                writer.Write(documentText.ToString());
            }
                
        }

        public static float GetPageHeight(string PathToPDF)
        {
            var reader = new PdfReader(PathToPDF);

            // A post script point is 0.352777778mm
            const float postScriptPoints = (float)0.352777778;

            // The height is returned in post script points from iTextSharp
            float height = reader.GetPageSizeWithRotation(1).Height * postScriptPoints;

            reader.Close();

            return height;

        }

        public static float GetPageWidth(string PathToPDF)
        {
            var reader = new PdfReader(PathToPDF);

            // A post script point is 0.352777778mm
            const float postScriptPoints = (float)0.352777778;

            // The height is returned in post script points from iTextSharp
            float width = reader.GetPageSizeWithRotation(1).Width * postScriptPoints;

            reader.Close();

            return width;

        }
    }
}
