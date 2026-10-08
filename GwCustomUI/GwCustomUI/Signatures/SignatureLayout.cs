using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace GwCustomUI.Signatures {
    public class SignatureLayout(int id, string name, string content,
       string infoDe, string infoEn, string infoFr) {
        public int Id { get; set; } = id;
        public string Name { get; set; } = name;
        public string Content { get; set; } = content;
        public string InfoDe { get; set; } = infoDe;
        public string InfoEn { get; set; } = infoEn;
        public string InfoFr { get; set; } = infoFr;

        private readonly List<SignatureItem> _signatures = [];

        public bool HasInfo(string language) {
            var result = language.ToLower() switch {
                "de" => !string.IsNullOrEmpty(InfoDe),
                "fr" => !string.IsNullOrEmpty(InfoFr),
                _ => !string.IsNullOrEmpty(InfoEn),
            };
            return result;
        }

        public void AddSignature(SignatureItem signature) {
            if (!_signatures.Contains(signature)) {
                _signatures.Add(signature);
                signature.AddLayoutInternal(this);
            }
        }

        public void RemoveSignature(SignatureItem signature) {
            if (_signatures.Remove(signature)) {
                signature.RemoveLayoutInternal(this);
            }
        }

        //public IReadOnlyList<SignatureItem> GetSignatures() => _signatures;
        public IReadOnlyList<SignatureItem> GetSignatures(SignatureUser user) {
            List<SignatureItem> result = [];
            foreach (var signature in _signatures) {
                if (signature.Users.Contains(user)) {
                    result.Add(signature);
                }
            }
            return result;
        }

        public string GetInfo(string language) {
            string result = language.ToLower() switch {
                "de" => InfoDe,
                "fr" => InfoFr,
                _ => InfoEn,
            };
            return result;
        }
    }
}
