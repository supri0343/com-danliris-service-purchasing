using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentUnitReceiptNoteViewModels.DOItems;

namespace Com.DanLiris.Service.Purchasing.Lib.PDFTemplates.GarmentUnitReceiptNotePDFTemplates
{
    public class DOItemsStellingPDFTemplate
    {
        public static MemoryStream GeneratePdfTemplate(IServiceProvider serviceProvider, List<StellingEndViewModels> viewModel)
        {
            Font header_font = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 15);
            Font normal_font = FontFactory.GetFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 10);
            Font bold_font = FontFactory.GetFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1250, BaseFont.NOT_EMBEDDED, 10);

            //Document document = new Document(PageSize.A4.Rotate(),10, 10, 10, 10);
            Document document = new Document(PageSize.A4, 10, 10, 10, 10);


            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            IdentityService identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));

            #region TableContent1
            PdfPCell cellCenter = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 4 };
            PdfPCell cellRight = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 4 };
            PdfPCell cellLeft = new PdfPCell() { Border = Rectangle.TOP_BORDER | Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.RIGHT_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE, Padding =  4};

            PdfPTable tableContent1 = new PdfPTable(8);
            //tableContent.SetWidths(new float[] { 4f, 2f, 2f, 2f, 2f, 2f, 2f,2f,0.5f,3.5f,2f,3.5f,2f,2f,3f, 3.5f });
            tableContent1.SetWidths(new float[] { 4f, 2f, 2f, 2f, 2f, 2f, 2f, 2f});


            var cell = new PdfPCell(new Phrase("DATA PENERIMAAN",bold_font));
            cell.Colspan = 8;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 3;
            tableContent1.AddCell(cell);

            //var cell2 = new PdfPCell(new Phrase("", bold_font));
            ////cell2.Colspan = 8;
            //tableContent.AddCell(cell2);

            //var cell3 = new PdfPCell(new Phrase("KARTU STELLING", bold_font));
            //cell3.Colspan = 7;
            //cell3.HorizontalAlignment = Element.ALIGN_CENTER;
            //cell3.Padding = 3;
            //tableContent.AddCell(cell3);


            cellCenter.Phrase = new Phrase("Nomor PO", bold_font);
            tableContent1.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("QTY", bold_font);
            tableContent1.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Satuan", bold_font);
            tableContent1.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Warna", bold_font);
            tableContent1.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Rak", bold_font);
            tableContent1.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Level", bold_font);
            tableContent1.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Box", bold_font);
            tableContent1.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Area", bold_font);
            tableContent1.AddCell(cellCenter);

            foreach (var item in viewModel)
            {
                cellCenter.Phrase = new Phrase(item.POSerialNumber, normal_font);
                tableContent1.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase($"{item.Quantity}", normal_font);
                tableContent1.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase(item.Uom, normal_font);
                tableContent1.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase(item.Colour, normal_font);
                tableContent1.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase(item.Rack, normal_font);
                tableContent1.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase(item.Level, normal_font);
                tableContent1.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase(item.Box, normal_font);
                tableContent1.AddCell(cellCenter);

                cellCenter.Phrase = new Phrase(item.Area, normal_font);
                tableContent1.AddCell(cellCenter);
            }

            PdfPCell cellContent1 = new PdfPCell(tableContent1);
            tableContent1.ExtendLastRow = false;
            tableContent1.SpacingAfter = 20f;
            document.Add(tableContent1);


            #endregion

            #region tableContent2
            //cellCenter.Phrase = new Phrase("", bold_font);
            //tableContent.AddCell(cellCenter);

            PdfPTable tableContent2 = new PdfPTable(7);
            //tableContent.SetWidths(new float[] { 4f, 2f, 2f, 2f, 2f, 2f, 2f,2f,0.5f,3.5f,2f,3.5f,2f,2f,3f, 3.5f });
            tableContent2.SetWidths(new float[] { 3.5f, 2f, 3.5f, 2f, 2f, 3f, 3.5f });

            var cell3 = new PdfPCell(new Phrase("KARTU STELLING", bold_font));
            cell3.Colspan = 7;
            cell3.HorizontalAlignment = Element.ALIGN_CENTER;
            cell3.Padding = 3;
            tableContent2.AddCell(cell3);

            cellCenter.Phrase = new Phrase("Tanggal Masuk", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Panjang (Mtr)", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Tanggal Keluar", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Panjang (Mtr)", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("Sisa", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("RO Job", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("User", bold_font);
            tableContent2.AddCell(cellCenter);

            int indexItem = 0;

            foreach (var item in viewModel)
            {

                //cellCenter.Phrase = new Phrase(item.POSerialNumber, normal_font);
                //tableContent.AddCell(cellCenter);

                //cellCenter.Phrase = new Phrase($"{item.Quantity}", normal_font);
                //tableContent.AddCell(cellCenter);

                //cellCenter.Phrase = new Phrase(item.Uom, normal_font);
                //tableContent.AddCell(cellCenter);

                //cellCenter.Phrase = new Phrase(item.Colour, normal_font);
                //tableContent.AddCell(cellCenter);

                //cellCenter.Phrase = new Phrase(item.Rack, normal_font);
                //tableContent.AddCell(cellCenter);

                //cellCenter.Phrase = new Phrase(item.Level, normal_font);
                //tableContent.AddCell(cellCenter);

                //cellCenter.Phrase = new Phrase(item.Box, normal_font);
                //tableContent.AddCell(cellCenter);

                //cellCenter.Phrase = new Phrase(item.Area, normal_font);
                //tableContent.AddCell(cellCenter);

                //cellCenter.Phrase = new Phrase("", normal_font);
                //tableContent.AddCell(cellCenter);

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

                cellCenter.Phrase = new Phrase(item.User, normal_font);
                tableContent2.AddCell(cellCenter);
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
    }
}
