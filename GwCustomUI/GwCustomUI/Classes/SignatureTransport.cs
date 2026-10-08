using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GwCustomUI.Classes {
    public class SignatureTransport(string original) {
        public string Original { get; } = original;

        public string Modified { get; set; } = string.Empty;

        public bool Storage {  get; set; } = false;
    }
}
