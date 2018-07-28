using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Windows.Input;
using OtpNet;
using _2FAQRCodeDemo.Annotations;

namespace _2FAQRCodeDemo
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum HashAlgorithm
    {
        [UsedImplicitly] SHA1,
        [UsedImplicitly] SHA256,
        [UsedImplicitly] SHA512
    }

    public class ViewModel : ObservableObject
    {
        private string _result;
        private string _encodedSecret;
        private string _uri;
        private Totp _totp;
        private string _issuer = "Some Company";
        private string _user = "josh";
        private int _digits = 6;
        private int _period = 30;
        private HashAlgorithm _algorithm;

        public ICommand Generate { get; }
        public ICommand VerifyCode { get; }

        public HashAlgorithm Algorithm
        {
            get => _algorithm;
            set => UpdateEnumOnPropertyChanged(ref _algorithm, value);
        }

        public int Digits
        {
            get => _digits;
            set => UpdateOnPropertyChanged(ref _digits, value);
        }

        public int Period
        {
            get => _period;
            set => UpdateOnPropertyChanged(ref _period, value);
        }

        public string Issuer
        {
            get => _issuer;
            set => UpdateCommandOnPropertyChanged(ref _issuer, value);
        }

        public string User
        {
            get => _user;
            set => UpdateCommandOnPropertyChanged(ref _user, value);
        }

        public string Uri
        {
            get => _uri;
            private set => UpdateOnPropertyChanged(ref _uri, value);
        }

        public string Result
        {
            get => _result;
            private set => UpdateOnPropertyChanged(ref _result, value);
        }

        public string EncodedSecret
        {
            get => _encodedSecret;
            private set => UpdateOnPropertyChanged(ref _encodedSecret, value);
        }

        public ViewModel()
        {
            Generate = new RelayCommand(GenerateExecute, GenerateCanExecute);
            VerifyCode = new RelayCommand<string>(VerifyCodeFromUser);

            GenerateExecute();
        }

        private bool GenerateCanExecute()
        {
            return !string.IsNullOrWhiteSpace(Issuer) && !string.IsNullOrWhiteSpace(User);
        }

        private void GenerateExecute()
        {
            if (!GenerateCanExecute()) return;

            var key = KeyGeneration.GenerateRandomKey(10);
            var encodedIssuer = HttpUtility.UrlPathEncode(Issuer);
            var encodedUser = HttpUtility.UrlPathEncode(User);
            var coercedDigits = Digits.LimitRange(6, 8);
            var coercedPeriod = Period.LimitRange(5, 60);

            EncodedSecret = Base32Encoding.ToString(key);
            Uri = $"otpauth://totp/{encodedIssuer}:{encodedUser}?secret={EncodedSecret}&issuer={encodedIssuer}&algorithm={Algorithm}&digits={coercedDigits}&period={coercedPeriod}";
            _totp = new Totp(key);
        }

        private void VerifyCodeFromUser(string code)
        {
            var date = DateTime.UtcNow;
            Result = $"{date.ToLocalTime()}{Environment.NewLine}{_totp.VerifyTotp(date, code, out var match, new VerificationWindow(1, 1))} {match}";
        }
    }
}
