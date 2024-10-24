using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using QRCoder;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitReceiptNoteViewModels.DOItems;
using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Drawing.Printing;

namespace Com.DanLiris.Service.Purchasing.Lib.PDFTemplates.GarmentUnitReceiptNotePDFTemplates
{
    public class DOItemsStellingPDFTemplate
    {
    
        public static MemoryStream GeneratePdfTemplate(IServiceProvider serviceProvider, List<StellingEndViewModels> viewModel)
        {
            Font header_font = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 15);
            Font normal_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 8);
            Font bold_font = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 9);

            Document document = new Document(PageSize.A5, 10, 10, 10, 10);
            //Document document = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);


            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            IdentityService identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
           

            #region Identity
            StellingEndViewModels data = viewModel.Where(x => x.QtyExpenditure == null).FirstOrDefault();
            StellingEndViewModels lastData = viewModel.Where(x => x.QtyExpenditure != null).Last();
            //PdfPTable tableMark1 = new PdfPTable(2);
            //tableMark1.SetWidths(new float[] { 2f, 4f });
            //tableMark1.WidthPercentage = 100;


            PdfPTable tableMark1 = new PdfPTable(2);
            tableMark1.SetWidths(new float[] { 3f, 4f, });
            PdfPCell cellMark1 = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_MIDDLE };

            string dataTOQr = string.Concat(data.Colour,"-",data.POSerialNumber,"-",lastData.Remaining.ToString(),lastData.Uom);
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(dataTOQr, QRCodeGenerator.ECCLevel.Q);

            Base64QRCode qrCode = new Base64QRCode(qrCodeData);
            var imgType = Base64QRCode.ImageType.Jpeg;
            string qrCodeImageAsBase64 = qrCode.GetGraphic(20);
            byte[] shippingMarkImage;
            shippingMarkImage = Convert.FromBase64String(qrCodeImageAsBase64);
            Image image = Image.GetInstance(imgb: shippingMarkImage);
       
            //if (image.Width > 60)
            //{
            //    float percentage = 0.0f;
            //    percentage = 100 / image.Width;
            //    image.ScalePercent(percentage * 100);
            //}

            image.Alignment = Element.ALIGN_RIGHT;
            image.SpacingBefore = 1f;
            image.SpacingAfter = 1f;
            image.ScaleToFit(70f, 70f);

            float xPosition = 350;
            float yPosition = 520; // Position from the bottom of the page
            image.SetAbsolutePosition(xPosition, yPosition);
            //PdfPCell _sideMarkImageCell = new PdfPCell();
            ////_sideMarkImageCell.Border = Rectangle.NO_BORDER;

            //_sideMarkImageCell.Image = image;
            //tableMark1.AddCell(_sideMarkImageCell);

            ////new PdfPCell(tableMark1);
            //tableMark1.ExtendLastRow = false;
            //tableMark1.SpacingAfter = 5f;
            //document.Add(tableMark1);
            document.Add(image);


            //cellMark1.Phrase = new Phrase("KARTU STELLING BAHAN BAKU", normal_font);
            //tableMark1.AddCell(cellMark1);
            //cellMark1.Image = image;
            //cellMark1.Image.ScaleToFit(70f, 70f);
            //cellMark1.Image.Alignment = Element.ALIGN_RIGHT;
            //tableMark1.AddCell(cellMark1);

            //document.Add(tableMark1);
            #region Header

            string titleString = "KARTU STELLING BAHAN BAKU";
            Paragraph title = new Paragraph(titleString, header_font) { Alignment = Element.ALIGN_CENTER };
            document.Add(title);

            #endregion
            PdfPTable tableIdentity = new PdfPTable(2);
            tableIdentity.SetWidths(new float[] { 3f, 4f, });
            PdfPCell cellIdentityContentLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_MIDDLE };


            //GeneratedBarcode generatedBarcode = QRCodeWriter.CreateQrCode(data.id.ToString(),500);

            //Bitmap qrCodeImage = qrCode.GetGraphic(20);



            cellIdentityContentLeft.Phrase = new Phrase("BUYER", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " +data.Buyer , normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("ARTICLE", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.Article, normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("NOMOR PO", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.POSerialNumber, normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("KOMPOSISI/KONSTRUKSI", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.Construction, normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("WARNA", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.Colour, normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("RACK / BOX", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.Rack+" / "+data.Box, normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("SUPPLIER", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.Supplier, normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("NO SURAT JALAN/INVOICE", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.DoNo, normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);

            PdfPCell cellIdentity = new PdfPCell(tableIdentity);
            tableIdentity.ExtendLastRow = false;
            tableIdentity.SpacingAfter = 10f;
            tableIdentity.SpacingBefore = 20f;
            document.Add(tableIdentity);

            #endregion

            #region tableContent1
            PdfPCell cellCenter = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 4 };
            PdfPCell cellRight = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 4 };
            PdfPCell cellLeft = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 4 };


            PdfPTable tableContent2 = new PdfPTable(9);
            tableContent2.SetWidths(new float[] { 3.5f, 3f, 3.5f, 3f, 3f, 3f, 4.5f, 3f, 2.5f });

            var cell1 = new PdfPCell(new Phrase("Masuk", bold_font));
            cell1.Colspan = 2;
            cell1.HorizontalAlignment = Element.ALIGN_CENTER;
            cell1.Padding = 3;
            tableContent2.AddCell(cell1);

            var cell2 = new PdfPCell(new Phrase("Keluar", bold_font));
            cell2.Colspan = 2;
            cell2.HorizontalAlignment = Element.ALIGN_CENTER;
            cell2.Padding = 3;
            tableContent2.AddCell(cell2 );

            var cell3 = new PdfPCell(new Phrase("Sisa", bold_font));
            cell3.Colspan = 1;
            cell3.HorizontalAlignment = Element.ALIGN_CENTER;
            cell3.Padding = 3;
            tableContent2.AddCell(cell3);

            var cell4 = new PdfPCell(new Phrase("Untuk", bold_font));
            cell4.Colspan = 2;
            cell4.HorizontalAlignment = Element.ALIGN_CENTER;
            cell4.Padding = 3;
            tableContent2.AddCell(cell4);

            var cell5 = new PdfPCell(new Phrase("User", bold_font));
            cell5.Rowspan = 2;
            cell5.HorizontalAlignment = Element.ALIGN_CENTER;
            cell5.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell5.Padding = 3;
            tableContent2.AddCell(cell5);

            var cell6 = new PdfPCell(new Phrase("Paraf", bold_font));
            cell6.Rowspan = 2;
            cell6.HorizontalAlignment = Element.ALIGN_CENTER;
            cell6.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell6.Padding = 3;
            tableContent2.AddCell(cell6);

            cellCenter.Phrase = new Phrase("Tanggal", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Qty (Mtr)", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Tanggal", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Qty (Mtr)", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Qty (Mtr)", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Ro Job", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Artikel", bold_font);
            tableContent2.AddCell(cellCenter);

            int indexItem = 0;

            foreach (var item in viewModel)
            {

                cellCenter.Phrase = new Phrase(item.ReceiptDate, normal_font);
                tableContent2.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase($"{item.Quantity}", normal_font);
                tableContent2.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase(item.ExpenditureDate, normal_font);
                tableContent2.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase($"{item.QtyExpenditure}", normal_font);
                tableContent2.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase($"{item.Remaining}", normal_font);
                tableContent2.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase($"{item.Remark}", normal_font);
                tableContent2.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase($"{item.Article}", normal_font);
                tableContent2.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase(item.User.ToLower(), normal_font);
                tableContent2.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase("", normal_font);
                tableContent2.AddCell(cellCenter);
            }

            PdfPCell cellEmpty = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 4 ,FixedHeight=17};
            
            for(int x = 0;x < 150;x++)
            {
                //cellEmpty.Phrase = new Phrase("", normal_font);
                tableContent2.AddCell(cellEmpty);
            }

            PdfPCell cellContent2 = new PdfPCell(tableContent2);
            tableContent2.ExtendLastRow = false;
            tableContent2.SpacingAfter = 20f;

           

            document.Add(tableContent2);
            #endregion

            document.Close();
            byte[] byteInfo = stream.ToArray();
            stream.Write(byteInfo, 0, byteInfo.Length);
            stream.Position = 0;

            return stream;
        }

        public static MemoryStream GenerateBarcode(IServiceProvider serviceProvider, List<StellingEndViewModels> viewModel)
        {
            Font header_font = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 15);
            Font normal_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 6);
            Font bold_font = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 8);

            Rectangle pageSize = new Rectangle(198.45f, 141.75f);
            Document document = new Document(pageSize, 2, 2, 2, 2);
            //Document document = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);


            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            IdentityService identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));


            #region Identity
            StellingEndViewModels data = viewModel.Where(x => x.QtyExpenditure == null).FirstOrDefault();
            StellingEndViewModels lastData = viewModel.Where(x => x.QtyExpenditure != null).Last();
            //PdfPTable tableMark1 = new PdfPTable(2);
            //tableMark1.SetWidths(new float[] { 2f, 4f });
            //tableMark1.WidthPercentage = 100;


            PdfPTable tableMark1 = new PdfPTable(2);
            tableMark1.SetWidths(new float[] { 3f, 4f, });
            PdfPCell cellMark1 = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_MIDDLE };

          


            PdfPTable tableIdentity = new PdfPTable(2);
            tableIdentity.SetWidths(new float[] { 1.5f, 4f, });
            PdfPCell cellIdentityContentLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_MIDDLE };

            cellIdentityContentLeft.Phrase = new Phrase("BUYER", bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.Buyer, bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("ARTICLE", bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.Article, bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("NO PO / RO", bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.POSerialNumber +" / " + data.RoNo, bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("KOMPO", bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.Construction, bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("WARNA", bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.Colour, bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("LOKASI", bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.Rack + " / " + data.Box + " / " + data.Level + " / " + data.Area, bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("SUPPLIER", bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.Supplier, bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);   
            cellIdentityContentLeft.Phrase = new Phrase("QTY TERIMA", bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.Quantity +" MTR" , bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("TGL TERIMA", bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.ReceiptDate, bold_font);
            tableIdentity.AddCell(cellIdentityContentLeft);


            PdfPCell cellIdentity = new PdfPCell(tableIdentity);
            tableIdentity.ExtendLastRow = false;
            tableIdentity.SpacingAfter = 10f;
            tableIdentity.SpacingBefore = 20f;
            document.Add(tableIdentity);
            #endregion

            #region Barcode
            //string dataTOQr = string.Concat(data.Colour, "-", data.POSerialNumber, "-", lastData.Remaining.ToString(), lastData.Uom);
            string dataTOQr = $"https://document-validator-ui.azurewebsites.net/stelling/{data.id}";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(dataTOQr, QRCodeGenerator.ECCLevel.Q);

            Base64QRCode qrCode = new Base64QRCode(qrCodeData);
            var imgType = Base64QRCode.ImageType.Jpeg;
            string qrCodeImageAsBase64 = qrCode.GetGraphic(20);
            byte[] shippingMarkImage;
            shippingMarkImage = Convert.FromBase64String(qrCodeImageAsBase64);
            Image image = Image.GetInstance(imgb: shippingMarkImage);


            image.Alignment = Element.ALIGN_RIGHT;
            image.SpacingBefore = 1f;
            image.SpacingAfter = 1f;
            image.ScaleToFit(55f, 55f);
            float xPosition = (pageSize.Width - image.ScaledWidth);
            float yPosition = 1; // Position from the bottom of the page

            // Set the position of the barcode image
            image.SetAbsolutePosition(xPosition, yPosition);
            document.Add(image);
            #endregion

            //PdfPTable Footer = new PdfPTable(1);
            //Footer.SetWidths(new float[] { 1.5f });
            //PdfPCell tableFooter = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT };

            //tableFooter.Phrase = new Phrase(DateTime.Now.ToString("dd/MM/yyyy"), bold_font);
            //Footer.AddCell(tableFooter);

            //PdfPCell cellFooter = new PdfPCell(Footer);
            //document.Add(Footer);

            // Draw a border around the entire page
            PdfContentByte canvas = writer.DirectContent;
            Rectangle pageBorder = new Rectangle(document.PageSize);

            // Set the border properties
            pageBorder.Left += document.LeftMargin;
            pageBorder.Right -= document.RightMargin;
            pageBorder.Top -= document.TopMargin;
            pageBorder.Bottom += document.BottomMargin;

            pageBorder.BorderWidth = 0.5f; // Set the border width
            pageBorder.BorderColor = BaseColor.Black; // Set the border color
            pageBorder.Border = Rectangle.BOX; // Apply border to all sides

            // Draw the border
            canvas.Rectangle(pageBorder);

            document.Close();
            byte[] byteInfo = stream.ToArray();
            stream.Write(byteInfo, 0, byteInfo.Length);
            stream.Position = 0;

            return stream;
        }
    }
}
