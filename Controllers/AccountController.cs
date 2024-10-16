using Google.Authenticator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OtpExample.Models;
using System.Data;
using System.Security.Cryptography;

namespace OtpExample.Controllers
{
    public class AccountController : Controller
    {
        private static readonly Dictionary<string, string> userSecrets = new Dictionary<string, string>();

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string email)
        {
            byte[] key = new byte[10];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            string secretKey = Base32Encoding.ToString(key);

            userSecrets[email] = secretKey;

            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            SetupCode setupInfo = tfa.GenerateSetupCode("OTP Example", email, secretKey, false, 3);

            return View("RegisterQR",
                new RegisterQRModel()
                {
                    Email = email,
                    Key = secretKey,
                    QRImageUrl = setupInfo.QrCodeSetupImageUrl,
                    ManualEntryKey = setupInfo.ManualEntryKey
                });
        }

        [HttpGet]
        public IActionResult RegisterQR()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegisterQR(string code)
        {
            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            bool result = tfa.ValidateTwoFactorPIN(code, Request.Form["txtCode"]);

            if (result)
            {
                return View("RegisterQR");
            }
            else
            {
                return Redirect("RegisterQR");
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string otp)
        {
            if (userSecrets.ContainsKey(email))
            {
                var secretKey = userSecrets[email];
                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();

                bool isValid = tfa.ValidateTwoFactorPIN(secretKey, otp);

                if (isValid)
                {
                    ViewBag.Message = "Авторизация прошла успешно";
                }
                else
                {
                    ViewBag.Message = "Неверный код";
                }
            }
            else
            {
                ViewBag.Message = "Пользователь не найден";
            }

            return View("LoginResult");
        }

        [HttpPost]
        public IActionResult ValidateCode(string email, string otp)
        {
            if (userSecrets.ContainsKey(email))
            {
                var secretKey = userSecrets[email]; 
                TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();

                bool isValid = tfa.ValidateTwoFactorPIN(secretKey, otp);

                if (isValid)
                {
                    ViewBag.Message = "Код успешно подтвержден!";
                }
                else
                {
                    ViewBag.Message = "Неверный код. Попробуйте снова.";
                }
            }
            else
            {
                ViewBag.Message = "Пользователь не найден.";
            }

            return View("ValidationResult");
        }
    }
}

