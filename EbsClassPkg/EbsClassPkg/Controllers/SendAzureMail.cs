using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Mail;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EbsClassPkg.Controllers {
    public class SendAzureMail {
        public static void SendMail(string toAddress, string subject, string msg) {
            Execute(toAddress, subject, msg).Wait();
        }

        private static async Task Execute(string toAddress, string subject, string msg) {
            //string apiKey = Environment.GetEnvironmentVariable("tjQKBg4lQiuZv0uPdoAesQ", EnvironmentVariableTarget.User);
            // couldn't get the previous line to work. See this link: https://github.com/sendgrid/sendgrid-csharp/issues/278
            string apiKey = "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";
            dynamic sg = new SendGridAPIClient(apiKey);

            Email from = new Email("ebshub@gmail.com");
            //string subject = "Your form is ready!";
            Email to = new Email(toAddress);
            //Content content = new Content("text/plain", "You can retrieve your file from myVault in the EBSHub.  Just look for the file named '" +
            //    link + "'");
            Content content = new Content("text/plain", msg);
            Mail mail = new Mail(from, subject, to, content);

            dynamic response = await sg.client.mail.send.post(requestBody: mail.Get());
        }
    }
}
