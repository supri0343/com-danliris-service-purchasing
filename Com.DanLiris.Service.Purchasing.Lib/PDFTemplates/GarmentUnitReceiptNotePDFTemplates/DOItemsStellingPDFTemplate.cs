using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using System;
using System.Linq;
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

            Document document = new Document(PageSize.A4, 10, 10, 10, 10);
            //Document document = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);


            MemoryStream stream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, stream);
            document.Open();

            IdentityService identityService = (IdentityService)serviceProvider.GetService(typeof(IdentityService));
            #region Header

            string titleString = "KARTU STELLING BAHAN BAKU";
            Paragraph title = new Paragraph(titleString, header_font) { Alignment = Element.ALIGN_CENTER };
            document.Add(title);

            #endregion

            #region Identity
            PdfPTable tableIdentity = new PdfPTable(2);
            tableIdentity.SetWidths(new float[] { 3f, 4f, });
            PdfPCell cellIdentityContentLeft = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_MIDDLE };
            StellingEndViewModels data = viewModel.Where(x => x.QtyExpenditure == null).FirstOrDefault();
            cellIdentityContentLeft.Phrase = new Phrase("BUYER", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " +data.Buyer , normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("ARTICLE", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.Article, normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("KOMPOSISI/KONSTRUKSI", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.Construction, normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase("WARNA", normal_font);
            tableIdentity.AddCell(cellIdentityContentLeft);
            cellIdentityContentLeft.Phrase = new Phrase(": " + data.Colour, normal_font);
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
            tableContent2.SetWidths(new float[] { 3.5f, 2f, 3.5f, 2f, 2f, 3f, 4.5f, 3f, 2.5f });

            var cell1 = new PdfPCell(new Phrase("MASUK", bold_font));
            cell1.Colspan = 2;
            cell1.HorizontalAlignment = Element.ALIGN_CENTER;
            cell1.Padding = 3;
            tableContent2.AddCell(cell1);

            var cell2 = new PdfPCell(new Phrase("KELUAR", bold_font));
            cell2.Colspan = 2;
            cell2.HorizontalAlignment = Element.ALIGN_CENTER;
            cell2.Padding = 3;
            tableContent2.AddCell(cell2 );

            var cell3 = new PdfPCell(new Phrase("SISA", bold_font));
            cell3.Colspan = 1;
            cell3.HorizontalAlignment = Element.ALIGN_CENTER;
            cell3.Padding = 3;
            tableContent2.AddCell(cell3);

            var cell4 = new PdfPCell(new Phrase("UNTUK", bold_font));
            cell4.Colspan = 2;
            cell4.HorizontalAlignment = Element.ALIGN_CENTER;
            cell4.Padding = 3;
            tableContent2.AddCell(cell4);

            var cell5 = new PdfPCell(new Phrase("USER", bold_font));
            cell5.Rowspan = 2;
            cell5.HorizontalAlignment = Element.ALIGN_CENTER;
            cell5.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell5.Padding = 3;
            tableContent2.AddCell(cell5);

            var cell6 = new PdfPCell(new Phrase("PARAF", bold_font));
            cell6.Rowspan = 2;
            cell6.HorizontalAlignment = Element.ALIGN_CENTER;
            cell6.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell6.Padding = 3;
            tableContent2.AddCell(cell6);

            cellCenter.Phrase = new Phrase("TANGGAL", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("QTY (Mtr)", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("TANGGAL", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("QTY (Mtr)", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("QTY (Mtr)", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("RO JOB", bold_font);
            tableContent2.AddCell(cellCenter);
            cellCenter.Phrase = new Phrase("ARTICLE", bold_font);
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

                cellCenter.Phrase = new Phrase(item.User, normal_font);
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
    }
}
