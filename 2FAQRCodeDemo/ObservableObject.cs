using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using _2FAQRCodeDemo.Annotations;

namespace _2FAQRCodeDemo
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual bool UpdateOnPropertyChanged<T>(ref T variable, T value, [CallerMemberName] string propertyName = null) where T : IEquatable<T>
        {
            if (variable == null && value == null) return false;
            if (variable == null || value == null || !value.Equals(variable))
            {
                variable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void UpdateCommandOnPropertyChanged<T>(ref T variable, T value, [CallerMemberName] string propertyName = null) where T : IEquatable<T>
        {
            if (variable == null && value == null) return;
            if (variable == null || value == null || !value.Equals(variable))
            {
                variable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void UpdateReferenceOnPropertyChanged<T>(ref T variable, T value, [CallerMemberName] string propertyName = null) where T : class
        {
            if (!ReferenceEquals(value, variable))
            {
                variable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual bool UpdateEnumOnPropertyChanged<T>(ref T variable, T value, [CallerMemberName] string propertyName = null) where T : Enum
        {
            if (value.CompareTo(variable) != 0)
            {
                variable = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
    }
}
