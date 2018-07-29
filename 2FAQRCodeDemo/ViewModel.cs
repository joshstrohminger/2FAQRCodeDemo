using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Web;
using System.Windows.Input;
using OtpNet;
using _2FAQRCodeDemo.Annotations;

namespace _2FAQRCodeDemo
{
    public class ViewModel : ObservableObject
    {
        private string _result;
        private string _encodedSecret;
        private string _uri;
        private Totp _totp;
        private string _issuer = "Some Company";
        private string _user = "josh";
        private ushort _digits = 6;
        private ushort _period = 30;
        private OtpHashMode _algorithm;
        private ushort _allowedPreviousCodes = 1;
        private ushort _allowedFutureCodes = 1;
        private TimeSpan _timeOffset = TimeSpan.Zero;
        private byte[] _secret;

        public ICommand GenerateKey { get; }
        public ICommand VerifyCode { get; }

        public TimeSpan TimeOffset
        {
            get => _timeOffset;
            set => UpdateAndBuildOnPropertyChanged(ref _timeOffset, value);
        }

        public ushort AllowedPreviousCodes
        {
            get => _allowedPreviousCodes;
            set => UpdateAndBuildOnPropertyChanged(ref _allowedPreviousCodes, value);
        }

        public ushort AllowedFutureCodes
        {
            get => _allowedFutureCodes;
            set => UpdateAndBuildOnPropertyChanged(ref _allowedFutureCodes, value);
        }

        public OtpHashMode Algorithm
        {
            get => _algorithm;
            set => UpdateEnumAndBuildOnPropertyChanged(ref _algorithm, value);
        }

        public ushort Digits
        {
            get => _digits;
            set => UpdateAndBuildOnPropertyChanged(ref _digits, value);
        }

        public ushort Period
        {
            get => _period;
            set => UpdateAndBuildOnPropertyChanged(ref _period, value);
        }

        public string Issuer
        {
            get => _issuer;
            set => UpdateAndBuildOnPropertyChanged(ref _issuer, value);
        }

        public string User
        {
            get => _user;
            set => UpdateAndBuildOnPropertyChanged(ref _user, value);
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
            private set => UpdateAndBuildOnPropertyChanged(ref _encodedSecret, value);
        }

        public ViewModel()
        {
            GenerateKey = new RelayCommand(GenerateKeyExecute);
            VerifyCode = new RelayCommand<string>(VerifyCodeFromUser);

            GenerateKey.Execute(null);
        }

        private void GenerateKeyExecute()
        {
            _secret = KeyGeneration.GenerateRandomKey(10);
            EncodedSecret = Base32Encoding.ToString(_secret);
        }

        private void BuildTotpAndUri()
        {
            try
            {
                var encodedIssuer = HttpUtility.UrlPathEncode(Issuer);
                var encodedUser = HttpUtility.UrlPathEncode(User);
                var coercedDigits = Digits.LimitRange(1, 10);
                var coercedPeriod = Period.LimitRange(1, 60);

                _totp = new Totp(_secret, coercedPeriod, Algorithm, coercedDigits, new TimeCorrection(DateTime.UtcNow.Add(TimeOffset)));
                Uri = $"otpauth://totp/{encodedIssuer}:{encodedUser}?secret={EncodedSecret}&issuer={encodedIssuer}&algorithm={Algorithm}&digits={coercedDigits}&period={coercedPeriod}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Result = e.Message;
                Uri = string.Empty;
            }
        }

        private void VerifyCodeFromUser(string code)
        {
            var utc = DateTime.UtcNow;
            try
            {
                _totp.VerifyTotp(utc, code, out var match,
                    new VerificationWindow(AllowedPreviousCodes, AllowedFutureCodes));
                Result = $"{utc.ToLocalTime()}: {match}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Result = e.Message;
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void UpdateAndBuildOnPropertyChanged<T>(ref T variable, T value, [CallerMemberName] string propertyName = null) where T : IEquatable<T>
        {
            if (UpdateOnPropertyChanged(ref variable, value, propertyName))
            {
                BuildTotpAndUri();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void UpdateEnumAndBuildOnPropertyChanged<T>(ref T variable, T value, [CallerMemberName] string propertyName = null) where T : Enum
        {
            if (UpdateEnumOnPropertyChanged(ref variable, value, propertyName))
            {
                BuildTotpAndUri();
            }
        }
    }
}
