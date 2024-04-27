using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace WebBanHangOnline.Helper
{
    public class SendMail
    {
        public static bool SenMail(string to, string subject, string body)
        {
            try
            {
                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(ConstanHelper.emailSender);
                msg.To.Add(to);
                msg.Subject = subject;
                msg.IsBodyHtml = true;
                msg.Body = body;


                SmtpClient client = new SmtpClient();

                client.Port = 587;
                client.Host = ConstanHelper.hostEmail;
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(ConstanHelper.emailSender, ConstanHelper.passwordSender);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(msg);

                
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}