using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GwCustomUI.Signatures {
    public class SignatureUser(string login, string name) {
        public string Login { get; set; } = login;
        public string Name { get; set; } = name;

        public List<SignatureItem> Signatures { get; set; } = [];

        public void AddSignature(SignatureItem signature) {
            if (!Signatures.Contains(signature)) {
                Signatures.Add(signature);
            }
        }

        public IReadOnlyList<SignatureLayout> GetLayouts() {
            var layouts = new List<SignatureLayout>();
            foreach (var signature in Signatures) {
                if (signature.Id > 0) {
                    foreach (var layout in signature.GetLayouts()) {
                        layouts.Add(layout);
                    }
                }
            }
            return [.. layouts.Distinct()];
        }
    }
}