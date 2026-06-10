using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Windows.Forms;
using Aspose.Cells;
using System.Collections;

namespace SWM.Common
{
    public class SendEmail
    {
        // Gửi báo cáo định kỳ
        public static bool SendReport(string SendFrom, string EmailSendPass, DataTable RpDataTable, DataTable dtRecevier, string DisplayName)
        {
            try
            {

                ExportPDF(RpDataTable);
                ExportExcel();
                string FileAttachments = "D:\\Report.pdf";
                int FromTime = DateTime.Now.DayOfYear / 7;
                string ToTime = DateTime.Now.ToString("dd/MM/yyyy");

                string subject = "[FEE Automation_Test] Báo cáo tình trạng vận hành nhà kho FEE tính tới ngày " + DateTime.Now.ToString("dd/MM/yyyy");
                string content = @"<tr>Báo cáo các cán bộ quản lý. TA Automation xin báo cáo tình trạng vận hành nhà kho FEE tuần " + FromTime.ToString()
                                    + " đến ngày " + ToTime + " như sau.</tr> Chi tiết mời mọi người xem file đính kèm.";

                string mess = SendEmail.Send(SendFrom, EmailSendPass, subject, content, FileAttachments, dtRecevier);

                return true;

            }
            catch
            {
                return false;
            }
        }

        private static void ExportExcel()
        {
            //string TempplateFileName = "D:\\Report\\Report Template.xlsx";
            //ArrayList strSheetName = new ArrayList();

            //DataTable dtAlarm = new DataTable();
            //dtAlarm = BLReport.GetAlarmHistoryForReport();

            //// Xuất Excel
            //if (_dtReport.Rows.Count <= 0)
            //{
            //    MessageBox.Show("Không tìm thấy dữ liệu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            //if (File.Exists(TempplateFileName))
            //{
            //    try
            //    {
            //        DataSet ds = new DataSet();
            //        ds = _dtReport.DataSet;
            //        strSheetName.Add("Lịch sử lệnh vận chuyển");
            //        strSheetName.Add("Lịch sử Lỗi vận hành");

            //        Workbook wbMapping = new Workbook(TempplateFileName);
            //        Worksheet wbSheetCommandHistory = wbMapping.Worksheets[0];
            //        Worksheet wbSheetAlarmHistory = wbMapping.Worksheets[1];

            //        int x = wbSheetCommandHistory.Cells.ImportDataTable(_dtReport, true, 1, 0);
            //        int y = wbSheetAlarmHistory.Cells.ImportDataTable(dtAlarm, true, 1, 0);

            //        string filePath = "D:\\Report\\Weekly Command History Report_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx";

            //        wbMapping.Save(filePath);

            //    }
            //    catch (IOException ex)
            //    {
            //        MessageBox.Show("Không thể ghi dữ liệu tới ổ đĩa. Mô tả lỗi:" + ex.Message);
            //    }
            //    return;
            //}
        }
    

        private static string Send(string SendFrom, string PassWord, string subject, string content, string FileAttachments, DataTable dtRecevier)
        {
            try
            {
                string fileExcelPath = "D:\\Report\\Weekly Report_" + DateTime.Now.ToString("ddMMyyyy") + ".xlsx";
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com", 587);
                string MailBody = "";
                mail.From = new MailAddress(SendFrom);
                for (int i = 0; i < dtRecevier.Rows.Count; i++)
                {
                    //string sendto = "trananh260697@gmail.com";
                    mail.To.Add("chienqtqqwppw@gmail.com");
                    mail.To.Add("kysu.ngoctuan.haui@gmail.com");
                    mail.To.Add("nguyenduc120501@gmail.com");
                    mail.To.Add(dtRecevier.Rows[i]["Email"].ToString());
                }


                if (File.Exists(FileAttachments))
                {
                    Attachment dinhkem = new Attachment(FileAttachments);
                    mail.Attachments.Add(dinhkem);
                }
                if (File.Exists(fileExcelPath))
                {
                    Attachment dinhkem = new Attachment(fileExcelPath);
                    mail.Attachments.Add(dinhkem);
                }

                mail.Subject = subject;
                mail.IsBodyHtml = true;
                mail.Body = content + MailBody;
                mail.Priority = MailPriority.High;

                SmtpServer.Credentials = new System.Net.NetworkCredential(SendFrom, PassWord);
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);

                return "";
            }
            catch
            {
                return "";
            }

        }

        private static void ExportPDF(DataTable dt)
        {

            if (dt.Rows.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                sfd.FileName = "D:\\Report.pdf";
                bool fileError = false;
                if (File.Exists(sfd.FileName))
                {
                    try
                    {
                        File.Delete(sfd.FileName);
                    }
                    catch (IOException ex)
                    {
                        fileError = true;
                        MessageBox.Show("Không thể ghi dữ liệu tới ổ đĩa. Mô tả lỗi:" + ex.Message);
                    }
                }
                if (!fileError)
                {
                    try
                    {
                        PdfPTable pdfTable = new PdfPTable(dt.Columns.Count);
                        pdfTable.DefaultCell.Padding = 3;
                        pdfTable.WidthPercentage = 100;
                        pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;

                        foreach (DataColumn column in dt.Columns)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(column.ColumnName));
                            pdfTable.AddCell(cell);
                        }

                        foreach (DataRow row in dt.Rows)
                        {
                            foreach (DataColumn column in dt.Columns)
                            {
                                PdfPCell cell = new PdfPCell(new Phrase(row[column].ToString()));
                                pdfTable.AddCell(cell);
                            }
                        }

                        using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
                        {
                            Document pdfDoc = new Document(PageSize.A4.Rotate(), 10f, 20f, 20f, 10f);
                            PdfWriter.GetInstance(pdfDoc, stream);
                            pdfDoc.Open();
                            pdfDoc.Add(pdfTable);

                            pdfDoc.Close();
                            stream.Close();
                        }

                        //MessageBox.Show("Dữ liệu Export thành công!!!", "Info");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Mô tả lỗi :" + ex.Message);
                    }
                }

            }
        }
    }
}
