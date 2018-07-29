using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2FAQRCodeDemo
{
    public class LabeledErrorCorrection
    {
        public string Label { get; set; }
        public ErrorCorrection? Value { get; set; }

        public override string ToString() => Label;
    }
}
