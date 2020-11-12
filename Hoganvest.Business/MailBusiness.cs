using Hoganvest.Business.Interfaces;
using Hoganvest.Core.Common;
using System;
using System.Net.Mail;

namespace Hoganvest.Business
{
    public class MailBusiness : IMailBusiness
    {
        private readonly MailSettings _mailSettings;

        public MailBusiness(MailSettings mailSettings)
        {
            _mailSettings = mailSettings;
        }
        public bool SendMail(string mailText)
        {
            bool res = false;
            try
            {
                if (!string.IsNullOrEmpty(mailText))
                {
                    MailMessage mail = new MailMessage();
                    System.Net.Mail.SmtpClient SmtpServer = new SmtpClient(_mailSettings.Host);
                    mail.From = new MailAddress(_mailSettings.FromMail);
                    if (!string.IsNullOrEmpty(_mailSettings.ToMails))
                    {
                        if (_mailSettings.ToMails.IndexOf(',') > 0)
                        {
                            for (int i = 0; i < _mailSettings.ToMails.Split(',').Length; i++)
                                mail.To.Add(_mailSettings.ToMails.Split(',')[i]);
                        }
                        else
                            mail.Bcc.Add(_mailSettings.ToMails);
                    }
                    if (!string.IsNullOrEmpty(_mailSettings.CCMails))
                    {
                        if (_mailSettings.CCMails.IndexOf(',') > 0)
                        {
                            for (int i = 0; i < _mailSettings.CCMails.Split(',').Length; i++)
                                mail.CC.Add(_mailSettings.CCMails.Split(',')[i]);
                        }
                        else
                            mail.CC.Add(_mailSettings.CCMails);
                    }
                    if (!string.IsNullOrEmpty(_mailSettings.BCCmails))
                    {
                        if (_mailSettings.BCCmails.IndexOf(',') > 0)
                        {
                            for (int i = 0; i < _mailSettings.BCCmails.Split(',').Length; i++)
                                mail.Bcc.Add(_mailSettings.BCCmails.Split(',')[i]);
                        }
                        else
                            mail.Bcc.Add(_mailSettings.BCCmails);
                    }
                    mail.Subject = _mailSettings.Subject;
                    mail.Body = mailText;
                    mail.IsBodyHtml = true;
                    SmtpServer.Port = _mailSettings.Port;
                    //  SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(_mailSettings.FromMail, _mailSettings.FromPsd);
                    SmtpServer.EnableSsl = _mailSettings.EnableSSL;
                    SmtpServer.Send(mail);

                    res = true;
                }
            }
            catch (Exception e)
            {
                res = false;
                Console.WriteLine(e.Message);
                throw e;
            }
            return res;
        }
    }
}
